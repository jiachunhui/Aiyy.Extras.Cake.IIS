using System.ComponentModel.DataAnnotations;
using Furion.ConfigurableOptions;

namespace Aiyy.Extras.Cake.IIS;

public class CakeIISOptions : IConfigurableOptions
{
	[Required(ErrorMessage = "名称不能为空")]
	public string Name { get; set; }
	/// <summary>
	/// 服务器ID 加密
	/// </summary>
	/// 
	[Required(ErrorMessage = "服务器ID不能为空")]
	public string ServerId { get; set; }
	[Required, RegularExpression(@"^[0-9][0-9\.]+[0-9]$", ErrorMessage = "不是有效的版本号")]
	public string Version { get; set; }
	/// <summary>
	/// 服务器绑定的二级域名
	/// </summary>
	/// 
	[Required(ErrorMessage = "服务器域名不能为空"), RegularExpression(@"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$", ErrorMessage = "不是有效的域名")]
	public string HostName { get; set; }
	/// <summary>
	/// 站点默认目录
	/// </summary>
	public string PhysicalDirectory { get; set; } = @"c:\\website";
	/// <summary>
	/// 密钥
	/// </summary>
	/// 
	[Required(ErrorMessage = "密钥不能为空，和服务器配置一样")]
	public string SecurityKey { get; set; }
	/// <summary>
	/// 白名单
	/// </summary>
	public string AdminSafeList { get; set; }
	/// <summary>
	/// 黑名单
	/// </summary>
	public string AdminBlackList { get; set; }

	public CakeFileOptions FileSettings { get; set; }


}


public class CakeFileOptions
{
	/// <summary>
	/// 访问目录要限制访问的目录
	/// </summary>
	public List<string> DirectoryLimit { get; set; }

	/// <summary>
	/// 可编辑文件后缀名
	/// </summary>
	public List<string> FileEditSuffix { get; set; }
}
