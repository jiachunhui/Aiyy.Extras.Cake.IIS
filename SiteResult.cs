using Cake.IIS;

namespace Aiyy.Extras.Cake.IIS;
public class SiteBaseResult
{
	/// <summary>
	/// 名称
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// 加密后ID
	/// </summary>
	public string Id { get; set; }
	/// <summary>
	/// 状态
	/// </summary>
	public string Status { get; set; }
}
/// <summary>
/// 站点名称
/// </summary>
public class SiteResult : SiteBaseResult
{
	/// <summary>
	/// 应用启用获取或设置目录浏览的标志
	/// </summary>
	public bool EnableDirectoryBrowsing { get; set; }
	/// <summary>
	/// 物理路径
	/// </summary>
	/// 
	public string PhysicalPath { get; set; }
	/// <summary>
	/// 服务器自动启动
	/// </summary>
	public bool ServerAutoStart { get; set; }
	/// <summary>
	/// 启用的协议
	/// </summary>
	public string EnabledProtocols { get; set; }
	public List<BindingResult> Binding { get; set; }
	
	public SiteBaseResult ApplicationPool { get; set; }
	/// <summary>
	/// 备用启用协议
	/// </summary>
	public string AlternateEnabledProtocols { get; set; }
	/// <summary>
	/// 跟踪失败请求已启用
	/// </summary>
	public bool TraceFailedRequestsEnabled { get; set; }
	/// <summary>
	/// 跟踪失败的请求目录
	/// </summary>
	public string TraceFailedRequestsDirectory { get; set; }
	/// <summary>
	/// 跟踪失败请求最大日志文件
	/// </summary>
	public long TraceFailedRequestsMaxLogFiles { get; set; }

	

}
/// <summary>
/// 站点目录
/// </summary>
public class SiteDirResult:SiteResult
{
	/// <summary>
	/// 站点目录 大小
	/// </summary>
	public long PhysicalDirectoryLength
	{
		get
		{
			var dirPath = PhysicalPath;
			var length = UtilsExtends.GetDirectorySize(dirPath);
			return length;
		}
	}

	/// <summary>
	/// 站点大小 说明
	/// </summary>
	public string PhysicalDirectoryLengthStr
	{
		get { return PhysicalDirectoryLength.ToFileSize(); }
	}

	/// <summary>
	/// 站点目录创建时间
	/// </summary>
	public DateTime? PhysicalDirectoryCreateDataTime
	{
		get
		{
			//这就是服务器上的目录
			var dirPath = PhysicalPath;

			return dirPath.ToDirectoryCreateDataTime();
		}
	}
}

public class BindingResult
{
	/// <summary>
	/// 获取或设置IP地址
	/// </summary>
	public string IpAddress { get; set; }

	/// <summary>
	/// 获取或设置IP端口
	/// </summary>
	public int Port { get; set; }

	/// <summary>
	/// 获取或设置绑定的主机名
	/// </summary>
	public string HostName { get; set; }

	/// <summary>
	/// 获取或设置特定证书的哈希。
	/// </summary>
	public byte[] CertificateHash { get; set; }

	/// <summary>
	/// 获取或设置证书存储的名称
	/// </summary>
	public string CertificateStoreName { get; set; }

	/// <summary>
	/// 获取IIS绑定类型。
	/// </summary>>
	/// <returns>
	/// </returns>
	public string BindingProtocol { get; set; }

	/// <summary>
	/// 获取或设置此绑定是否需要服务器名称指示（SNI）
	/// </summary>
	/// <remarks>
	/// 此设置仅对HTTPS绑定有效。
	/// </remarks>
	public bool RequireServerNameIndication { get; set; }

	/// <summary>
	/// 获取IIS绑定信息
	/// </summary>
	/// <returns>
	/// 返回设置特定绑定类型所需的绑定属性的详细信息。
	/// </returns>
	public virtual string BindingInformation
	{
		get
		{
			return $"{IpAddress}:{Port}:{HostName}";
		}
	}
}
/// <summary>
/// 程序池输出
/// </summary>
public class ApplicationPoolResult: SiteBaseResult
{
	/// <summary>
	/// 标识 LocalSystem /LocalService /NetworkService /ApplicationPoolIdentity /SpecificUser
	/// </summary>
	public string IdentityType { get; set; }

	/// <summary>
	/// 用户名
	/// </summary>
	public string Username { get; set; }

	/// <summary>
	/// 密码
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	/// net clr版本号
	/// </summary>

	public string ManagedRuntimeVersion { get; set; }

	/// <summary>
	/// 常规-托管管道模式 Classic==ture
	/// </summary>
	public bool ClassicManagedPipelineMode { get; set; }

	/// <summary>
	/// 常规-启用32位应用程序
	/// </summary>
	public bool Enable32BitAppOnWin64 { get; set; }

	/// <summary>
	/// 加载用户配置文件
	/// </summary>
	public bool LoadUserProfile { get; set; }

	/// <summary>
	/// 最大进程
	/// </summary>
	public long MaxProcesses { get; set; }

	/// <summary>
	/// 启用ping
	/// </summary>
	public bool PingingEnabled { get; set; }

	/// <summary>
	/// 进程模型 Ping 间隔（秒）
	/// </summary>
	public TimeSpan PingInterval { get; set; }

	/// <summary>
	/// 进程模型 Ping 最大响应（秒）
	/// </summary>
	public TimeSpan PingResponseTime { get; set; }

	/// <summary>
	/// 进程模型 闲置超时（分）
	/// </summary>
	public TimeSpan IdleTimeout { get; set; }

	/// <summary>
	/// 进程模型 关闭时间限制（秒）
	/// </summary>
	public TimeSpan ShutdownTimeLimit { get; set; }

	/// <summary>
	/// 进程模型 启动时间限制（秒）
	/// </summary>
	public TimeSpan StartupTimeLimit { get; set; }

	public bool Autostart { get; set; }
}
