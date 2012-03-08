using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text.RegularExpressions;
using LiveDomain.Core.Logging;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;

namespace LiveDomain.Core
{
    public class Modules
    {
        readonly static Modules _instance = new Modules();

        [ImportMany()]
        private IEnumerable<ILogFactory> LogFactories { get; set; }

        public static void SetLogFactory(ILogFactory logFactory)
        {
            _instance.LogFactories = new ILogFactory[] {logFactory};
        }

        public static ILogFactory GetLogFactory()
        {
            if (_instance.LogFactories == null || !_instance.LogFactories.Any())
            {
                return new SingletonLogFactory(InternalLog);
            }
            else return _instance.LogFactories.First();
        }

        private static ILog _internalLog;
        internal static ILog InternalLog
        {
            get
            {
                if (_internalLog == null)
                {
                    _internalLog = new InternalLogFactory().GetLog("internal");
                }
                return _internalLog;
            }
        }

        internal Modules()
        {
            Initialize();
        }


        private void Initialize()
        {
            var aggregateCatalog = new AggregateCatalog();
            ImportReferencedAssemblies(aggregateCatalog);
            ImportModulesDirectoryRecursively(aggregateCatalog);
            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts(this);
            ValidateImports();
        }

        private void ValidateImports()
        {
            if(LogFactories.Count() > 1)
            {
                InternalLog.Warn("Multiple log modules imported, using {0}", LogFactories.First().GetType().FullName); 
            }
        }

        private void ImportModulesDirectoryRecursively(AggregateCatalog aggregateCatalog)
        {
            const string moduleCatalog = @".\modules"; //todo: Config.ModuleDirectory
            if (Directory.Exists(moduleCatalog))
            {
                ImportModulesDirectoryRecursively(aggregateCatalog, moduleCatalog);
            }
                

        }
        private static void ImportModulesDirectoryRecursively(AggregateCatalog aggregateCatalog, string path)
        {
            aggregateCatalog.Catalogs.Add(new DirectoryCatalog(path));
            foreach(var subdirectory in Directory.EnumerateDirectories(path))
            {
                aggregateCatalog.Catalogs.Add(new DirectoryCatalog(subdirectory));
            }
        }

        private static void ImportReferencedAssemblies(AggregateCatalog aggregateCatalog)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly != null)
            {
                aggregateCatalog.Catalogs.Add(new AssemblyCatalog(entryAssembly));
                var referencedAssemblies = entryAssembly.GetReferencedAssemblies();
                foreach (var assemblyName in referencedAssemblies.Where(asmName => !IsSystemAssembly(asmName.Name)))
                {
                    var assembly = Assembly.Load(assemblyName);
                    aggregateCatalog.Catalogs.Add(new AssemblyCatalog(assembly));
                }
            }
        }

        private static bool IsSystemAssembly(string name)
        {
            return Regex.IsMatch(name, "^(Microsoft|System|mscorlib)");
        }
    }
}
