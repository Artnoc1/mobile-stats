
const string solution = "./MobileStats.sln";

var target = Argument("target", "Default");

private void BuildSolution(string configuration)
{
    var buildSettings = new MSBuildSettings 
    {
        Verbosity = Verbosity.Minimal,
        Configuration = configuration
    }.WithProperty("Platform", "Any Cpu");

    MSBuild(solution, buildSettings);
}

private void Run()
{
    var args = "./bin/Debug/MobileStats.exe";
    var result = StartProcess("mono", new ProcessSettings { Arguments = args });

    if (result == 0) return;

    throw new Exception("An error occured while running statistics.");
}


Task("Clean")
    .Does(() => 
    {
        CleanDirectory("./bin");
        CleanDirectory("./obj");
    });

Task("Nuget")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore(solution));

Task("Build")
    .IsDependentOn("Nuget")
    .Does(() => BuildSolution("Debug"));

Task("Run")
    .IsDependentOn("Build")
    .Does(() => Run());

Task("Default")
    .IsDependentOn("Run");

RunTarget(target);