using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace OrigoDB.Core
{
    public static partial class Utils
    {
        public static Dictionary<Type, Func<string, object>> Converters { get; private set; }

        public static object Convert(string s, Type type)
        {
            try
            {
                return type.IsEnum
                    ? Enum.Parse(type, s, ignoreCase: true)
                    : Converters[type].Invoke(s);
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("No converter for type " + type);
            }
        }

        public static T Convert<T>(string s)
        {
            return (T)Convert(s, typeof(T));
        }

        static Utils()
        {
            InitConverters();
        }

        private static void InitConverters()
        {
            Converters = new Dictionary<Type, Func<string, object>>();
            Converters[typeof (String)] = x => x;
            Converters[typeof (Double)] = x => Double.Parse(x);
            Converters[typeof (Int32)] = x => Int32.Parse(x);
            Converters[typeof (Int64)] = x => Int64.Parse(x);
            Converters[typeof (IPAddress)] = IPAddress.Parse;
            Converters[typeof (TimeSpan)] = new TimeSpanConverter().ConvertFrom;
            Converters[typeof (DateTime)] = new DateTimeConverter().ConvertFrom;
            Converters[typeof (Boolean)] = s => Boolean.Parse(s);
            Converters[typeof (UInt16)] = s => UInt16.Parse(s);
        }
    }
}