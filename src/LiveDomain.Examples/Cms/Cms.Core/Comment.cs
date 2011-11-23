using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public class Comment
    {
        public string Author { get; set; }
        public string Email { get; set; }
        public Markup Content { get; set; }
        public DateTime Added { get; set; }
    }
}
