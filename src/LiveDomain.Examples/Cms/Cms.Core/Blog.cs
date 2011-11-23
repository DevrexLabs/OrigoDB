using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public class Blog
    {

        /// <summary>
        /// Name of the blog as presented to users
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Name of a CSS theme used to style the blog
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Description of the blog presented to visitors and in the RSS feed
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Entries published to this blog in chronological order
        /// </summary>
        public List<BlogEntry> Entries{ get; set; }

        //Authors allowed to post to this blog
        public List<User> Authors { get; set; }

        public Blog()
        {
            Authors = new List<User>();
            Entries = new List<BlogEntry>();
        }
    }
}
