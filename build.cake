#addin "Cake.FileHelpers"
#tool "nsis"
#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var system = Argument<string>("system", "win");
var netcoreTargetFramework = Argument<string>("targetFrameworkNetCore", "netcoreapp2.1");
var netcoreTargetRuntime = Argument<string>("netcoreTargetRuntime", system=="win" ? "win-x64" : "osx-x64");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _solution = $"./RepoZ.{system}.sln";
var _appVersion = "";

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

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

		_appVersion = gitVersion.MajorMinorPatch;
		Information($"GitVersion:\t{_appVersion}");

		ReplaceRegexInFiles("./**/AssemblyInfo.*", "(?<=AssemblyBuildDate\\(\")([0-9\\-\\:T]+)(?=\"\\))", DateTime.Now.ToString("s"));
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
	
	NUnit3(assemblies, new NUnit3Settings
	{
		NoResults = true
    });
});

Task("Publish")
	.IsDependentOn("Test")
	.Does(() => 
{
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
	
	// copy file to a single folder
	var outputDir = Directory($"_output/{system}");
	var assemblyDir = Directory($"_output/{system}/Assemblies");
	
	EnsureDirectoryExists(outputDir);
	CleanDirectory(outputDir);
	EnsureDirectoryExists(assemblyDir);
		
	CopyFiles($"RepoZ.App.{system}/bin/" + configuration + "/**/*.*", assemblyDir, true);
	CopyFiles($"grr/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*", assemblyDir, true);
	CopyFiles($"grrui/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*", assemblyDir, true);
	
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(assemblyDir.Path + "/*." + extension);
	
	Zip(assemblyDir, outputDir.Path + "/v" + _appVersion + $"-{system}-portable.zip");
});

Task("CompileSetup")
	.IsDependentOn("Publish")
	.Does(() => 
{	
	if (system == "win")
	{
		MakeNSIS("_setup/RepoZ.nsi", new MakeNSISSettings
		{
			Defines = new Dictionary<string, string>
			{
				{ "PRODUCT_VERSION", _appVersion }
			}
		});
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