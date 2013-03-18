using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OrigoDB.Core;
using OrigoDB.Core.Storage;
using OrigoDB.Modules.SqlStorage;

namespace OrigoDB.StoreUtility
{
    /// <summary>
    /// Convert between different store formats and versions
    /// </summary>
    public class StoreConverter
    {
        public delegate void NotificationHandler(string message);
        public event NotificationHandler Notifications = (m) => {};

        private ConverterArguments _args;
        private Dictionary<string, Assembly> _dynamicallyReferencedAssemblies;

        public StoreConverter(ConverterArguments args)
        {
            _args = args;
            _dynamicallyReferencedAssemblies = new Dictionary<string, Assembly>();
        }

        public void Convert()
        {
            //dynamically reference the assembly passed as argument
            LoadAssembly();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
            Notifications.Invoke("Assembly loaded\n");

            var destinationStore = CreateStoreAndWriteInitialSnaphot();
            Notifications.Invoke("Destination snapshot created\n");

            long entriesConverted;
            ConvertJournal(destinationStore, out entriesConverted);
            Notifications.Invoke("\rEntries converted: " + entriesConverted + "\n");
        }
        
        private Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!_dynamicallyReferencedAssemblies.ContainsKey(args.Name))
                throw new Exception(args.Name);

            return _dynamicallyReferencedAssemblies[args.Name];
        }

        private void LoadAssembly()
        {
            var assembly = Assembly.LoadFrom(_args.Assembly);
            _dynamicallyReferencedAssemblies[assembly.FullName] = assembly;
        }

        private void ConvertJournal(Store destination, out long entriesConverted)
        {
            entriesConverted = 0;
            var destinationJournal = destination.CreateJournalWriter(0);
            var sourceJournal = GetSourceJournalEntries();
            foreach (var journalEntry in sourceJournal)
            {

                if (++entriesConverted % 67 == 0) Notifications.Invoke("\rEntries: " + entriesConverted);
                destinationJournal.Write(journalEntry);
            }
            destinationJournal.Close();
        }

        private IEnumerable<JournalEntry> GetSourceJournalEntries()
        {
            EngineConfiguration config;
            Store store;
            switch (_args.SourceType)
            {
                case "file-v0.4":
                    return GetJournalEntries_v4();
                case "file-v0.5":
                    config = new EngineConfiguration(_args.Source);
                    config.Location.OfSnapshots = _args.SourceSnapshots;
                    store = new FileStore(config);
                    break;
                case "sql":
                    config = new SqlEngineConfiguration(_args.Source);
                    config.Location.OfSnapshots = _args.SourceSnapshots;
                    store = new SqlStore(config);
                    break;
                default:
                    throw new ArgumentException("Invalid source type");
            }
            store.Load();
            return store.GetJournalEntries();
        }

        private Model LoadSnapshot(string absolutePath)
        {
            Stream stream = File.OpenRead(absolutePath);
            var serializer = new EngineConfiguration().CreateSerializer();
            var model = serializer.Read<Model>(stream);
            stream.Dispose();
            return model;
        }

        private IEnumerable<JournalEntry> GetJournalEntries_v4()
        {
            ISerializer serializer = new EngineConfiguration().CreateSerializer();
            List<string> files = Directory.GetFiles(_args.Source, "*.journal").ToList();
            files.Sort(String.CompareOrdinal);
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

        private Store CreateStoreAndWriteInitialSnaphot()
        {
            Store store;
            if (_args.DestinationType == "file-v0.5")
            {
                var config = new EngineConfiguration(_args.Destination);
                config.Location.OfSnapshots = _args.DestinationSnapshots;
                store = new FileStore(config);
            }
            else if (_args.DestinationType == "sql")
            {
                var config = new SqlEngineConfiguration(_args.Destination);
                config.Location.OfSnapshots = _args.DestinationSnapshots;
                store = new SqlStore(config);
            }
            else throw new ArgumentException("Invalid destination type");
            var initialModel = LoadSnapshot(Path.Combine(_args.SourceSnapshots, "000000000.snapshot"));
            store.Create(initialModel);
            return store;
        }
    }
}
