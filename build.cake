#addin "Cake.FileHelpers"
#tool "nsis"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var netcoreTargetFramework = Argument<string>("targetFrameworkNetCore", "netcoreapp2.1");
var netcoreTargetRuntime = Argument<string>("netcoreTargetRuntime", "win-x64");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _solution = "./RepoZ.Win.sln";
var _appVersion = FileReadLines("app.version").First();
var dotIndex = _appVersion.IndexOf(".");
dotIndex = _appVersion.IndexOf(".", dotIndex + 1); // find the second "." for "2.1" from "2.1.0.0"
var _appVersionShort = _appVersion.Substring(0, dotIndex);

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
       ReplaceRegexInFiles("./**/AssemblyInfo.*", 
                           "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
                           _appVersion);

       ReplaceRegexInFiles("./**/AssemblyInfo.*", 
                           "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
                           _appVersion);
						   
						   
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

Task("Publish")
	.IsDependentOn("Build")
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
	var outputDir = Directory("_output");
	var assemblyDir = Directory("_output/Assemblies");
	
	EnsureDirectoryExists(outputDir);
	CleanDirectory(outputDir);
	EnsureDirectoryExists(assemblyDir);
		
	CopyFiles("RepoZ.App.Win/bin/" + configuration + "/**/*.*", assemblyDir, true);
	CopyFiles($"grr/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*.*", assemblyDir, true);
	CopyFiles($"grrui/bin/{configuration}/{netcoreTargetFramework}/{netcoreTargetRuntime}/publish/*.*", assemblyDir, true);
	
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(assemblyDir.Path + "/*." + extension);
	
	Zip(assemblyDir, outputDir.Path + "/v" + _appVersionShort + "-win-portable.zip");
});

Task("CompileWinSetup")
	.IsDependentOn("Publish")
	.Does(() => 
{	
	MakeNSIS("_setup/RepoZ.nsi", new MakeNSISSettings
	{
		Defines = new Dictionary<string, string>
		{
			{ "PRODUCT_VERSION", _appVersionShort }
		}
	});
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("CompileWinSetup");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);