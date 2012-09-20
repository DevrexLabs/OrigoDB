using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using LiveDomain.Core;

namespace LiveDomain.Core
{
	public class RequestContext : IDisposable
	{
		bool _disposed;
		readonly Disposable<TcpClient> _disposable;
		public NetworkStream NetworkStream { get { return _disposable.Resource.GetStream(); }}

		public RequestContext(Disposable<TcpClient> disposable)
		{
			_disposable = disposable;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			if(_disposed) return;
			_disposed = true;

			_disposable.Dispose();

			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
