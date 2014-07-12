using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    [Serializable]
    public class ConfigDictionary : Dictionary<string, string>
    {

        public ConfigDictionary()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }


        public void Set(string key, object value)
        {
            this[key] = value.ToString();
        }

        public T Get<T>(string key, Func<T> @default = null)
        {
            if (ContainsKey(key)) return Utils.Convert<T>(this[key]);
            else if (@default != null) return @default.Invoke();
            else throw new KeyNotFoundException();
        }


        public bool TryGet<T>(string key, out T result)
        {
            result = default(T);
            bool exists = ContainsKey(key);
            if (exists) result = Get<T>(key);
            return exists;
        }

        /// <summary>
        /// Parse a string of key/value pairs
        /// </summary>
        /// <param name="source"></param>
        /// <param name="primaryDelimiter"></param>
        /// <param name="secondaryDelimiter"></param>
        /// <returns></returns>
        public static ConfigDictionary FromDelimitedString(string source, char primaryDelimiter = ';', char secondaryDelimiter = '=')
        {
            var config = new ConfigDictionary();
            //Do some pre cleaning
            source = source.Trim().TrimEnd(';');

            var properties = source.Split(new char[]{ primaryDelimiter}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string property in properties)
            {
                var pair = property.Split(new char[]{ secondaryDelimiter},StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2) throw new InvalidOperationException("Invalid delimited string");
                config[pair[0].Trim()] = pair[1].Trim();
            }
            return config;
        }

        public static ConfigDictionary FromAppSettings()
        {
            return FromNameValueCollection(ConfigurationManager.AppSettings);
        }

        public static ConfigDictionary FromNameValueCollection(NameValueCollection collection)
        {
            var config = new ConfigDictionary();
            foreach (string key in collection.Keys) config[key] = collection[key];
            return config;
        }

        /// <summary>
        /// Copy dictionary values to public and private, declared and inherited properties, using
        /// case insensitive mapping from keys to property names.
        /// <remarks>Keys with two part dot notation will be interpreted as ClassName.PropertyName and only mapped
        /// when the target type name is matched: EngineConfiguration.PacketOptions</remarks>
        /// <remarks>Requires a converter for each encountered type, see Utils.Converters</remarks>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="throwWhenPropertyMissing"></param>
        /// <param name="keyFilter"></param>
        /// <param name="targetType"></param>
        public void MapTo(object target, bool throwWhenPropertyMissing = true, Func<string, bool> keyFilter = null, Type targetType = null)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            targetType = targetType ?? target.GetType();
            keyFilter = keyFilter ?? (key => true);
            var props = targetType.GetProperties(bindingFlags)
                .ToDictionary(p => p.Name, p => p, StringComparer.InvariantCultureIgnoreCase);
            foreach (string key in Keys.Where(k => keyFilter.Invoke(k)))
            {
                var path = key.Split('.');
                Ensure.That(path.Length <= 2, "Nested properties not supported");
                if (path.Length == 2 && !String.Equals(path[0], targetType.Name, StringComparison.InvariantCultureIgnoreCase)) continue;
                var propName = path.Last();
                if (!props.ContainsKey(propName))
                {
                    if (throwWhenPropertyMissing) throw new KeyNotFoundException("No such property: <" + propName + ">");
                }
                else
                {
                    var propInfo = props[propName];
                    var propType = propInfo.PropertyType;
                    var convertedValue = Utils.Convert(this[key], propType);
                    propInfo.SetValue(target, convertedValue, null);
                }
            }
        }
 }
}