using System.Text.RegularExpressions;
 
var target = Argument("target", "NuGet");
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
 
Task("Clean")
   .Does(() =>
{
   CleanDirectory(output);
});
 
Task("Build")
   .IsDependentOn("Clean")
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

/* No support for MS Test?
Task("MSTest")
	.IsDependentOn("Build")
	.Does( () =>
{
    NUnit("./src/*Test/bin/" + config + "/*.Test.dll");
});
*/

Task("Copy")
   .IsDependentOn("Build")
   .IsDependentOn("NUnitTest")
   .Does(() =>
{
   var pattern = "src/OrigoDB.*/bin/" + config + "/OrigoDB.*";
   CopyFiles(pattern, output);
});

Task("Zip")
   .IsDependentOn("Copy")
   .Does(() =>
{
   var root = "./build/";
   var output = "./build/OrigoDB.Core.binaries." + version + "-" + config + ".zip";
   var files = root + "/*";
 
   // Package the bin folder.
   Zip(root, output);	
});
 
Task("NuGet")
   .IsDependentOn("Zip")
   .Does(() =>
{
   NuGetPack("./OrigoDB.Core.nuspec", new NuGetPackSettings {
      Version = version,
      OutputDirectory = "./build",
	  Symbols = true
   });
});
 
////////////////////////////////////////////////
// RUN TASKS
////////////////////////////////////////////////
 
Run(target);
 
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