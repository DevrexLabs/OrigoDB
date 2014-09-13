using System.Text.RegularExpressions;
 
var target = Argument("target", "Default");
var config = Argument("config", "Release");
var output = "./build";
var version = ParseVersion("./src/SharedAssemblyInfo.cs");

if(version == null)
{
   // We make sure the version is set.
   throw new InvalidOperationException("Could not parse version.");
}
 
////////////////////////////////////////////////
// TASKS
////////////////////////////////////////////////
 

Task("NuGetRestore").Does(ctx =>
{
	NuGetRestore("./src/OrigoDB.sln");
});
 
Task("Build")
   .IsDependentOn("NuGetRestore")
   .Does(() =>
{
   MSBuild("./src/OrigoDB.sln", settings => 
      settings.SetConfiguration(config)
		 .UseToolVersion(MSBuildToolVersion.VS2012)
         .WithTarget("clean")
         .WithTarget("build")); 	
});

Task("NUnitTest")
	.IsDependentOn("Build")
	.Does( () =>
{
    NUnit("./src/*Tests/bin/" + config + "/*.Tests.dll");
});


Task("MSTest")
	.IsDependentOn("Build")
	.Does( () =>
{
    MSTest("./src/*Test/bin/" + config + "/*.Test.dll");
});

Task("Tests")
	.IsDependentOn("MSTest")
	.IsDependentOn("NUnitTest");
	

Task("Zip")
	.IsDependentOn("Build")
   .Does(() =>
{
	//copy stuff to build directory and create a release package
	CleanDirectory(output);

	var pattern = "src/OrigoDB.*/bin/" + config + "/OrigoDB.*";
	CopyFiles(pattern, output);

	var root = "./build/";
	var outFile = "./build/OrigoDB.Core.binaries." + version + "-" + config + ".zip";
	var files = root + "/*";
 
	// Package the bin folder.
	Zip(root, outFile);	
});
 
 
Task("NuGet")
	.IsDependentOn("Build")
.Does(() => {
   NuGetPack("./OrigoDB.Core.nuspec", new NuGetPackSettings {
      Version = version,
	  Symbols = true
   });
});

Task("Default")
	.IsDependentOn("Zip")
	.IsDependentOn("NuGet")
	.IsDependentOn("Tests");
 
////////////////////////////////////////////////
// RUN TASKS
////////////////////////////////////////////////
 
RunTarget(target);
 
////////////////////////////////////////////////
// UTILITIES
////////////////////////////////////////////////
 
private string ParseVersion(string filename)
{
   var file = FileSystem.GetFile(filename);
   using(var reader = new StreamReader(file.OpenRead()))
   {
      var text = reader.ReadToEnd();
      Regex regex = new Regex(@"AssemblyVersion\(""(?<theversionnumber>\d+\.\d+\.\d+)""\)");
      return regex.Match(text).Groups["theversionnumber"].Value;
   }
}