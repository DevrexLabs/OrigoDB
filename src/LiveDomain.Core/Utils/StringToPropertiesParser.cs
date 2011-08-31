using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace LiveDomain.Core.Utils
{
	public class StringToPropertiesParser
	{
		static StringToPropertiesParser()
		{
			Converters = new Dictionary<string, Func<string, object>>();
			// Add supported types. (Extend with more later)
			Converters["String"] = x => x;
			Converters["Double"] = x => Double.Parse(x);
			Converters["Int32"] = x => Int32.Parse(x);
			Converters["Int64"] = x => Int64.Parse(x);
			Converters["Double[]"] = SetArrayAndValues<Double>;
			Converters["Int32[]"] = SetArrayAndValues<Int32>;
			Converters["Int64[]"] = SetArrayAndValues<Int64>;
			Converters["String[]"] = SetArrayAndValues<String>;
			Converters["IPAddress"] = IPAddress.Parse;
			Converters["TimeSpan"] = new TimeSpanConverter().ConvertFrom;
			Converters["DateTime"] = new DateTimeConverter().ConvertFrom;
		}

		public static Dictionary<string, Func<string, object>> Converters { get; private set; }

		public static void PopulateProperties(object @object, string inputString, char keyValueDelimiter = '=', char keyValuePairDelimiter = ';')
		{
			var keyvaluepairs = inputString.Split(keyValuePairDelimiter).Where(x => !string.IsNullOrEmpty(x));
			var properties = @object.GetType().GetProperties();

			foreach (var parameter in keyvaluepairs)
			{
				var keyvalue = parameter.Split(keyValueDelimiter);
				var objectKeyValuePair = new KeyValuePair<string, string>(keyvalue[0].Trim().ToLower(), keyvalue[1].Trim());
				// Get all Properties.
				var property = properties.SingleOrDefault(x => x.Name.ToLower() == objectKeyValuePair.Key);
				if (property == null)
					continue;
				// Convert Value to match property type.
				var propertyValue = CreateTypeWithValue(property.PropertyType.Name, objectKeyValuePair.Value);
				// Set the property.
				property.SetValue(@object, propertyValue, null);
			}
		}

		private static T[] SetArrayAndValues<T>(string arrayData)
		{
			Type arrayType = typeof (T);
			string[] arrayitems = arrayData.Split(',');
			return arrayitems.Select(x => (T) CreateTypeWithValue(arrayType.Name, x)).ToArray();
		}

		private static object CreateTypeWithValue(string propertyType, string data)
		{
			if (!Converters.ContainsKey(propertyType))
				throw new NotSupportedException(string.Format("Unsupported propertytype : {0}", propertyType));

			return Converters[propertyType](data);
		}
	}
}