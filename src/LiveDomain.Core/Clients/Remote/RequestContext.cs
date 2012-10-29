using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using LiveDomain.Core;
using LiveDomain.Core.Utilities;

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

		public void EnsureConnected()
		{
			// Todo : reconnect.
			if(_disposable.Resource.Client.IsConnected())
			{
			}
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
