using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeTracker.Core;

namespace TimeTracker.Web
{
    public class UserSession
    {
        public const string SessionKey = "CurrentUserKey";

        public User User { get; set; }

        public static UserSession Current
        {
            get
            {
                UserSession currentUser = HttpContext.Current.Session[SessionKey] as UserSession;
                if (currentUser == null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    PopulateUserSession(HttpContext.Current.User.Identity.Name);
                    currentUser = HttpContext.Current.Session[SessionKey] as UserSession;
                }
                return currentUser;
            }
        }

        public static void PopulateUserSession(string email)
        {
            List<User> users = MvcApplication.Engine.Execute(m => m.Users);
            User user = users.SingleOrDefault(u => u.Email == email);

            UserSession session = new UserSession();
            session.User = user;

            HttpContext.Current.Session[UserSession.SessionKey] = session;
        }

        public static void LogOut()
        {
            UserSession currentUser = HttpContext.Current.Session[SessionKey] as UserSession;
            if (currentUser != null)
            {
                HttpContext.Current.Session[SessionKey] = null;
            }
        }
    }
}