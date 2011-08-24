using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeTracker.Core;

namespace TimeTracker.Web.ViewModels
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public List<Project> AssignedProjects { get; set; }
        public List<Project> Projects { get; set; }

        public int SelectedProjectId { get; set; }

        public HomeViewModel()
        {
            AssignedProjects = new List<Project>();
            Projects = new List<Project>();
        }
    }
}