using System;

namespace LiveDomain.Core
{
	public interface IRequestContextFactory : IDisposable
	{
		RequestContext GetContext();
	}
}