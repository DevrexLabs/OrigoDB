using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Cms.Core;
using LiveDomain.Core;
using System.IO;
using Blog = Cms.Core.Blog;
namespace Blog.Web
{
    public class Global : System.Web.HttpApplication
    {

        private static BlogModel CreateModelWithTestData()
        {
            BlogModel model = new BlogModel();
            var robert = new User{ UserName = "robertfriberg", Description = "Software architect working devoted to #liveDB", Email = "robert@devrex.se", IsAdmin = true};
            var blog = new Cms.Core.Blog() { Title = "#liveDB team blog" };
            blog.Authors.Add(robert);
            var entry =new BlogEntry{ Title = "Introducing prevalence", Author = robert.UserName, Published = DateTime.Today};
            entry.Content = new HtmlMarkup("Hello world! This is <em>fun</em>");
            entry.Categories.Add("Other");
            entry.Tags.Add("prevalence");
            entry.Tags.Add("nodb");
            blog.Entries.Add(entry);
            entry.Comments.Add( new Comment{ Content = new HtmlMarkup("I agree"), Added = DateTime.Now, Author = "Anonymous"});
            model.Blogs.Add(blog);
            return model;
        }

        public static Engine<BlogModel> Engine
        {
            get; private set;
        }

        void Application_Start(object sender, EventArgs e)
        {
            string path = Server.MapPath("~/App_Data/blogdb");
            if (!Directory.Exists(path)) Engine<BlogModel>.Create(CreateModelWithTestData(), new EngineSettings(path));
            Engine = (Engine<BlogModel>) Engine<BlogModel>.Load<BlogModel>(path);
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
