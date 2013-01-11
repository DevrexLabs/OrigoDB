using System;
using System.IO;

namespace LiveDomain.Core
{
    public abstract class StorageLocation
    {
        public abstract string OfJournal { get; set; }
        public abstract string OfSnapshots { get; set; }

        public abstract string RelativeLocation { get; }

        internal virtual void SetLocationFromType<M>()
        {
            SetLocationFromType(typeof(M));
        }


        public bool HasJournal 
        { 
            get
            {
                return !String.IsNullOrEmpty(OfJournal);        
            }
        }

        public abstract bool HasAlternativeSnapshotLocation { get; }

        internal void SetLocationFromType(Type type)
        {
            OfJournal = type.Name;
        }


        /// <summary>
        /// The default directory to use if the Location is relative
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultDirectory()
        {
            string result = Directory.GetCurrentDirectory();

            String dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as String;

            if(dataDirectory != null && dataDirectory.EndsWith("App_Data"))
            {
                result = dataDirectory;
            }
            return result;

            ////Attempt web
            //try
            //{
            //    string typeName = "System.Web.HttpContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            //    Type type = Type.GetType(typeName);
            //    object httpContext = type.GetProperty("Current").GetGetMethod().Invoke(null, null);
            //    object httpServer = type.GetProperty("Server").GetGetMethod().Invoke(httpContext, null);
            //    result = (string)httpServer.GetType().GetMethod("MapPath").Invoke(httpServer, new object[] { "~/App_Data" });
            //}
            //catch { }
            //return result;
        }
    }
}