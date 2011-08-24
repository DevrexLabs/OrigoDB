using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Core;
using TimeTracker.Web.ViewModels;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Commands.Project;

namespace TimeTracker.Web.Controllers
{
    public class ProjectController : Controller
    {
        public ActionResult Index()
        {
            String validationMessage = TempData["validationMessage"] as String;
            if (validationMessage != null)
            {
                ModelState.AddModelError("validationMessage", validationMessage);
            }

            ProjectViewModel viewModel = new ProjectViewModel();
            viewModel.Clients = MvcApplication.Engine.Execute(x => x.Clients);
            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Add()
        {
            ProjectViewModel viewModel = new ProjectViewModel();
            viewModel.Clients = MvcApplication.Engine.Execute(x => x.Clients);

            if (viewModel.Clients.Count == 0)
            {
                TempData["validationMessage"] = "You need to add at least one Client before adding a new Project";
                return RedirectToAction("Index");
            }
            
            return View(viewModel);
        }   

        [HttpPost]
        public ActionResult Add(ProjectViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View();
                }

                AddProjectCommand<TModel> command = new AddProjectCommand<TModel>();
                command.Name = viewModel.Name;
                command.Description = viewModel.Description;
                command.ClientId = viewModel.SelectedClientId;
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
            Project project = MvcApplication.Engine.Execute(m => m.Projects.SingleOrDefault(p => p.Id == id));
            ProjectViewModel viewModel = new ProjectViewModel();
            viewModel.Id = id;
            viewModel.Clients = MvcApplication.Engine.Execute(x => x.Clients);
            viewModel.Name = project.Name;
            viewModel.Description = project.Description;
            viewModel.SelectedClientId = project.Client.Id;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ProjectViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View();
                }

                EditProjectDetailsCommand<TModel> editDetailsCommand = new EditProjectDetailsCommand<TModel>();
                editDetailsCommand.Id = viewModel.Id;
                editDetailsCommand.Name = viewModel.Name;
                editDetailsCommand.Description = viewModel.Description;
                MvcApplication.Engine.Execute(editDetailsCommand);

                ChangeClientForProjectCommand<TModel> changeClientCommand = new ChangeClientForProjectCommand<TModel>();
                changeClientCommand.ProjectId = viewModel.Id;
                changeClientCommand.ClientId = viewModel.SelectedClientId;
                MvcApplication.Engine.Execute(changeClientCommand);

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
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
