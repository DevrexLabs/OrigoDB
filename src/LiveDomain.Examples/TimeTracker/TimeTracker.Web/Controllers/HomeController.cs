using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LiveDomain.Core;
using TimeTracker.Core.Queries;
using TimeTracker.Core;
using TimeTracker.Core.Commands;
using TimeTracker.Web.ViewModels;
using TimeTracker.Core.Commands.Assignment;

namespace TimeTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            UserSession session = UserSession.Current;
            if (session == null)
            {
                return View();
            }

            HomeViewModel viewModel = new HomeViewModel();
            viewModel.User = session.User;
            viewModel.Projects = MvcApplication.Engine.Execute(model => model.Projects);
            viewModel.Roles= MvcApplication.Engine.Execute(model => model.Roles);

            foreach (Project project in viewModel.Projects)
            {
                if (project.Members.Count(m => m.User.Email == viewModel.User.Email) > 0)
                {
                    viewModel.AssignedProjects.Add(project);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddAssignment(HomeViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return RedirectToAction("Index");
                }

                AddAssignmentCommand<TModel> command = new AddAssignmentCommand<TModel>();
                command.User = UserSession.Current.User;
                command.RoleId = viewModel.SelectedRoleId;
                command.ProjectId = viewModel.SelectedProjectId;
                command.HourlyRate = viewModel.HourlyRate;
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
