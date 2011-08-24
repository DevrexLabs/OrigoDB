using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LiveDomain.Core;
using TimeTracker.Core;
using System.IO;
using System.Web.Hosting;
using LiveDomain.Core;

namespace TimeTracker.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        private static Engine<TModel> _engine;
        private static TModel _model;

        public static Engine<TModel> Engine
        {
            get
            {
                if (_engine == null)
                {
                    InitLiveDb();
                }
                return _engine;                
            }
        }

        private static void InitLiveDb()
        {
            string path = HostingEnvironment.ApplicationPhysicalPath + "/App_Data/testDb";
            if (Directory.Exists(path) == false)
            {
                _model = new TModel();
                LiveDomain.Core.Engine.Create(path, _model);
            }
            _engine = LiveDomain.Core.Engine.Load<TModel>(path);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            InitLiveDb();
        }
    }
}