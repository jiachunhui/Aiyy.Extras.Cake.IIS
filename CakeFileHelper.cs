using System.Text;
using Furion;
using Furion.FriendlyException;
using Furion.VirtualFileServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aiyy.Extras.Cake.IIS;

public static class CakeFileHelper
{
	/// <summary>
	/// 读取限制目录
	/// </summary>
	/// <param name="dirPath"></param>
	private static void DicLimit(string dirPath)
	{
		var dicLimit = CakeFileConsts.DirectoryLimits;
		//读取配置文件中的限制目录
		var ss = App.GetOptionsMonitor<CakeIISOptions>();
		if (ss != null)
		{
			if (ss.FileSettings != null && ss.FileSettings.DirectoryLimit != null)
			{
				dicLimit = ss.FileSettings.DirectoryLimit;
			}

		}

		var pathAny = dicLimit.Any(q => dirPath.Replace("\\", "").Replace("/", "").ToUpper().Contains(q.Replace("\\", "").Replace("/", "").ToUpper()));
		if (pathAny)
		{
			throw Oops.Oh($"您访问的目录：{dirPath},已被限制！");
		}
	}
	/// <summary>
	/// 限制后台编辑文件
	/// </summary>
	/// <param name="filePath"></param>
	private static void FileEditSuffix(string filePath)
	{
		var fileSuffix = CakeFileConsts.FileEditSuffix;
		//读取配置文件中的限制目录
		var ss = App.GetOptionsMonitor<CakeIISOptions>();
		if (ss != null)
		{
			if (ss.FileSettings != null && ss.FileSettings.FileEditSuffix != null)
			{
				fileSuffix = ss.FileSettings.FileEditSuffix;
			}

		}

		var fileExt = Path.GetExtension(filePath).Replace(".", "").ToUpper();
		var anys = fileSuffix.Any(q => q.ToUpper().Equals(fileExt));
		if (!anys)
		{
			throw Oops.Oh($"您访问的文件不能编辑：{filePath}，{string.Join(",", fileSuffix)}");
		}
	}
	/// <summary>
	/// 目录文件列表
	/// </summary>
	/// <param name="directoryPath"></param>
	/// <returns></returns>
	public static async Task<List<CakeFileResult>> GetListAsync(string directoryPath)
	{
		//读取配置文件中的限制目录
		DicLimit(directoryPath);
		//读取
		var list = new List<CakeFileResult>();
		var physicalFileProvider = FS.GetPhysicalFileProvider(directoryPath);
		var fileinfos = physicalFileProvider.GetDirectoryContents("").OrderByDescending(q => q.IsDirectory).ThenByDescending(q => q.Name);
		foreach (var fileinfo in fileinfos)
		{
			FS.TryGetContentType(fileinfo.Name, out var contentType);
			list.Add(new CakeFileResult
			{
				Exists = fileinfo.Exists,
				IsDirectory = fileinfo.IsDirectory,
				LastModified = fileinfo.LastModified,
				Length = fileinfo.Length,
				Name = fileinfo.Name,
				PhysicalPath = fileinfo.PhysicalPath,
				ContentType = contentType,
				DirectoryPath = fileinfo.IsDirectory ? Path.GetDirectoryName(fileinfo.PhysicalPath) : null,
			});
		}
		return await Task.FromResult(list);
	}
	/// <summary>
	/// 下载文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	public static async Task<IActionResult> DownFileAsync(string filePath)
	{
		DicLimit(filePath);

		if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
		{
			throw Oops.Oh($"文件路径不完整:{filePath}");
		}
		if (!File.Exists(filePath))
		{
			throw Oops.Oh($"下载的文件不存在:{filePath}");
		}
		var fileName = Path.GetFileName(filePath);
		var fileStream = new FileStream(filePath, FileMode.Open);
		var result = new FileStreamResult(fileStream, "application/octet-stream")
		{
			FileDownloadName = fileName
		};
		return await Task.FromResult(result);
	}
	/// <summary>
	/// 删除目录或文件
	/// </summary>
	/// <param name="directorOrFilePath"></param>
	/// <returns></returns>
	public static async Task<bool> DeleteAsync(string directorOrFilePath)
	{
		DicLimit(directorOrFilePath);

		if (string.IsNullOrWhiteSpace(Path.GetExtension(directorOrFilePath)))
		{
			//没有后缀，应该是目录了
			if (!Directory.Exists(directorOrFilePath))
			{
				throw Oops.Oh($"访问的目录不存在:{directorOrFilePath}");
			}

			Directory.Delete(directorOrFilePath, true);
		}
		else
		{
			//文件
			if (!File.Exists(directorOrFilePath))
			{
				throw Oops.Oh($"访问的文件不存在:{directorOrFilePath}");
			}
			File.Delete(directorOrFilePath);
		}

		return await Task.FromResult(true);
	}
	/// <summary>
	/// 获取单个文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="encoding"></param>
	/// <returns></returns>
	public static async Task<CakeFileStreamResult> GetFileAsync(string filePath, Encoding encoding = null)
	{
		DicLimit(filePath);
		FileEditSuffix(filePath);

		if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
		{
			throw Oops.Oh($"文件路径不完整:{filePath}");
		}
		if (!File.Exists(filePath))
		{
			throw Oops.Oh($"访问的文件不存在:{filePath}");
		}
		var rootPath = Path.GetDirectoryName(filePath);
		var fileName = Path.GetFileName(filePath);
		byte[] buffer;
		var physicalFileProvider = FS.GetPhysicalFileProvider(rootPath);
		var fileinfo = new CakeFileStreamResult();
		using (Stream readStream = physicalFileProvider.GetFileInfo(fileName).CreateReadStream())
		{
			buffer = new byte[readStream.Length];
			await readStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
		}
		// 读取文件内容
		var content = encoding.GetString(buffer);
		var fileInfo = physicalFileProvider.GetFileInfo(fileName);

		FS.TryGetContentType(fileinfo.Name, out var contentType);
		fileinfo.Exists = fileInfo.Exists;
		fileinfo.IsDirectory = fileInfo.IsDirectory;
		fileinfo.LastModified = fileInfo.LastModified;
		fileinfo.Length = fileInfo.Length;
		fileinfo.Name = fileInfo.Name;
		fileinfo.PhysicalPath = fileInfo.PhysicalPath;
		fileinfo.ContentType = contentType;

		fileinfo.CreateReadStream = content;

		return await Task.FromResult(fileinfo);
	}
	/// <summary>
	/// 编辑文本类
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="fileContent"></param>
	/// <param name="encoding"></param>
	/// <returns></returns>
	public static async Task<bool> PostEditAsync(string filePath, string fileContent, Encoding encoding = null)
	{
		DicLimit(filePath);
		FileEditSuffix(filePath);
		//文件
		if (!File.Exists(filePath))
		{
			throw Oops.Oh($"访问的文件不存在:{filePath}");
		}
		if (string.IsNullOrWhiteSpace(fileContent))
		{
			throw Oops.Oh($"文件内容不能为空:{filePath}");
		}
		using (var stream = new StreamWriter(filePath, false, encoding))
		{
			await stream.WriteAsync(fileContent);
		}

		return true;
	}
	/// <summary>
	/// 上传文件
	/// </summary>
	/// <param name="rootPath"></param>
	/// <param name="file"></param>
	/// <returns></returns>
	public static async Task<bool> PostUpfileAsync(string rootPath, IFormFile file)
	{
		DicLimit(rootPath);
		if (!Directory.Exists(rootPath))
		{
			throw Oops.Oh($"访问的目录不存在:{rootPath}");
		}
		if (file == null)
		{
			throw Oops.Oh($"上传文件不存在{file.FileName}");
		}
		//开通上传文件夹 file.FileName 是带了路径的
		var dicPath = Path.GetDirectoryName(file.FileName);
		var fileName = Path.GetFileName(file.FileName);

		var newDicPath = Path.Combine(rootPath, dicPath);
		if (!Directory.Exists(newDicPath))
		{
			Directory.CreateDirectory(newDicPath);
		}
		var filePath = Path.Combine(newDicPath, fileName);
		using (var stream = System.IO.File.Create(filePath))
		{
			await file.CopyToAsync(stream);
		}
		return true;
	}
}
