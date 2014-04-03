#!/usr/bin/perl
use strict;

#
# Build script for OrigoDB
# Note: 
# 

# tools
my $msbuild = 'C:/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe';
my $nuget = "nuget.exe";
my $sevenZip = "C:/Program Files/7-Zip/7z.exe";


my $version = extract('src/SharedAssemblyInfo.cs');
die "missing or bad version: [$version]\n" unless $version =~ /^\d+\.\d+\.\d+(-[a-z]+)?$/;

my $target = shift || 'default';
my $config = shift || 'Release';


my $solution = 'src/OrigoDB.sln';
my $output = "build";


my $x = "src/OrigoDB.*/bin/$config/";


sub run;

my $targets = {
	default => 	sub 
	{
		run qw|clean compile copy zip pack|;
	},
	clean => sub 
	{
		system "rm -fr $output";
		mkdir $output;
	},
	compile => sub 
	{
		system "$msbuild $solution -target:clean,rebuild -p:Configuration=$config -p:NoWarn=1591 > build/build.log";
	},
	copy => sub 
	{
		system("cp -r $x/OrigoDB.* $output");
		#remove test assemblies
		system("rm $output/*Test*");
	},
	zip => sub {
		system "\"$sevenZip\" a -r -tzip OrigoDB.Core.binaries.$version-$config.zip build/*";
	},
	pack => sub
	{
		system("$nuget pack OrigoDB.Core.nuspec -OutputDirectory build -version $version -symbols")
	}
};

run $target;

sub run
{
	for my $targetName (@_) {
		die "No such target: $targetName\n" unless exists $targets->{$targetName};
		print "target: $targetName\n";
		$targets->{$targetName}->();
	}
}

sub extract
{
	my $file = shift;
	`grep AssemblyVersion $file` =~ /(\d+\.\d+\.\d+)/;
	$1;
}

