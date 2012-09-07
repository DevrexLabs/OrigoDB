using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LiveDomain.Core.Storage;
using LiveDomain.Core;
using LiveDomain.Modules.SqlStorage;
using System.IO;
using System.Reflection;

namespace LiveDb.JournalUtility
{
	internal class Program
	{
		static Model initialModel;
		static Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

		static void Main(string[] args)
		{
			//file -> file
			args = new[]
			           {
			               @"-source=c:\livedb\freedb.001", 
			               @"-destination=c:\livedb\freedb.003", 
			               "-source-type=file-v0.4", 
			               "-destination-type=file", 
			               @"-assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			           };

			//file-v0.4 -> file
			//args = new[]
			//           {
			//               @"-source=c:\livedb\freedb.001", 
			//               @"-destination=c:\livedb\freedb.003", 
			//               "-source-type=file-v0.4", 
			//               "-destination-type=file", 
			//               @"-assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };

			//file -> sql
			//args = new[]
			//           {
			//               @"-source=c:\livedb\freedb.003", 
			//               @"-destination=livedbstorage", 
			//               "-source-type=file", 
			//               "-destination-type=sql", 
			//               @"-assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll", 
			//               @"-destination-snapshots=c:\livedb\freedb.ss.001"
			//           };

			//file-v0.4 -> sql
			//args = new[]
			//           {
			//               @"-source=c:\livedb\freedb.001", 
			//               @"-destination=livedbstorage", 
			//               "-source-type=file-v0.4", 
			//               "-destination-type=sql", 
			//               @"-destination-snapshots=c:\livedb\freedb.ss.001", 
			//               @"-assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };

			//sql -> file
			//args = new[]
			//           {
			//               @"-destination=c:\livedb\freedb.004", 
			//               @"-source=livedbstorage", 
			//               "-source-type=sql", 
			//               "-destination-type=file" , 
			//               @"-source-snapshots=c:\livedb\freedb.ss.001", 
			//               @"-assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };

			var parsedArgs = ParseArgs(args);
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			LoadAssembly(parsedArgs["assembly"]);

			Store destination = CreateStore(parsedArgs["destination"], parsedArgs["destination-type"],
			                                parsedArgs["destination-snapshots"]);
			IterateSource(parsedArgs["source"], parsedArgs["source-type"], parsedArgs["source-snapshots"], destination);
		}

		static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (!_assemblies.ContainsKey(args.Name))
				throw new Exception(args.Name);

			return _assemblies[args.Name];
		}

		static void LoadAssembly(string assemblyFile)
		{
			var path = Path.GetPathRoot(assemblyFile);
			var files = Directory.GetFiles(path, "*.dll");

			foreach (var assembly in files.Select(file => Assembly.LoadFrom(path + file)))
			{
				_assemblies[assembly.FullName] = assembly;
			}
		}

		static void IterateSource(string location, string sourceType, string snapshotLocation, Store destination)
		{
			var writer = destination.CreateJournalWriter(0);

			if (sourceType == "file-v0.4")
			{
				initialModel = LoadSnapshot(Path.Combine(location, "000000000.snapshot"));
				destination.Create(initialModel);
				foreach (var journalEntry in ReadJournal_v4(location))
				{
					Console.WriteLine(journalEntry.Id);
					writer.Write(journalEntry);
				}
			}
			else if (sourceType == "file")
			{
				var config = new EngineConfiguration(location);
				config.SnapshotLocation = snapshotLocation;
				var store = new FileStore(config);
				store.Load();
				initialModel = LoadSnapshot(Path.Combine(config.SnapshotLocation, "000000000.snapshot"));
				destination.Create(initialModel);
				foreach (var journalEntry in store.GetJournalEntries())
				{
					Console.WriteLine(journalEntry.Id);
					writer.Write(journalEntry);
				}
			}
			else if (sourceType == "sql")
			{
				var config = new SqlEngineConfiguration(location);
				config.SnapshotLocation = snapshotLocation;
				var store = new SqlStore(config);
				store.Load();
				initialModel = LoadSnapshot(Path.Combine(config.SnapshotLocation, "000000000.snapshot"));
				destination.Create(initialModel);

				foreach (var journalEntry in store.GetJournalEntries())
				{
					Console.WriteLine(journalEntry.Id);
					writer.Write(journalEntry);
				}
			}
			else throw new ArgumentException("Invalid source type");
			writer.Close();
		}

		static Model LoadSnapshot(string absolutePath)
		{
			Stream stream = File.OpenRead(absolutePath);
			var serializer = new EngineConfiguration().CreateSerializer();
			var model = serializer.Read<Model>(stream);
			stream.Dispose();
			return model;
		}

		static IEnumerable<JournalEntry> ReadJournal_v4(string location)
		{
			ISerializer serializer = new EngineConfiguration().CreateSerializer();
			List<string> files = Directory.GetFiles(location, "*.journal").ToList();
			files.Sort((a, b) => a.CompareTo(b));
			long entryId = 1;
			foreach (string file in files)
			{
				Stream stream = File.OpenRead(file);
				using (stream)
				{
					foreach (JournalEntry<Command> entry in serializer.ReadToEnd<JournalEntry<Command>>(stream))
					{
						yield return new JournalEntry<Command>(entryId++, entry.Item, entry.Created);
					}
				}
			}
		}

		static Store CreateStore(string location, string type, string snapshotLocation)
		{
			if (type == "file")
			{
				var config = new EngineConfiguration(location);
				config.SnapshotLocation = snapshotLocation;
				return new FileStore(config);
			}
			else if (type == "sql")
			{
				var config = new SqlEngineConfiguration(location);
				config.SnapshotLocation = snapshotLocation;
				return new SqlStore(config);
			}
			else throw new ArgumentException("Invalid destination type");
		}

		static Dictionary<string, string> ParseArgs(string[] args)
		{
			Regex regex = new Regex("^-(?<key>.*?)=(?<val>.*)$");
			var dictionary = new Dictionary<string, string>();
			foreach (var pair in args)
			{
				Match m = regex.Match(pair);
				if (!m.Success) throw new ArgumentException("bad parameter format");
				string key = m.Groups["key"].Value;
				string val = m.Groups["val"].Value;
				if (dictionary.ContainsKey(key)) throw new ArgumentException("Duplicate parameter");
				dictionary[key] = val;
			}

			//Validate
			var required = new[] {"source", "destination", "assembly"};
			var allowed = new[]
				{
					"source", "source-type", "destination", "destination-type", "source-snapshots", "destination-snapshots", "assembly"
				};
			Validate(dictionary.Keys.ToArray(), allowed, required);

			//set defaults
			if (!dictionary.ContainsKey("source-type")) dictionary["source-type"] = "file";
			if (!dictionary.ContainsKey("destination-type")) dictionary["destination-type"] = "file";
			if (!dictionary.ContainsKey("destination-snapshots"))
				dictionary["destination-snapshots"] = dictionary["destination"];
			if (!dictionary.ContainsKey("source-snapshots")) dictionary["source-snapshots"] = dictionary["source"];

			if (dictionary["source-type"] == dictionary["destination-type"])
				throw new ArgumentException("source and destination types must be different");

			if (dictionary["source-type"] == "file-v0.4" && dictionary["destination-type"] == "file" &&
			    dictionary["source"] == dictionary["destination"])
				throw new ArgumentException(
					"source and destination cannot be the same (in-place upgrade not supported at this time)");
			return dictionary;
		}

		static void Validate(string[] keysGiven, string[] keysAllowed, string[] keysRequired)
		{
			foreach (string key in keysGiven)
			{
				if (!keysAllowed.Contains(key)) throw new ArgumentException("Invalid key:" + key);
			}

			foreach (var key in keysRequired)
			{
				if (!keysGiven.Contains(key)) throw new ArgumentException("key required: " + key);
			}
		}
	}
}