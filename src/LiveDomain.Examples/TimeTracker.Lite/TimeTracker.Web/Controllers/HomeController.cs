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

            foreach (Project project in viewModel.Projects)
            {
                if (project.Members.Count(m => m.Email == viewModel.User.Email) > 0)
                {
                    viewModel.AssignedProjects.Add(project);
                }
            }

            return View(viewModel);
        }
    }
}
