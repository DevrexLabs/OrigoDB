using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public class User
    {
        /// <summary>
        /// Blogger uses this to log in. 
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Used for notifications and inviting new bloggers
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Has administrative permission to the blog.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Presentable to blog visitors 
        /// </summary>
        public string Description { get; set; }
        
        
        
        public void SetPassword(string password)
        {
            //TODO: Add crypto stuff
            _hashedPassword = password;
        }
        
        public bool PasswordIs(string password)
        {
            return password == _hashedPassword;
        }

        string _hashedPassword;
    }
}
