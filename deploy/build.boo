solution_name = "OrigoDB"
solution_dir_path = "../src"
project_name = "OrigoDb"
builds_dir_path = "builds"
build_version = "0.11.0"
build_config = "Release"
build_name = "${project_name}-v${build_version}-${build_config}"
build_dir_path = "${builds_dir_path}/${build_name}"
nuget = "nuget.exe"

target default, (clean, compile, copy, zip):
    pass

target clean:
    rm(build_dir_path)

target compile:
    msbuild(
        file: "${solution_dir_path}/${solution_name}.sln",
        targets: ("Clean", "Build"),
        configuration: build_config)
        
target copy:
    with FileList("${solution_dir_path}/${project_name}.Core/bin/${build_config}"):
        .Include("$*.{dll,xml}")
        .ForEach def(file):
            file.CopyToDirectory("${build_dir_path}")
    with FileList("${solution_dir_path}/${project_name}.Benchmark/bin/${build_config}"):
        .Include("${project_name}.*.exe")
        .ForEach def(file):
            file.CopyToDirectory("${build_dir_path}")
    with FileList("${solution_dir_path}/${project_name}.Modules.Protobuf/bin/${build_config}"):
        .Include("*.dll")
        .ForEach def(file):
            file.CopyToDirectory("${build_dir_path}")
    with FileList("${solution_dir_path}/${project_name}.Modules.SqlStorage/bin/${build_config}"):
        .Include("*.dll")
        .ForEach def(file):
            file.CopyToDirectory("${build_dir_path}")
    with FileList("${solution_dir_path}/${project_name}.StoreUtility/bin/${build_config}"):
        .Include("*.{dll,exe}")
        .ForEach def(file):
            file.CopyToDirectory("${build_dir_path}")
			
			

target test:
    pass
	
target zip:
    zip(build_dir_path, "${builds_dir_path}/${build_name}.zip")
    
target nuget_pack:
    exec(nuget, "pack ${project_name}.nuspec -version ${build_version} -basepath ${build_dir_path} -outputdirectory ${builds_dir_path}")
