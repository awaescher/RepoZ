#addin "Cake.FileHelpers"
#tool "nsis"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _solutions = GetFiles("./**/*Win.sln");
var _solutionPaths = _solutions.Select(solution => solution.GetDirectory());
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
    // Clean solution directories.
    foreach(var path in _solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var solution in _solutions)
    {
        Information("Restoring {0}...", solution);
        NuGetRestore(solution);
    }
});

Task("SetVersion")
    .IsDependentOn("Restore")
   .Does(() => 
   {
       ReplaceRegexInFiles("../**/AssemblyInfo.*", 
                           "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
                           _appVersion);

       ReplaceRegexInFiles("../**/AssemblyInfo.*", 
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
    // Build all _solutions.
    foreach(var solution in _solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .WithProperty("TreatWarningsAsErrors","true")
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

Task("Deploy")
	.IsDependentOn("Build")
	.Does(() => 
{
	var assemblyDir = Directory("_output/Assemblies");
	var outputDir = Directory("_output");
	
	EnsureDirectoryExists(outputDir);
	CleanDirectory(outputDir);
	EnsureDirectoryExists(assemblyDir);
		
	CopyFiles("RepoZ.App.Win/bin/" + configuration + "/**/*.*", assemblyDir, true);
	CopyFiles("grr/bin/" + configuration + "/**/*.*", assemblyDir, true);
	
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(assemblyDir.Path + "/*." + extension);
	
	Zip(assemblyDir, outputDir.Path + "/v" + _appVersionShort + "-win-portable.zip");
});

Task("CompileWinSetup")
	.IsDependentOn("Deploy")
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