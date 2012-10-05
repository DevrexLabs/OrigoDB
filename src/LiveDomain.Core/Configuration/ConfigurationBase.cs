using System;
using System.Configuration;

namespace LiveDomain.Core
{
    [Serializable]
    public abstract class ConfigurationBase
    {

        public virtual string KeyTemplate
        {
            get { return "LiveDomain.{0}"; }
        }

        protected T LoadFromConfigOrDefault<T>(Func<T> @default = null)
        {
            @default = @default ?? (() => (T)Activator.CreateInstance(typeof(T)));
            string configKey = ConfigKeyFromType(typeof(T));
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