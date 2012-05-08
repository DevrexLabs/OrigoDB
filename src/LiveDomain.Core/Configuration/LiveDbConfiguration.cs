
using System.Configuration;
using System;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core.Configuration
{
    public class LiveDbConfiguration
    {

        public virtual string KeyTemplate
        {
            get { return "LiveDbConfiguration.{0}"; }
        }

        private static LiveDbConfiguration _current;

        public static LiveDbConfiguration Current
        {
            get
            {
                if (_current == null) _current = Load();
                return _current;
            }
            
        }

        public static LiveDbConfiguration Load()
        {
            var bootStrapper = new LiveDbConfiguration();
            return bootStrapper.LoadFromConfigOrDefault<LiveDbConfiguration>();
        }

        protected virtual T LoadFromConfigOrDefault<T>(Func<T> @default = null )
        {
            @default = @default ?? (() => (T)Activator.CreateInstance(typeof(T)));
            string configKey = ConfigKeyFromType(typeof (T));
            var configTypeName = ConfigurationManager.AppSettings[configKey];
            if (!String.IsNullOrEmpty(configTypeName))
            {
                return InstanceFromTypeName<T>(configTypeName);
            }
            else
            {
                return @default.Invoke();
            }
        }

        protected virtual T InstanceFromTypeName<T>(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName);
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                String messageTemplate = "Can't load type {0}, see inner exception for details";
                throw new ConfigurationErrorsException(String.Format(messageTemplate, typeName), exception);
            }
        }


        private ILogFactory _logFactory = null;

        protected internal virtual ILogFactory GetLogFactory()
        {
            if (_logFactory == null)
            {
                _logFactory = LoadFromConfigOrDefault<ILogFactory>(() => new InternalLogFactory());
            }
            return _logFactory;
        }

        public virtual void SetLogFactory(ILogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        internal Security.IAuthorizer<Type> GetAuthorizer()
        {
            throw new NotImplementedException();
        }

        public virtual EngineConfiguration GetEngineConfiguration()
        {
            return LoadFromConfigOrDefault<EngineConfiguration>();
        }

        internal IStorage GetCustomStorage(EngineConfiguration engineConfiguration)
        {
            return LoadFromConfig<IStorage>();
        }

        protected virtual T LoadFromConfig<T>()
        {
            string configKey = String.Format(KeyTemplate, ConfigKeyFromType(typeof(T)));
            var configTypeName = ConfigurationManager.AppSettings[configKey];
            return InstanceFromTypeName<T>(configTypeName);
        }

        protected virtual String ConfigKeyFromType(Type type)
        {
            return type.Name;
        }
    }
}
