using System;
using System.IO;

namespace OrigoDB.Core
{
    public abstract class StorageLocation
    {
        public abstract string OfJournal { get; set; }
        public abstract string OfSnapshots { get; set; }

        public abstract string RelativeLocation { get; }

        internal virtual void SetLocationFromType<TModel>()
        {
            SetLocationFromType(typeof(TModel));
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
        }
    }
}