using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Core;
using TimeTracker.Core.Commands;

namespace TimeTracker.Web.Controllers
{
    public class ClientController : Controller
    {
        public ActionResult Index()
        {
            IEnumerable<Client> allClients = MvcApplication.Engine.Execute(x => x.Clients);
            return View(allClients);
        }

        public ActionResult Details(string name)
        {
            Client client = MvcApplication.Engine.Execute(m => m.Clients.FirstOrDefault(x => x.Name == name));
            return View(client);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Client inputClient)
        {
            try
            {
                AddClientCommand<TModel, bool> command = new AddClientCommand<TModel, bool>();
                command.Name = inputClient.Name;
                command.Projects = new List<Project>();
                MvcApplication.Engine.Execute(command);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
 
        public ActionResult Edit(string name)
        {
            Client client = MvcApplication.Engine.Execute(m => m.Clients.FirstOrDefault(x => x.Name == name));
            return View(client);
        }

        [HttpPost]
        public ActionResult Edit(string oldName, string name, Client inputClient)
        {
            try
            {
                UpdateClientCommand<TModel, bool> command = new UpdateClientCommand<TModel, bool>();
                command.OldName = oldName;
                command.NewName = name;
                MvcApplication.Engine.Execute(command);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(string name)
        {
            Client client = MvcApplication.Engine.Execute(m => m.Clients.FirstOrDefault(x => x.Name == name));
            return View(client);
        }

        [HttpPost]
        public ActionResult Delete(string name, FormCollection collection)
        {
            try
            {
                DeleteClientCommand<TModel, bool> command = new DeleteClientCommand<TModel, bool>();
                command.Name = name;
                MvcApplication.Engine.Execute(command);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
