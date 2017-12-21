#addin "Cake.FileHelpers"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/*Win.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
var appVersion = FileReadLines("app.version").First();

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(() =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    // Clean solution directories.
    foreach(var path in solutionPaths)
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
    foreach(var solution in solutions)
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
                           appVersion);
       ReplaceRegexInFiles("../**/AssemblyInfo.*", 
                           "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
                           appVersion);
   });

Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
	.IsDependentOn("SetVersion")
    .Does(() =>
{
    // Build all solutions.
    foreach(var solution in solutions)
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
	var outputDir = Directory("_output");
	
	EnsureDirectoryExists(outputDir);
	CleanDirectory(outputDir);
	
	CopyFiles("RepoZ.UI.Win.Wpf/bin/" + configuration + "/**/*.*", outputDir, true);
	CopyFiles("grr/bin/" + configuration + "/**/*.*", outputDir, true);
	
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(outputDir.Path + "/*." + extension);
	
	var dotIndex = appVersion.IndexOf(".");
	dotIndex = appVersion.IndexOf(".", dotIndex + 1); // find the second "." for "2.1" from "2.1.0.0"
	var shortVersion = appVersion.Substring(0, dotIndex);
	Zip(outputDir, outputDir.Path + "/v" + shortVersion + ".zip");
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("Deploy");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);