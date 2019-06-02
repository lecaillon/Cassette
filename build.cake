///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/Version/text()");

var sln = "./Cassette.sln";
var distDir = "./dist";
var distDirFullPath = MakeAbsolute(Directory($"{distDir}")).FullPath;
var publishDir = "./publish";
var publishDirFullPath = MakeAbsolute(Directory($"{publishDir}")).FullPath;
var framework = "netstandard2.0";
var logger = Environment.GetEnvironmentVariable("TF_BUILD") == "True" ? $"-l:trx --results-directory {publishDirFullPath}" : "-l:console;verbosity=normal";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx => 
{ 
    Information($"Building Cassette {version}");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean").Does(() =>
{
    CreateDirectory(distDir);
    
    CleanDirectories(distDir);
    CleanDirectories(publishDir);
    CleanDirectories($"./**/obj/{framework}");
    CleanDirectories(string.Format("./**/obj/{0}", configuration));
    CleanDirectories(string.Format("./**/bin/{0}", configuration));
});

Task("build").Does(() =>
{
    DotNetCoreBuild(".", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal
    });
});

Task("test").Does(() =>
{
    DotNetCoreTest("./test/Cassette.Tests", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append(logger)
    });
});

Task("pack").Does(() =>
{
    NuGetPack("./src/Cassette/Cassette.nuspec", new NuGetPackSettings 
    {
        OutputDirectory = distDir,
        Version = version
    });
});

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("pack");

RunTarget(target);