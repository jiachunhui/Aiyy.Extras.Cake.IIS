using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiyy.Extras.Cake.IIS;
/// <summary>
/// 文件信息
/// </summary>
public class CakeFileResult
{
	/// <summary>
	/// 是否存在
	/// </summary>
	public bool Exists { get; set; }

	/// <summary>
	/// 是否目录
	/// </summary>
	public bool IsDirectory { get; set; }

	/// <summary>
	/// 最后修改日期
	/// </summary>
	public DateTimeOffset LastModified { get; set; }

	/// <summary>
	/// 目录/文件创建日期
	/// </summary>
	public DateTime? CreationTime
	{
		get
		{
			if (IsDirectory)
			{
				return PhysicalPath.ToDirectoryCreateDataTime();
			}
			else
			{
				FileInfo fileInfo = new FileInfo(PhysicalPath);
				if (fileInfo.Exists)
				{
					return fileInfo.CreationTime;
				}
				return null;
			}
		}
	}

	/// <summary>
	/// 文件大小 目录 -1
	/// </summary>
	public long Length { get; set; }

	/// <summary>
	/// 文件/目录名
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// 物理路径
	/// </summary>
	public string PhysicalPath { get; set; }

	/// <summary>
	/// 目录名
	/// </summary>
	public string DirectoryPath { get; set; }

	/// <summary>
	/// 文件 ContentType
	/// </summary>
	public string ContentType { get; set; }

	/// <summary>
	/// 文件大小 转换成KB MB GB  直接获取目录大小，不现实
	/// </summary>
	public string LengthStr
	{
		get
		{
			if (Length < 0)
			{
				return "0 B";
			}
			return Length.ToFileSize();
		}
	}
}
public class CakeFileStreamResult : CakeFileResult
{
	public string CreateReadStream { get; set; }
}

public static class CakeFileConsts
{
	public static List<string> DirectoryLimits { get
		{

			var result = new List<string>();
			result.Add(@"C:\\Windows");
			result.Add(@"C:\\Program Files");
			result.Add(@"C:\\Program Files (x86)");
			result.Add(@"C:\\Users");
			return result;
		} }


	/// <summary>
	/// 文件编辑后缀
	/// </summary>
	public static List<string> FileEditSuffix {
		get
		{

			var result = new List<string>();
			result.Add(@"txt");
			result.Add(@"js");
			result.Add(@"html");
			return result;
		}
	}
}