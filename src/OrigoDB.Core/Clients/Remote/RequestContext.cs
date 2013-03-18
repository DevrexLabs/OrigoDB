using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using OrigoDB.Core;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
	public class RequestContext : IDisposable
	{
		bool _disposed;
		readonly Disposable<RemoteConnection> _disposable;
		public RemoteConnection Connection { get { return _disposable.Resource; }}


		public RequestContext(Disposable<RemoteConnection> disposable)
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
