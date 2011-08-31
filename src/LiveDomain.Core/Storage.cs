using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace LiveDomain.Core
{
    /// <summary>
    /// Responsible for knowing about file names, formats and directory layout.
    /// </summary>
    internal class Storage : IStorage
    {
        /*
         *  {name} 
         *     {name}.base
         *     {name}.commands
         *     {name}.config
         *     {name}.snapshots
         *        {name}.001.snapshot
         */


        /// <summary>
        /// Absolute path to the command log file
        /// </summary>
        string _commandLogFile;
        string _baseImageFile;
        string _snapshotDirectory;
        EngineSettings _settings;

	
		internal Model ReadModel()
		{
            string path = GetDataFilePath();
            return ReadModel(path);

		}

        internal Model ReadSnapshot(string name)
        {
            string path = Path.Combine(_snapshotDirectory, name);
            return ReadModel(path);
        }

        private Model ReadModel(string path)
        {
            Model model = null;
            var stream = File.OpenRead(path);
            using (stream)
            {
                model = CreateSerializer().Read<Model>(stream);
            }
            model.OnLoad();
            return model;
        }

		/// <summary>
		/// Serializes and writes the model to disc. Will throw an exception if a snapshot with the same name exists.
		/// </summary>
		/// <param name="name">The name of the snapshot, must be a valid filename</param>
		/// <param name="model">The model to save</param>      

        internal Storage(EngineSettings settings)
        {
            _settings = settings;
            string targetDirectory = settings.Path;
            if (!Path.IsPathRooted(targetDirectory)) throw new ArgumentException("Must be absolute path", "targetDirectory");
            
            //Extract last directoryname from path.
            //BUG: null or exception if target is root of drive
            string name = Regex.Match(targetDirectory, @"([^\\]+)$").Value;

            //Avoid code duplication
            Func<string,string,string,string> pathBuilder = (@base, template, id) => Path.Combine(@base, String.Format(template, id));
            
            _baseImageFile = pathBuilder.Invoke(targetDirectory, "{0}.base", name);
            _commandLogFile = pathBuilder.Invoke(targetDirectory, "{0}.commands", name);
            _snapshotDirectory = pathBuilder.Invoke(targetDirectory, "{0}.snapshots", name);
        }






		internal IEnumerable<SnapshotInfo> GetSnapshots()
		{
			return Directory.GetFiles(_snapshotDirectory)
				.Select(name => new FileInfo(Path.Combine(_snapshotDirectory, name)))
				.Select(s => new SnapshotInfo
				{
					Name = s.Name,
					Created = s.CreationTime,
					SizeOnDiscInBytes = s.Length
				}).ToArray();
		}

		public ILogWriter CreateLogWriter()
		{
			Serializer serializer = CreateSerializer();
			Stream stream = GetWriteStream(GetLogFilePath(), append: true);
			return _settings.CreateLogWriter(stream, serializer);
		}

		internal ICommandLog CreateLog()
		{
			return new CommandLog(this);
		}


        /// <summary>
        /// Write the model to the data file
        /// </summary>
        /// <param name="model"></param>
        internal void WriteModel(Model model, bool useTempFile = false)
        {
                string path = GetDataFilePath();
                string tempPath = useTempFile ? GetTempPath() : path;

                WriteModel(tempPath, model);

                if (useTempFile)
                {
                    if (File.Exists(path)) File.Delete(path);
                    File.Move(tempPath, path);
                }
        }


		/// <summary>
		/// Serializes the model to the specified path. Assumes sufficient disc space and that the file is writeable.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="model"></param>
		private void WriteModel(string path, Model model)
		{
			Stream stream = GetWriteStream(path, append: false);
			using (stream)
			{
                CreateSerializer().Write(model, stream);
			}
		}

        internal void WriteSnapshot(string name, Model model)
        {
            if (!Directory.Exists(_snapshotDirectory)) Directory.CreateDirectory(_snapshotDirectory);
            string path = Path.Combine(_snapshotDirectory, name);
            WriteModel(path, model);
        }


        public string GetDataFilePath()
        {
            return _baseImageFile;
        }

        public string GetLogFilePath()
        {
            return _commandLogFile;
        }

        public string GetTempPath()
        {
            return Path.Combine(_baseImageFile, Guid.NewGuid().ToString());
        }

		public Serializer CreateSerializer()
		{
			IFormatter formatter = _settings.CreateSerializationFormatter();
			return new Serializer(formatter);
		}

		public Stream GetWriteStream(string path, bool append)
		{
			var filemode = append ? FileMode.Append : FileMode.Create;
			Stream stream = new FileStream(path, filemode, FileAccess.Write);
			if (_settings.Compression == CompressionMethod.GZip) stream = new GZipStream(stream, CompressionMode.Compress);
			return stream;
		}

		public Stream GetReadStream(string path)
		{
			Stream stream = File.OpenRead(path);
			if (_settings.Compression == CompressionMethod.GZip) stream = new GZipStream(stream, CompressionMode.Decompress);
			return stream;
		}

		internal static Storage Create(string path)
		{
			return Create(new EngineSettings(path));
		}

		internal static Storage Create(EngineSettings settings)
		{
			string path = settings.Path;
			if (Directory.Exists(path)) throw new ArgumentException("Directory already exists", "settings.Path");
			Directory.CreateDirectory(path);
			return new Storage(settings);
		}


    }
}
