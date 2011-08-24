using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands
{
    [Serializable]
    public class AddUserCommand<M> : Command<TModel>
    {
        public String Email { get; set; }
        public String Password { get; set; }
        public String Name { get; set; }

        protected override void Execute(TModel model)
        {
            model.AddUser(Name, Email, Password);
        }
    }

	[Serializable]
	public class RemoveUserCommand<M> : Command<TModel>
	{
		public User User { get; set; }

		protected override void Execute(TModel model)
		{
			model.RemoveUser(User);
		}
	}
}