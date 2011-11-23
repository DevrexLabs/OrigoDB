using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Blog.Web
{
    public partial class BloggRoll : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var blogs = Global.Engine.Execute( m => m.Blogs.Select(b => new { Title = b.Title, Description = b.Description}));
            BlogRollDataList.DataSource = blogs;
            BlogRollDataList.DataBind();
        }
    }
}