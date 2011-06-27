using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LiveDomain.Core;
using TimeTracker.Core.Queries;
using TimeTracker.Core;
using TimeTracker.Core.Commands;

namespace TimeTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            //int numContacts = MvcApplication.Engine.Execute(ClientQueries.NumOfClients());
            //if (numContacts < 1)
            //{
            //    AddClientCommand<TModel, bool> command = new AddClientCommand<TModel, bool>();
            //    command.Name = "Client #1";
            //    command.Projects = new List<Project>();
            //    MvcApplication.Engine.Execute(command);
            //}

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
