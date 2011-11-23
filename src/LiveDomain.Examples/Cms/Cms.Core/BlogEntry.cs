using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public class BlogEntry
    {

        public Markup Content { get; set; }
        
        public string Summary { get; set; }
        
        public string Title { get; set; }
        
        public ICollection<String> Categories { get; set; }
        
        public ICollection<String> Tags { get; set; }

        public string Author { get; set; }


        public DateTime Published { get; set; }

        public List<Comment> Comments { get; set; }

        public BlogEntry()
        {
            Categories = new List<string>();
            Tags = new List<String>();
            Content = new HtmlMarkup(String.Empty);
            Comments = new List<Comment>();
        }
    }
}
