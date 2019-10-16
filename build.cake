#addin "Cake.FileHelpers"
#addin "Cake.Git"
#tool "nsis"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var system = Argument<string>("system", System.Environment.OSVersion.Platform.ToString().StartsWith("Win") ? "win" : "mac");
var netcoreTargetFramework = Argument<string>("targetFrameworkNetCore", "netcoreapp2.1");
var netcoreTargetRuntime = Argument<string>("netcoreTargetRuntime", system=="win" ? "win-x64" : "osx-x64");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _solution = $"./RepoZ.{system}.sln";
var _appVersion = "";
var _outputDir = Directory($"_output");
var _assemblyDir = Directory($"_output/{system}/Assemblies");
	
///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
	EnsureDirectoryExists(_outputDir);
	CleanDirectory(_outputDir);
	EnsureDirectoryExists(_assemblyDir);
	CleanDirectory(_assemblyDir);
});

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
	CleanDirectories("./**/bin/" + configuration);
	CleanDirectories("./**/obj/" + configuration);
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
	Information("Restoring {0}...", _solution);
	NuGetRestore(_solution);
});

Task("SetVersion")
	.IsDependentOn("Restore")
   	.Does(() => 
	{
		var gitVersion = GitVersion(new GitVersionSettings
		{
			UpdateAssemblyInfo = true
		});

		var commits = GitLog(".", int.MaxValue);

		_appVersion = $"{gitVersion.Major}.{gitVersion.Minor}";
		var buildNumber = commits.Count.ToString();
		var fullVersion = gitVersion.AssemblySemVer;
		
		Information($"AppVersion:\t{_appVersion}");
		Information($"FullVersion:\t{fullVersion}");
		Information($"Build {buildNumber} (= number of commits in git history)");

		ReplaceRegexInFiles("./**/AssemblyInfo.*", "(?<=AssemblyBuildDate\\(\")([0-9\\-\\:T]+)(?=\"\\))", DateTime.Now.ToString("s"));
		ReplaceRegexInFiles("./**/*.csproj", "(?<=<ReleaseVersion>).*?(?=</ReleaseVersion>)", _appVersion);
		ReplaceRegexInFiles("./**/*.csproj", "(?<=<Version>).*?(?=</Version>)", fullVersion);

        // update Apple versions
        ReplaceRegexInFiles("**/Info.plist", "(?<=<key>CFBundleShortVersionString</key>\\n*\\s*<string>).*?(?=</string>)", _appVersion);
        ReplaceRegexInFiles("**/Info.plist", "(?<=<key>CFBundleVersion</key>\\n*\\s*<string>).*?(?=</string>)", buildNumber);

	});

Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
	.IsDependentOn("SetVersion")
    .Does(() =>
{
	// build the solution
	Information("Building {0}", _solution);
	MSBuild(_solution, settings =>
		settings.SetPlatformTarget(PlatformTarget.MSIL)
			.UseToolVersion(MSBuildToolVersion.VS2019)
			.WithProperty("TreatWarningsAsErrors","true")
			.WithTarget("Build")
			.SetConfiguration(configuration));
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => 
{
	if (system == "mac")
		return;

	var assemblies = new[] 
	{
		$"./Tests/bin/{configuration}/Tests.dll",
		$"./Specs/bin/{configuration}/Specs.dll"
	};
	
	var testResultsFile = MakeAbsolute(File($"{_outputDir}/TestResults.xml")).FullPath;
	var testCoverageFile = MakeAbsolute(File($"{_outputDir}/TestCoverage.xml")).FullPath;
	
	Information("Test results xml:  " + testResultsFile);
	Information("Test coverage xml: " + testCoverageFile);
	
	var openCoverSettings = new OpenCoverSettings()
		.WithFilter("+[*]*")
		.WithFilter("-[Specs]*")
		.WithFilter("-[Tests]*")
		.WithFilter("-[FluentAssertions*]*")
		.WithFilter("-[Moq*]*")
		.WithFilter("-[LibGit2Sharp*]*");
		
	openCoverSettings.ReturnTargetCodeOffset = 0;

	var nunitSettings = new NUnit3Settings
	{
		Results = new[]
		{
			new NUnit3Result { FileName = testResultsFile }
		},
		NoHeader = true,
		Configuration = "Default"             
	};
	
	OpenCover(tool => tool.NUnit3(assemblies, nunitSettings),
		new FilePath(testCoverageFile),
		openCoverSettings
	);
});

Task("Publish")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.Does(() => 
{
	// copy RepoZ main app files
	CopyFiles($"RepoZ.App.{system}/bin/" + configuration + "/**/*", _assemblyDir, true);
	
	// publish netcore apps
	var settings = new DotNetCorePublishSettings
	{
		Framework = netcoreTargetFramework,
		Configuration = configuration,
		Runtime = netcoreTargetRuntime,
		SelfContained = true
	};
	DotNetCorePublish("./grr/grr.csproj", settings);
	DotNetCorePublish("./grrui/grrui.csproj", settings);
	
	// on macOS, we need to put the "tools" grr & grrui to another location, so deploy them to a subfolder here.
	// the RepoZ.app file has to be copied to "Applications" whereas the tools might go to "Application Support".
	if (system == "mac")
	{
		_assemblyDir = Directory($"{_assemblyDir}/RepoZ-CLI");
		EnsureDirectoryExists(_assemblyDir);
	}

	CopyFiles($"grr/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*", _assemblyDir, true);
	CopyFiles($"grrui/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*", _assemblyDir, true);
	
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(_assemblyDir.Path + "/*." + extension);
});

Task("CompileSetup")
	.IsDependentOn("Publish")
	.Does(() => 
{	
	if (system == "win")
	{
		// NSIS Windows Setup
		MakeNSIS("_setup/RepoZ.nsi", new MakeNSISSettings
		{
			Defines = new Dictionary<string, string>
			{
				{ "PRODUCT_VERSION", _appVersion }
			}
		});

		// Chocolatey
		ReplaceTextInFiles("_setup/choco/RepoZ.nuspec", "{PRODUCT_VERSION}", _appVersion);
		ReplaceTextInFiles("_setup/choco/tools/chocolateyinstall.ps1", "{PRODUCT_VERSION}", _appVersion);
		
		var settings = new ChocolateyPackSettings()
		{
			OutputDirectory = _outputDir,
			Authors = { "Andreas Wäscher" },
			Tags = { "repoz", "git", "repository", "development", "foss", "utilities", "productivity" },
			Version = _appVersion
		};

		ChocolateyPack("_setup/choco/RepoZ.nuspec", settings);
	}
	else
	{
		// update the pkgproj file and run packagesbuild
		ReplaceTextInFiles("_setup/RepoZ.pkgproj", "{PRODUCT_VERSION}", _appVersion);
		StartProcess("packagesbuild", "--verbose _setup/RepoZ.pkgproj");
	}
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("CompileSetup");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);