using Furion;
using Furion.DataEncryption.Extensions;

namespace Aiyy.Extras.Cake.IIS;

public static class UtilsExtends
{
	/// <summary>
	/// 所有的加密站点ID
	/// </summary>
	public static string SecurityKey
	{
		get
		{
			var skey = App.GetOptionsMonitor<CakeIISOptions>().SecurityKey;

			if (string.IsNullOrWhiteSpace(skey))
			{
				return "30EA5D5B7A224F94890002DFDD42C2B9";
			}
			return skey;
		}
	}
	/// <summary>
	/// 站点ID加密码输出
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static string ToSiteIdEncrypt(this long id)
	{

		return id.ToString().ToDESCEncrypt(SecurityKey, true);
	}
	/// <summary>
	/// 站点ID加密码输出
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static string ToSiteIdEncrypt(this string id)
	{

		return id.ToString().ToDESCEncrypt(SecurityKey, true);
	}
	/// <summary>
	/// 站点ID解密
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static string ToSiteIdDecrypt(this string id)
	{

		return id.ToDESCDecrypt(SecurityKey, true);
	}

	/// <summary>
	/// 把文件长度long 转换成能看懂的单位
	/// </summary>
	/// <param name="fileLength"></param>
	/// <returns>b,kb,mb,gb</returns>
	public static string ToFileSize(this long fileLength)
	{
		const int GB = 1024 * 1024 * 1024;
		const int MB = 1024 * 1024;
		const int KB = 1024;

		if (fileLength / GB >= 1)
		{
			return Math.Round(fileLength / (float)GB, 2) + "GB";
		}

		if (fileLength / MB >= 1)
		{
			return Math.Round(fileLength / (float)MB, 2) + "MB";
		}

		if (fileLength / KB >= 1)
		{
			return Math.Round(fileLength / (float)KB, 2) + "KB";
		}

		return fileLength + "B";
	}
	/// <summary>
	/// 获取目录的创建时间
	/// </summary>
	/// <param name="dirPath"></param>
	/// <returns></returns>
	public static DateTime? ToDirectoryCreateDataTime(this string dirPath)
	{
		if (!Directory.Exists(dirPath))
		{
			return null;
		}
		DirectoryInfo di = new DirectoryInfo(dirPath);

		return di.CreationTime;
	}
	/// <summary>
	/// 获取目录 所有文件大小
	/// </summary>
	/// <param name="dirPath"></param>
	/// <returns></returns>
	public static long GetDirectorySize(string dirPath)
	{
		//判断给定的路径是否存在,如果不存在则退出
		if (!Directory.Exists(dirPath))
			return 0;
		long len = 0;
		//定义一个DirectoryInfo对象
		DirectoryInfo di = new DirectoryInfo(dirPath);
		//通过GetFiles方法,获取di目录中的所有文件的大小
		foreach (FileInfo fi in di.GetFiles())
		{
			len += fi.Length;
		}
		//获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
		DirectoryInfo[] dis = di.GetDirectories();
		if (dis.Length > 0)
		{
			for (int i = 0; i < dis.Length; i++)
			{
				len += GetDirectorySize(dis[i].FullName);
			}
		}
		return len;
	}

}
