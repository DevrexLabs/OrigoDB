using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using LiveDomain.Core.Storage;
using LiveDomain.Core;
using LiveDomain.Modules.SqlStorage;
using System.IO;
using System.Reflection;
using TinyIoC;

namespace LiveDb.JournalUtility
{
	internal class Program
	{
		static Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

		static void Main(string[] args)
		{
			#region Argument tests
			//file-v0.4 -> file-v0.5
			args = new[]
				{
					@"--source=C:\db\freedb.1",
					@"--destination=C:\db\freedb.1.3",
					"--source-type=file-v0.4",
					"--destination-type=file-v0.5",
					@"--assembly=C:\Dropbox\Shared Work\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
				};


			//file-v0.4 -> file
			//args = new[]
			//           {
			//               @"--source=c:\livedb\freedb.001", 
			//               @"--destination=c:\livedb\freedb.003", 
			//               "--source-type=file-v0.4", 
			//               "--destination-type=file", 
			//               @"--assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };

			//file -> sql
			//args = new[]
			//           {
			//               @"--source=c:\livedb\freedb.003", 
			//               @"--destination=livedbstorage", 
			//               "--source-type=file-v0.5", 
			//               "--destination-type=sql", 
			//               @"--assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll", 
			//               @"--destination-snapshots=c:\livedb\freedb.ss.001"
			//           };

			//file-v0.4 -> sql
			//args = new[]
			//           {
			//               @"--source=c:\livedb\freedb.001", 
			//               @"--destination=livedbstorage", 
			//               "--source-type=file-v0.4", 
			//               "--destination-type=sql", 
			//               @"--destination-snapshots=c:\livedb\freedb.ss.001", 
			//               @"--assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };

			//sql -> file
			//args = new[]
			//           {
			//               @"--destination=c:\livedb\freedb.004", 
			//               @"--source=livedbstorage", 
			//               "--source-type=sql", 
			//               "--destination-type=file-v0.5" , 
			//               @"--source-snapshots=c:\livedb\freedb.ss.001", 
			//               @"--assembly=C:\temp\FreeDb\FreeDb.Core\bin\Debug\FreeDb.Core.dll"
			//           };
			#endregion

			// Parse and validate arguments
			var arguments = new Arguments();
			if (!CommandLine.CommandLineParser.Default.ParseArguments(args, arguments))
				throw new Exception("Arguments");
			arguments.Validate();

			// Load assembly and handle assembly resolve
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			LoadAssembly(arguments.Assembly);

			// Do the thing we do!
			var destination = CreateStore(arguments.Destination, arguments.DestinationType, arguments.DestinationSnapshots);
			TransformJournal(arguments.Source, arguments.SourceType, arguments.SourceSnapshots, destination);
		}

		static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (!_assemblies.ContainsKey(args.Name))
				throw new Exception(args.Name);

			return _assemblies[args.Name];
		}

		static void LoadAssembly(string assemblyFile)
		{
			var assembly = Assembly.LoadFrom(assemblyFile);
			_assemblies[assembly.FullName] = assembly;
		}

		static void TransformJournal(string location, string sourceType, string snapshotLocation, Store destination)
		{		
			var initialModel = LoadSnapshot(Path.Combine(snapshotLocation, "000000000.snapshot"));
			destination.Create(initialModel);

			var destinationJournal = destination.CreateJournalWriter(0);
			var sourceJournal = GetSourceJournalEntries(location, sourceType, snapshotLocation);
			foreach (var journalEntry in sourceJournal)
			{
				Console.WriteLine(journalEntry.Id);
				destinationJournal.Write(journalEntry);
			}
			destinationJournal.Close();
		}

		static IEnumerable<JournalEntry> GetSourceJournalEntries(string location, string sourceType, string snapshotLocation)
		{
			EngineConfiguration config;
			Store store;
			
			switch (sourceType)
			{
				case "file-v0.4":
					return ReadJournal_v4(location);
				case "file-v0.5":
					config = new EngineConfiguration(location) { SnapshotLocation = snapshotLocation };
					store = new FileStore(config);
					break;
				case "sql":
					config = new SqlEngineConfiguration(location) { SnapshotLocation = snapshotLocation };
					store = new SqlStore(config);
					
					break;
				default:
					throw new ArgumentException("Invalid source type");
			}

			store.Load();
			return store.GetJournalEntries();
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
			if (type == "file-v0.5")
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
	}
}