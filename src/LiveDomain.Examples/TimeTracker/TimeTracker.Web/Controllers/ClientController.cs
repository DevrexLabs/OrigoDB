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

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(Client inputClient)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                AddClientCommand<TModel, bool> command = new AddClientCommand<TModel, bool>();
                command.Name = inputClient.Name;
                MvcApplication.Engine.Execute(command);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            Client client = MvcApplication.Engine.Execute(m => m.Clients.FirstOrDefault(x => x.Id == id));
            return View(client);
        }

        [HttpPost]
        public ActionResult Edit(Client inputClient)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                UpdateClientCommand<TModel, bool> command = new UpdateClientCommand<TModel, bool>();
                command.Id = inputClient.Id;
                command.NewName = inputClient.Name;
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
