using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Woocode.Utils
{
	public class StringToPropertiesMapper
	{

        public char KeyValueDelimiter { get; set; }
        public char KeyValuePairDelimiter { get; set; }

        public Dictionary<Type, Func<string, object>> Converters { get; private set; }
        
        public StringToPropertiesMapper()
		{
			KeyValueDelimiter = '=';
			KeyValuePairDelimiter = ';';

            Converters = new Dictionary<Type, Func<string, object>>();
			Converters[typeof(String)] = x => x;
			Converters[typeof(Double)] = x => Double.Parse(x);
			Converters[typeof(Int32)] = x => Int32.Parse(x);
			Converters[typeof(Int64)] = x => Int64.Parse(x);
			Converters[typeof(IPAddress)] = IPAddress.Parse;
			Converters[typeof(TimeSpan)] = new TimeSpanConverter().ConvertFrom;
			Converters[typeof(DateTime)] = new DateTimeConverter().ConvertFrom;
            Converters[typeof(Boolean)] = s => Boolean.Parse(s);
            Converters[typeof(UInt16)] = s => UInt16.Parse(s);
		}


        public string ToPropertiesString(object source, object defaults, bool includeDefaults = false)
        {
            List<string> pairs = new List<string>();
            foreach (PropertyInfo propertyInfo in source.GetType().GetProperties())
            {
                string propertyName = propertyInfo.Name;
                object propertyValue = propertyInfo.GetGetMethod().Invoke(source, null);
                object @defaultValue = propertyInfo.GetGetMethod().Invoke(defaults, null);
                if (includeDefaults || propertyValue != @defaultValue)
                {
                    pairs.Add(String.Format("{0}={1}", propertyName, propertyValue));
                }
            }
            return String.Join(";", pairs);
        }

		public void MapProperties(string propertiesString, object target, bool throwIfPropertyMissing = true)
		{
            var keyvaluepairs = propertiesString.Split(new[] { KeyValuePairDelimiter },StringSplitOptions.RemoveEmptyEntries);
			var properties = target.GetType().GetProperties();

			foreach (var parameter in keyvaluepairs)
			{
				var keyValuePair = parameter.Split(KeyValueDelimiter);
			    string propertyName = keyValuePair[0].Trim().ToLower();
			    string val = keyValuePair[1].Trim();

				var property = properties.SingleOrDefault(x => x.Name.ToLower() == propertyName);
				if (property == null)
				{
                    if(throwIfPropertyMissing) throw new Exception(String.Format("Missing property [{0}] on target object", propertyName));
				    continue;
				}
				object propertyValue;
				// Convert Value to match property type.
				if(property.PropertyType.IsEnum)
					propertyValue = Enum.Parse(property.PropertyType, val,true);
				else
					propertyValue = Converters[property.PropertyType].Invoke(val);
				// Set the property.
				property.SetValue(target, propertyValue, null);
			}
		}

        

	}
}