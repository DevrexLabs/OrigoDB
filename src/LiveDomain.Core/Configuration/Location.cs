using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    //Location related parts of this class. promote to Location class later while implementing storage providers.
    public partial class EngineConfiguration
    {
        string _location, _snapshotLocation;

        /// <summary>
        /// The location of the command journal and snapshots. A directory path when using FileStorage, 
        /// a connection string when using SqlStorage.
        /// Assigning a relative path will resolve to current directory or App_Data if running in a web context
        /// </summary>
        public string Location
        {
            get { return _location != null ? Path.Combine(GetDefaultDirectory(), _location) : _location; }
            set { _location = value; }
        }

        /// <summary>
        /// Same as TargetLocation unless set to some other location
        /// </summary>
        public string SnapshotLocation
        {
            get
            {
                return _snapshotLocation ?? Location;
            }
            set
            {
                if (value == null || value == Location) _snapshotLocation = null;
                else _snapshotLocation = value;
            }
        }




        /// <summary>
        /// True if the snapshotlocation differs from the location of the journal
        /// </summary>
        public bool HasAlternativeSnapshotLocation
        {
            get
            {
                return _snapshotLocation != null;
            }

        }


        /// <summary>
        /// The default directory to use if the Location is relative
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultDirectory()
        {

            string result = Directory.GetCurrentDirectory();

            //Attempt web
            try
            {
                string typeName = "System.Web.HttpContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                Type type = Type.GetType(typeName);
                object httpContext = type.GetProperty("Current").GetGetMethod().Invoke(null, null);
                object httpServer = type.GetProperty("Server").GetGetMethod().Invoke(httpContext, null);
                result = (string)httpServer.GetType().GetMethod("MapPath").Invoke(httpServer, new object[] { "~/App_Data" });
            }
            catch { }
            return result;
        }

        public bool HasLocation
        {
            get
            {
                return !String.IsNullOrEmpty(_location);
            }
        }

        internal void SetLocationFromType<M>()
        {
            Location = typeof(M).Name;
        }

    }
}
