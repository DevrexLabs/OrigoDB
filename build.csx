using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var config = Argument("config", "Release");
var nunit = Argument("nunit", false);
var output = "./build";

// Parse the version from the assembly info.
var version = ParseAssemblyInfo("./src/SharedAssemblyInfo.cs").AssemblyVersion;
Information("Building version {0} of OrigoDB.", version);

////////////////////////////////////////////////

Task("NuGetRestore")
	.Does(() =>
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
	.WithCriteria(() => nunit)
	.Does(() =>
{
    NUnit("./src/*Tests/bin/" + config + "/*.Test.NUnit.dll");
});


Task("Tests")
	.IsDependentOn("NUnitTest");

Task("Zip")
	.IsDependentOn("Tests")
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
	.IsDependentOn("Tests")
	.Does(() =>
{
	NuGetPack("./OrigoDB.Core.nuspec", new NuGetPackSettings {
		Version = version,
		Symbols = true
   });
});

Task("ApiDocs")
  .IsDependentOn("Build")
  .Does(() => {
    XmlTransform("Documenation.xsl", @"bin\Release\OrigoDB.Core.XML", output + "/api-docs.html")
  })
  ;
Task("Default")
	.IsDependentOn("Zip")
	.IsDependentOn("NuGet")
	.IsDependentOn("Tests")
  .IsDependentOn("ApiDocs")

////////////////////////////////////////////////

RunTarget(target);
