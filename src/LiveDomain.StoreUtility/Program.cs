 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
 using LiveDomain.Core.Storage;
using LiveDomain.Core;
using LiveDomain.Modules.SqlStorage;
using System.IO;
using System.Reflection;
using CommandLine;

namespace LiveDomain.StoreUtility
{
	public class Program
	{
		

		static void Main(string[] args)
		{
            if (args[0].ToLower() == "convert")
            {
                args = args.Skip(1).ToArray();
                var arguments = ParseArgs<ConverterArguments>(args);
                var converter = new StoreConverter(arguments);
                converter.Notifications += Console.Write;
                converter.Convert();
            }
            else
            {
                Console.WriteLine("Bad input, valid first arguments: convert");
            }
  
		}

	    private static T ParseArgs<T>(string[] args) where T : Arguments
	    {
	        var result = Activator.CreateInstance<T>();
            if (!CommandLineParser.Default.ParseArguments(args, result))
                throw new Exception(result.GetType().Name);
            result.Validate();
	        return result;
	    }


	}
}