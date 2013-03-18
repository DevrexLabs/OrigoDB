using System;

namespace OrigoDB.Core
{
	public interface IRequestContextFactory : IDisposable
	{
		RequestContext GetContext();
	}
}