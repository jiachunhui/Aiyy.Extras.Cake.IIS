namespace Aiyy.Extras.Cake.IIS.Monitoring;

public class MemoryData
{
	private static MEMORYSTATUSEX _memoryStatus = null;

	public static long TotalInstalledMemory
	{
		get
		{
			if (_memoryStatus == null)
			{
				_memoryStatus = new MEMORYSTATUSEX();
				NativeMethods.GlobalMemoryStatusEx(_memoryStatus);
			}

			return (long)_memoryStatus.ullTotalPhys;
		}
	}
}
