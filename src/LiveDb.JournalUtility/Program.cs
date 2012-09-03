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

namespace LiveDb.JournalUtility
{
    class Program
    {
        static Model initialModel;
        static void Main(string[] args)
        {
            
            args = new[] {@"-source=c:\livedb\freedb.002", @"-destination=c:\livedb\freedb.003", "-source-type=file-v0.4", "-destination-type=file"};
            var parsedArgs = ParseArgs(args);

            Store destination = CreateStore(parsedArgs["destination"], parsedArgs["destination-type"]);
            IterateSource(parsedArgs["source"], parsedArgs["source-type"], destination);
            
            
        }

        private static void IterateSource(string location, string sourceType,  Store destination )
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
                var store = new FileStore(config);
                store.Load();
                initialModel = LoadSnapshot(Path.Combine(location, "000000000.000000000.snapshot"));
                foreach (var journalEntry in store.GetJournalEntries())
                {
                    writer.Write(journalEntry);
                }
            }
            else if (sourceType == "sql")
            {
                var config = new SqlEngineConfiguration(location);
                var store = new SqlStore(config);
                //store.Load();
                foreach (var journalEntry in store.GetJournalEntries())
                {
                    writer.Write(journalEntry);
                }
            }
            else throw new ArgumentException("Invalid source type");
            writer.Close();

        }

        private static Model LoadSnapshot(string path)
        {
            Stream stream = File.OpenRead(path);
            var serializer = new EngineConfiguration().CreateSerializer();
            var model = serializer.Read<Model>(stream);
            stream.Dispose();
            return model;
        }

        private static IEnumerable<JournalEntry> ReadJournal_v4(string location)
        {
            ISerializer serializer = new EngineConfiguration().CreateSerializer();
            List<string> files = Directory.GetFiles(location, "*.journal").ToList();
            files.Sort((a,b) => a.CompareTo(b));
            long entryId = 1;
            foreach(string file in files)
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

        private static Store CreateStore(string location, string type)
        {
            if(type == "file") return new FileStore(new EngineConfiguration(location));
            else if (type == "sql") return new SqlStore(new SqlEngineConfiguration(location));
            else throw new ArgumentException("Invalid destination type");
        }

        static Dictionary<string,string> ParseArgs(string[] args)
        {
            Regex regex = new Regex("^-(?<key>.*?)=(?<val>.*)$");
            var dictionary = new Dictionary<string, string>();
            foreach (var pair in args)
            {
                Match m = regex.Match(pair);
                if(!m.Success) throw new ArgumentException("bad parameter format");
                string key = m.Groups["key"].Value;
                string val = m.Groups["val"].Value;
                if(dictionary.ContainsKey(key)) throw new ArgumentException("Duplicate parameter");
                dictionary[key] = val;
            }

            //Validate
            var required = new []{"source", "destination"};
            var allowed = new[] {"source", "source-type", "destination", "destination-type"};
            Validate(dictionary.Keys.ToArray(), allowed, required);

            //set defaults
            if (!dictionary.ContainsKey("source-type")) dictionary["source-type"] = "file";
            if (!dictionary.ContainsKey("destination-type")) dictionary["destination-type"] = "file";

            if(dictionary["source-type"] == dictionary["destination-type"]) throw new ArgumentException("source and destination types must be different");

            if(dictionary["source-type"] == "file-v0.4" && dictionary["destination-type"] == "file" && dictionary["source"] == dictionary["destination"])
                throw new ArgumentException("source and destination cannot be the same (in-place upgrade not supported at this time)");
            return dictionary;
        }

        private static void Validate(string[] keysGiven, string[] keysAllowed, string[] keysRequired)
        {
            foreach(string key in keysGiven)
            {
                if (!keysAllowed.Contains(key)) throw new ArgumentException("Invalid key:" + key);
            }

            foreach (var key in keysRequired)
            {
                if(!keysGiven.Contains(key)) throw new ArgumentException("key required: " + key);
            }
        }

        static void Usage()
        {
            Console.WriteLine("usage:");
        }
    }
}
