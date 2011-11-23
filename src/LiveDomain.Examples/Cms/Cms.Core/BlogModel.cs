using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using System.Text.RegularExpressions;

namespace Cms.Core
{
    [Serializable]
    public class BlogModel : Model
    {
        public IDictionary<string, User> Users { get; private set; }
        public List<Blog> Blogs { get; private set; }

        /// <summary>
        /// Rebuild after load to save space and image read/write time
        /// </summary>
        [NonSerialized]
        IDictionary<string, HashSet<BlogEntry>> _searchIndex;

        public IDictionary<string, HashSet<BlogEntry>> SearchIndex
        {
            get { return _searchIndex; }
            private set { _searchIndex = value; }
        }

        public BlogModel()
        {
            Users = new Dictionary<string, User>();
            Blogs = new List<Blog>();
            User admin = new User() { UserName = "admin", Description = "System administrator", IsAdmin = true };
            admin.SetPassword("admin");
            Users.Add(admin.UserName, admin);
            SearchIndex = new Dictionary<string, HashSet<BlogEntry>>();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            RebuildIndex();
        }

        /// <summary>
        /// In memory full text indexing hosted on #liveDB
        /// </summary>
        public void RebuildIndex()
        {
            const string SplitterExpr = @"\W+";
            SearchIndex = new Dictionary<string, HashSet<BlogEntry>>();
            foreach (var entry in Blogs.SelectMany(b => b.Entries))
            {
                foreach (string html in entry.Comments.Select(c => c.Content.RenderHtml())
                    .Union(new String[] { entry.Content.RenderHtml(), entry.Summary }))
                {
                    string text = StripHtml(html ?? String.Empty);
                    foreach (var key in Regex.Split(text, SplitterExpr).Select(s => s.ToLowerInvariant()))
                    {
                        if (!SearchIndex.ContainsKey(key)) SearchIndex.Add(key, new HashSet<BlogEntry>());
                        SearchIndex[key].Add(entry);
                    }
                }
            }
        }


        /// <summary>
        /// Used to build a tag cloud
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, int> GetTagsAndCounts()
        {
            Dictionary<string, int> tagCounts = new Dictionary<string, int>();

            foreach (string tag in Blogs.SelectMany(b => b.Entries.SelectMany(e => e.Tags)))
            {
                if (!tagCounts.ContainsKey(tag)) tagCounts.Add(tag, 0);
                tagCounts[tag]++;
            }
            return tagCounts;
        }

        //TODO: Doesnt belong here..put in common dictionary and add adjust time with 5 minutes
        private string StripHtml(string html)
        {
            const string StripperExpr = "<[^>]+?>";
            return Regex.Replace(html, StripperExpr, " ");
        }
    }
}
