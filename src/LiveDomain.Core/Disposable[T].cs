using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	public class Disposable<T> : IDisposable
	{
		readonly Action _disposeAction;
		readonly T _resource;
		bool _isDisposed;

		public T Resource { get { ThrowIfDisposed(); return _resource; } }

		public Disposable(T resource, Action disposeAction = null)
		{
			_resource = resource;
			_disposeAction = disposeAction;
		}

		public void Dispose()
		{
			if(_isDisposed) return;
			if(_disposeAction != null) _disposeAction.Invoke();
			_isDisposed = true;
		}

		void ThrowIfDisposed()
		{
			if (_isDisposed) throw new ObjectDisposedException("Object is disposed");
		}
	}
}
