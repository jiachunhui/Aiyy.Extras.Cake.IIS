using Microsoft.Extensions.Caching.Memory;

namespace Aiyy.Extras.Cake.IIS.Monitoring;
class ConcurrentCacheHelper : IDisposable
{
	private MemoryCache _cache;
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

	public ConcurrentCacheHelper(MemoryCache cache)
	{
		_cache = cache;
	}

	public T GetOrCreate<T>(string key, Func<T> factory, MemoryCacheEntryOptions options)
	{
		GetOrCreate(key, factory, options, out T t);

		return t;
	}

	public bool GetOrCreate<T>(string key, Func<T> factory, MemoryCacheEntryOptions options, out T t)
	{
		bool created = false;

		_lock.EnterUpgradeableReadLock();

		T obj = default(T);

		try
		{
			obj = _cache.Get<T>(key);

			if (obj == null)
			{

				try
				{
					_lock.EnterWriteLock();

					obj = _cache.Get<T>(key);

					if (obj == null)
					{

						obj = factory();

						created = true;
					}

					_cache.Set(key, obj, options);
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}
		finally
		{
			_lock.ExitUpgradeableReadLock();
		}

		t = obj;

		return created;
	}

	public void Dispose()
	{
		if (_lock != null)
		{
			_lock.Dispose();
			_lock = null;
		}
	}
}