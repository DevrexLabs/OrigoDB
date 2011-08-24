using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Core;
using TimeTracker.Core.Commands.Role;

namespace TimeTracker.Web.Controllers
{
    public class RoleController : Controller
    {

        public ActionResult Index()
        {
            List<Role> roles = MvcApplication.Engine.Execute(model => model.Roles);
            return View(roles);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        } 

        [HttpPost]
        public ActionResult Add(Role role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                AddRoleCommand<TModel> command = new AddRoleCommand<TModel>();
                command.Name = role.Name;
                command.Description = role.Description;
                command.DefaultHourlyRate = role.DefaultHourlyRate;
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
            List<Role> roles = MvcApplication.Engine.Execute(model => model.Roles);
            Role role = roles.SingleOrDefault(r => r.Id == id);
            return View(role);
        }

        [HttpPost]
        public ActionResult Edit(int id, Role role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                EditRoleCommand<TModel> command = new EditRoleCommand<TModel>();
                command.Id = id;
                command.Name = role.Name;
                command.Description = role.Description;
                command.DefaultHourlyRate = role.DefaultHourlyRate;
                MvcApplication.Engine.Execute(command);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
