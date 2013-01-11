using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace LiveDomain.Core
{
	public class ResourcePool<T> : IDisposable where T : IDisposable
	{
		private BlockingCollection<T> _availableResources;
		private List<T> _allResources;
		private int _maxSize;
		private Func<T> _constructor;

		private bool _isDisposed;

		~ResourcePool()
		{
			if (!_isDisposed)
			{
				Dispose();
			}
		}

		public int MaxSize
		{
			get { return _maxSize; }
		}

		public int Count
		{
			get { return _allResources.Count; }
		}

		public int Available
		{
			get { return _availableResources.Count; }
		}

		internal ResourcePool(Func<T> constructor, int maxSize = 100)
		{
			_constructor = constructor;
			_maxSize = maxSize;
			_availableResources = new BlockingCollection<T>(new ConcurrentQueue<T>());
			_allResources = new List<T>(_maxSize);
		}

		private void ThrowIfDisposed()
		{
			if (_isDisposed)
				throw new InvalidOperationException("Resource pool is disposed");
		}

		/// <summary>
		/// Aquires a resource
		/// </summary>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait for the resource to be aquired, 
		/// default <see cref="F:System.Threading.Timeout.Infinite"/> (-1) will wait indefinitely.</param>
		/// <returns></returns>
		internal T AquireResource(int millisecondsTimeout = -1)
		{
			ThrowIfDisposed();
			lock (this)
			{
				//Create new resource if necessary and allowed
				if (Available == 0)
				{
					var freeSlots = MaxSize - Count;
					if (freeSlots > 0)
					{
						// Create one extra resource if possible
						if (freeSlots > 1) _availableResources.Add(CreateResource());
						return CreateResource();
					}

				}
				T result;
				if (_availableResources.TryTake(out result, millisecondsTimeout))
				{
					return result;
				}
			}

			throw new Exception("Couldn't get resource from pool");
		}

		T CreateResource()
		{
			var resource = _constructor.Invoke();
			_allResources.Add(resource);
			return resource;
		}

		internal void Release(T resource)
		{
			ThrowIfDisposed();
			_availableResources.Add(resource);
		}

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;
			if (_allResources != null)
			{
				foreach (var resource in _allResources)
				{
					resource.Dispose();
				}
				_allResources.Clear();
			}
			_availableResources = null;
			
		}
	}
}
