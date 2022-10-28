using System.Dynamic;
using System.Net;
using System.Runtime.InteropServices;
using Aiyy.Extras.Cake.IIS;
using Aiyy.Extras.Cake.IIS.Monitoring;
using Cake.Core;
using Cake.IIS;
using Furion;
using Furion.FriendlyException;
using Microsoft.Web.Administration;

namespace Aiyy.Cake.IIS;

/// <summary>
/// Cake IIS Helper
/// </summary>
public static class CakeIISHelper
{
	private const string IdleTimeoutActionAttribute = "idleTimeoutAction";
	public static ICakeEnvironment CreateEnvironment()
	{
		var environment = new CakeEnvironment(new CakePlatform(), new CakeRuntime());
		return environment;
	}

	#region 管理器

	/// <summary>
	/// 应用程序池管理器
	/// </summary>
	/// <returns></returns>
	public static ApplicationPoolManager CreateApplicationPoolManager()
	{
		ApplicationPoolManager manager = new ApplicationPoolManager(CreateEnvironment(), new CakeIISLog());
		manager.SetServer();
		return manager;
	}

	/// <summary>
	/// 重写规则管理器
	/// </summary>
	/// <returns></returns>
	public static RewriteManager CreateRewriteManager()
	{
		RewriteManager manager = new RewriteManager(CreateEnvironment(), new CakeIISLog());
		manager.SetServer();
		return manager;
	}

	/// <summary>
	/// FTP 管理器
	/// </summary>
	/// <returns></returns>
	public static FtpsiteManager CreateFtpsiteManager()
	{
		FtpsiteManager manager = new FtpsiteManager(CreateEnvironment(), new CakeIISLog());
		manager.SetServer();
		return manager;
	}

	/// <summary>
	/// 站点管理器
	/// </summary>
	/// <returns></returns>
	public static WebsiteManager CreateWebsiteManager()
	{
		WebsiteManager manager = new WebsiteManager(CreateEnvironment(), new CakeIISLog());
		manager.SetServer();
		return manager;
	}

	/// <summary>
	/// WebFarm Manager
	/// </summary>
	/// <returns></returns>
	public static WebFarmManager CreateWebFarmManager()
	{
		WebFarmManager manager = new WebFarmManager(CreateEnvironment(), new CakeIISLog());
		manager.SetServer();
		return manager;
	}

	#endregion 管理器

	#region 设置项 settings

	/// <summary>
	/// 应用程序池 设置
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ApplicationPoolSettings GetAppPoolSettings(string name = "DC")
	{
		return new ApplicationPoolSettings
		{
			Name = name,
			IdentityType = IdentityType.NetworkService,
			Autostart = true,
			MaxProcesses = 1,
			Enable32BitAppOnWin64 = false,

			IdleTimeout = TimeSpan.FromMinutes(20),
			ShutdownTimeLimit = TimeSpan.FromSeconds(90),
			StartupTimeLimit = TimeSpan.FromSeconds(90),

			PingingEnabled = true,
			PingInterval = TimeSpan.FromSeconds(30),
			PingResponseTime = TimeSpan.FromSeconds(90),
			Overwrite = false
		};
	}

	/// <summary>
	/// 重写规则 设置
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static RewriteRuleSettings GetRewriteRuleSettings(string name)
	{
		return new RewriteRuleSettings
		{
			Name = name,
			Pattern = "*",
			PatternSintax = RewritePatternSintax.Wildcard,
			IgnoreCase = true,
			StopProcessing = true,
			Conditions = new[]
			{
					new RewriteRuleConditionSettings {ConditionInput = "{HTTPS}", Pattern = "off", IgnoreCase = true},
				},
			Action = new RewriteRuleRedirectAction
			{
				Url = @"https://{HTTP_HOST}{REQUEST_URI}",
				RedirectType = RewriteRuleRedirectType.Found
			}
		};
	}

	/// <summary>
	/// 站点设置项
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static WebsiteSettings GetWebsiteSettings(string name = "Superman")
	{
		WebsiteSettings settings = new WebsiteSettings
		{
			Name = name,
			PhysicalDirectory = "./Test/" + name,
			ApplicationPool = GetAppPoolSettings(),
			ServerAutoStart = true,
			Overwrite = false
		};

		settings.Binding = IISBindings.Http
			.SetHostName(name + ".web")
			.SetIpAddress("*")
			.SetPort(80);

		return settings;
	}

	/// <summary>
	/// 应用程序设置
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ApplicationSettings GetApplicationSettings(string name)
	{
		return new ApplicationSettings
		{
			ApplicationPath = "/Test",
			ApplicationPool = GetAppPoolSettings().Name,
			VirtualDirectory = "/",
			PhysicalDirectory = "./Test/App/",
			SiteName = name,
		};
	}

	/// <summary>
	/// GetWebFarmSettings
	/// </summary>
	/// <returns></returns>
	public static WebFarmSettings GetWebFarmSettings()
	{
		return new WebFarmSettings
		{
			Name = "Batman",
			Servers = new string[] { "Gotham", "Metroplis" }
		};
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="anonymous"></param>
	/// <param name="basic"></param>
	/// <param name="windows"></param>
	/// <returns></returns>
	public static AuthenticationSettings GetAuthenticationSettings(bool? anonymous, bool? basic, bool? windows)
	{
		return new AuthenticationSettings()
		{
			EnableAnonymousAuthentication = anonymous,
			EnableBasicAuthentication = basic,
			EnableWindowsAuthentication = windows
		};
	}

	#endregion 设置项 settings

	#region  method


	//Website
	public static void CreateWebsite(WebsiteSettings settings)
	{
		WebsiteManager manager = CreateWebsiteManager();
		manager.Create(settings);
	}
	/// <summary>
	/// 删除网站:name
	/// </summary>
	/// <param name="name"></param>
	public static void DeleteWebsite(string name)
	{
		using (var server = new ServerManager())
		{
			var site = server.Sites.FirstOrDefault(x => x.Name == name);
			if (site != null)
			{
				server.Sites.Remove(site);
				server.CommitChanges();
			}
		}
	}
	/// <summary>
	/// 删除网站:id
	/// </summary>
	/// <param name="id"></param>
	public static void DeleteWebsite(long id)
	{
		using (var server = new ServerManager())
		{
			var site = server.Sites.FirstOrDefault(x => x.Id == id);
			if (site != null)
			{
				server.Sites.Remove(site);
				server.CommitChanges();
			}
		}
	}
	/// <summary>
	/// 根据站点名，获取
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static Site GetWebsite(string name)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Name == name);
			if (site != null && site.ApplicationDefaults != null)
			{
				return site;
			}
			return site;
		}
	}
	/// <summary>
	/// 根据站点ID 
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static Site GetWebsite(long id)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Id == id);
			if (site != null && site.ApplicationDefaults != null)
			{
				return site;
			}
			return site;
		}
	}
	public static SiteResult GetWebsiteResult(long id)
	{
		using (var serverManager = new ServerManager())
		{
			var item = serverManager.Sites.FirstOrDefault(x => x.Id == id);
			if (item != null && item.ApplicationDefaults != null)
			{
				var site = new SiteResult();
				site.Name = item.Name;
				site.Id = item.Id.ToSiteIdEncrypt();
				site.Status = item.State.ToString().ToLower();
				//site.EnableDirectoryBrowsing = false,//这个暂不知道怎么获取
				site.PhysicalPath = item.Applications["/"].VirtualDirectories["/"].PhysicalPath;
				//site.AlternateEnabledProtocols = item.ApplicationDefaults.EnabledProtocols;
				//site.TraceFailedRequestsEnabled = item.TraceFailedRequestsLogging.Enabled;
				//site.TraceFailedRequestsDirectory = item.TraceFailedRequestsLogging.Directory;
				//site.TraceFailedRequestsMaxLogFiles = item.TraceFailedRequestsLogging.MaxLogFiles;
				//site.ServerAutoStart = item.ServerAutoStart;

				//邦定信息
				var bindlist = new List<BindingResult>();
				foreach (var itemBind in item.Bindings)
				{

					bindlist.Add(new BindingResult
					{

						IpAddress = (!object.Equals(itemBind.EndPoint.Address, IPAddress.Any) ? itemBind.EndPoint.Address.ToString() : "*"),
						Port = itemBind.EndPoint.Port,
						HostName = itemBind.Host,
						CertificateHash = itemBind.CertificateHash,
						CertificateStoreName = itemBind.CertificateStoreName,
						BindingProtocol = itemBind.Protocol,
						RequireServerNameIndication = itemBind.SslFlags == SslFlags.Sni,
					});
				}
				var appPool = new ApplicationPoolResult();
				var app = item.Applications[0];


				site.Binding = bindlist;
				site.ApplicationPool = new SiteBaseResult { Name = app.ApplicationPoolName, Status = GetApplicationPoolFind(app.ApplicationPoolName).Status.ToLower() };

				return site;
			}
			return null;
		}
	}
	/// <summary>
	/// 根据ID 获取站点名
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static string GetWebsiteName(long id)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Id == id);
			if (site != null && site.ApplicationDefaults != null)
			{
				return site.Name;
			}
			return null;
		}
	}
	/// <summary>
	/// 查找站点是否存在
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static bool GetWebsiteExists(string name)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Name == name);
			if (site != null && site.ApplicationDefaults != null)
			{
				return true;
			}
			return false;
		}
	}

	public static Application GetApplication(string siteName, string appPath)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Name == siteName);
			return site != null ? site.Applications.FirstOrDefault(a => a.Path == appPath) : null;
		}
	}

	public static object GetWebConfigurationValue(string siteName, string appPath, string section, string key)
	{
		using (var serverManager = new ServerManager())
		{
			var site = serverManager.Sites.FirstOrDefault(x => x.Name == siteName);
			Configuration config;
			if (appPath != null)
			{
				var app = site?.Applications.FirstOrDefault(a => a.Path == appPath);
				config = app?.GetWebConfiguration();
			}
			else
			{
				config = site?.GetWebConfiguration();
			}
			var sectionObject = config?.GetSection(section);
			return sectionObject?[key];
		}
	}
	/// <summary>
	/// 开启站点
	/// </summary>
	/// <param name="name"></param>
	public static void StartWebsite(string name)
	{
		using (var server = new ServerManager())
		{
			Site site = server.Sites.FirstOrDefault(x => x.Name == name);

			if (site != null)
			{
				try
				{
					site.Start();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
	/// <summary>
	/// 开启站点
	/// </summary>
	/// <param name="id"></param>
	public static void StartWebsite(long id)
	{
		using (var server = new ServerManager())
		{
			Site site = server.Sites.FirstOrDefault(x => x.Id == id);

			if (site != null)
			{
				try
				{
					site.Start();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
	/// <summary>
	/// 关闭站点
	/// </summary>
	/// <param name="name"></param>
	public static void StopWebsite(string name)
	{
		using (var server = new ServerManager())
		{
			Site site = server.Sites.FirstOrDefault(x => x.Name == name);

			if (site != null)
			{
				try
				{
					site.Stop();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
	/// <summary>
	/// 关闭站点
	/// </summary>
	/// <param name="id"></param>
	public static void StopWebsite(long id)
	{
		using (var server = new ServerManager())
		{
			Site site = server.Sites.FirstOrDefault(x => x.Id == id);

			if (site != null)
			{
				try
				{
					site.Stop();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}

	//Pool
	public static void CreatePool(ApplicationPoolSettings settings)
	{
		ApplicationPoolManager manager = CreateApplicationPoolManager();

		manager.Create(settings);
	}

	public static void DeletePool(string name)
	{
		using (var server = new ServerManager())
		{
			ApplicationPool pool = server.ApplicationPools.FirstOrDefault(x => x.Name == name);

			if (pool != null)
			{
				server.ApplicationPools.Remove(pool);
				server.CommitChanges();
			}
		}
	}
	public static ApplicationPool GetAppPool(string name)
	{
		using (var server = new ServerManager())
		{
			var pool = server.ApplicationPools.FirstOrDefault(x => x.Name == name);
			return pool;
		}
	}
	/// <summary>
	/// 获取程序池
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static dynamic GetPool(string name)
	{
		using (var server = new ServerManager())
		{
			var pool = server.ApplicationPools.FirstOrDefault(x => x.Name == name);
			if (pool == null)
			{
				throw new Exception($"程序池：{name},不存在");
			}

			dynamic obj = new ExpandoObject();
			//
			// name
			obj.name = pool.Name;

			//
			// id
			obj.id = pool.Name.ToSiteIdEncrypt();
			ObjectState state = ObjectState.Unknown;
			try
			{
				state = pool.State;
			}
			catch (COMException)
			{
				// Problem getting state of app pool. Possible reasons:
				// 1. App pool's application pool was deleted.
				// 2. App pool was just created and the status is not accessible yet.
			}
			//catch (System.UnauthorizedAccessException) {
			//    // do nothing
			//}
			obj.status = System.Enum.GetName(typeof(ObjectState), state).ToLower();
			obj.auto_start = pool.AutoStart;
			obj.pipeline_mode = System.Enum.GetName(typeof(ManagedPipelineMode), pool.ManagedPipelineMode).ToLower();
			obj.managed_runtime_version = pool.ManagedRuntimeVersion;
			obj.enable_32bit_win64 = pool.Enable32BitAppOnWin64;

			obj.queue_length = pool.QueueLength;
			obj.start_mode = System.Enum.GetName(typeof(StartMode), pool.StartMode);
			obj.cpu = new
			{
				limit = pool.Cpu.Limit,
				limit_interval = pool.Cpu.ResetInterval.TotalMinutes,
				action = System.Enum.GetName(typeof(ProcessorAction), pool.Cpu.Action),
				processor_affinity_enabled = pool.Cpu.SmpAffinitized,
				processor_affinity_mask32 = "0x" + pool.Cpu.SmpProcessorAffinityMask.ToString("X"),
				processor_affinity_mask64 = "0x" + pool.Cpu.SmpProcessorAffinityMask2.ToString("X")
			};
			dynamic processModel = new ExpandoObject();

			processModel.idle_timeout = pool.ProcessModel.IdleTimeout.TotalMinutes;
			processModel.max_processes = pool.ProcessModel.MaxProcesses;
			processModel.pinging_enabled = pool.ProcessModel.PingingEnabled;
			processModel.ping_interval = pool.ProcessModel.PingInterval.TotalSeconds;
			processModel.ping_response_time = pool.ProcessModel.PingResponseTime.TotalSeconds;
			processModel.shutdown_time_limit = pool.ProcessModel.ShutdownTimeLimit.TotalSeconds;
			processModel.startup_time_limit = pool.ProcessModel.StartupTimeLimit.TotalSeconds;

			if (pool.ProcessModel.Schema.HasAttribute(IdleTimeoutActionAttribute))
			{
				processModel.idle_timeout_action = System.Enum.GetName(typeof(IdleTimeoutAction), pool.ProcessModel.IdleTimeoutAction);
			}

			obj.process_model = processModel;

			obj.identity = new
			{
				// Not changing the casing or adding '_' on the identity type enum because they represent identities and therefore spelling and casing are important
				identity_type = System.Enum.GetName(typeof(ProcessModelIdentityType), pool.ProcessModel.IdentityType),
				username = pool.ProcessModel.UserName,
				load_user_profile = pool.ProcessModel.LoadUserProfile
			};

			RecyclingLogEventOnRecycle logEvent = pool.Recycling.LogEventOnRecycle;

			Dictionary<string, bool> logEvents = new Dictionary<string, bool>();
			logEvents.Add("time", logEvent.HasFlag(RecyclingLogEventOnRecycle.Time));
			logEvents.Add("requests", logEvent.HasFlag(RecyclingLogEventOnRecycle.Requests));
			logEvents.Add("schedule", logEvent.HasFlag(RecyclingLogEventOnRecycle.Schedule));
			logEvents.Add("memory", logEvent.HasFlag(RecyclingLogEventOnRecycle.Memory));
			logEvents.Add("isapi_unhealthy", logEvent.HasFlag(RecyclingLogEventOnRecycle.IsapiUnhealthy));
			logEvents.Add("on_demand", logEvent.HasFlag(RecyclingLogEventOnRecycle.OnDemand));
			logEvents.Add("config_change", logEvent.HasFlag(RecyclingLogEventOnRecycle.ConfigChange));
			logEvents.Add("private_memory", logEvent.HasFlag(RecyclingLogEventOnRecycle.PrivateMemory));

			obj.recycling = new
			{
				disable_overlapped_recycle = pool.Recycling.DisallowOverlappingRotation,
				disable_recycle_on_config_change = pool.Recycling.DisallowRotationOnConfigChange,
				log_events = logEvents,
				periodic_restart = new
				{
					time_interval = pool.Recycling.PeriodicRestart.Time.TotalMinutes,
					private_memory = pool.Recycling.PeriodicRestart.PrivateMemory,
					request_limit = pool.Recycling.PeriodicRestart.Requests,
					virtual_memory = pool.Recycling.PeriodicRestart.Memory,
					schedule = pool.Recycling.PeriodicRestart.Schedule.Select(s => s.Time.ToString(@"hh\:mm"))
				}
			};
			obj.rapid_fail_protection = new
			{
				enabled = pool.Failure.RapidFailProtection,
				load_balancer_capabilities = System.Enum.GetName(typeof(LoadBalancerCapabilities), pool.Failure.LoadBalancerCapabilities),
				interval = pool.Failure.RapidFailProtectionInterval.TotalMinutes,
				max_crashes = pool.Failure.RapidFailProtectionMaxCrashes,
				auto_shutdown_exe = pool.Failure.AutoShutdownExe,
				auto_shutdown_params = pool.Failure.AutoShutdownParams
			};
			obj.process_orphaning = new
			{
				enabled = pool.Failure.OrphanWorkerProcess,
				orphan_action_exe = pool.Failure.OrphanActionExe,
				orphan_action_params = pool.Failure.OrphanActionParams,
			};


			//var pool = new ApplicationPoolResult();
			//pool.Name = pools.Name;
			//pool.ManagedRuntimeVersion = pools.ManagedRuntimeVersion;
			//pool.Autostart = pools.AutoStart;

			//pool.Status = pools.State.ToString();



			//pool.Enable32BitAppOnWin64 = pools.Enable32BitAppOnWin64;
			//if (pools.ManagedPipelineMode == ManagedPipelineMode.Classic)
			//{
			//}
			//pool.ClassicManagedPipelineMode = pools.ManagedPipelineMode == ManagedPipelineMode.Classic;
			////进程模型
			//pool.LoadUserProfile = pools.ProcessModel.LoadUserProfile;
			//pool.MaxProcesses = pools.ProcessModel.MaxProcesses;
			//pool.PingingEnabled = pools.ProcessModel.PingingEnabled;
			//pool.PingInterval = pools.ProcessModel.PingInterval;
			//pool.PingResponseTime = pools.ProcessModel.PingResponseTime;
			//pool.IdleTimeout = pools.ProcessModel.IdleTimeout;
			//pool.ShutdownTimeLimit = pools.ProcessModel.ShutdownTimeLimit;
			//pool.StartupTimeLimit = pools.ProcessModel.StartupTimeLimit;

			//switch (pools.ProcessModel.IdentityType)
			//{
			//	case ProcessModelIdentityType.LocalSystem:
			//		pool.IdentityType = IdentityType.LocalSystem.ToString();
			//		break;

			//	case ProcessModelIdentityType.LocalService:
			//		pool.IdentityType = IdentityType.LocalService.ToString();
			//		break;

			//	case ProcessModelIdentityType.NetworkService:
			//		pool.IdentityType = IdentityType.NetworkService.ToString();
			//		break;

			//	case ProcessModelIdentityType.ApplicationPoolIdentity:
			//		pool.IdentityType = IdentityType.ApplicationPoolIdentity.ToString();
			//		break;

			//	case ProcessModelIdentityType.SpecificUser:
			//		pool.IdentityType = IdentityType.SpecificUser.ToString();
			//		pool.Username = pools.ProcessModel.UserName;
			//		pool.Password = pools.ProcessModel.Password;
			//		break;

			//	default:
			//		throw new ArgumentOutOfRangeException();
			//}
			return obj;
		}
	}
	/// <summary>
	/// 开启程序池
	/// </summary>
	/// <param name="name"></param>
	public static void StartPool(string name)
	{
		using (var server = new ServerManager())
		{
			ApplicationPool pool = server.ApplicationPools.FirstOrDefault(x => x.Name == name);

			if (pool != null)
			{
				try
				{
					pool.Start();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
	/// <summary>
	/// 关闭程序池
	/// </summary>
	/// <param name="name"></param>
	public static void StopPool(string name)
	{
		using (var server = new ServerManager())
		{
			ApplicationPool pool = server.ApplicationPools.FirstOrDefault(x => x.Name == name);

			if (pool != null)
			{
				try
				{
					pool.Stop();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}



	//Config 
	public static AuthenticationSettings ReadAuthenticationSettings(string siteName = null, string appPath = null)
	{
		var location = siteName != null ? siteName + (appPath ?? "") : null;
		var element = "system.webServer/security/authentication/{0}";

		var anon = GetSectionElementValue<bool>(string.Format(element, "anonymousAuthentication"), "enabled", location);
		var basic = GetSectionElementValue<bool>(string.Format(element, "basicAuthentication"), "enabled", location);
		var windows = GetSectionElementValue<bool>(string.Format(element, "windowsAuthentication"), "enabled", location);

		return new AuthenticationSettings()
		{
			EnableWindowsAuthentication = windows,
			EnableBasicAuthentication = basic,
			EnableAnonymousAuthentication = anon
		};
	}

	public static T GetSectionElementValue<T>(string elementPath, string attributeName, string location)
	{
		using (var serverManager = new ServerManager())
		{
			var config = serverManager.GetApplicationHostConfiguration();
			var element = location == null ? config.GetSection(elementPath) : config.GetSection(elementPath, location);
			var t = typeof(T);
			return (T)Convert.ChangeType(element[attributeName], t);
		}
	}



	//WebFarm
	public static void CreateWebFarm(WebFarmSettings settings)
	{
		WebFarmManager manager = CreateWebFarmManager();

		manager.Create(settings);
	}

	public static void DeleteWebFarm(string name)
	{
		using (var serverManager = new ServerManager())
		{
			Configuration config = serverManager.GetApplicationHostConfiguration();

			ConfigurationSection section = config.GetSection("webFarms");
			ConfigurationElementCollection farms = section.GetCollection();

			ConfigurationElement farm = farms.FirstOrDefault(f => f.GetAttributeValue("name").ToString() == name);

			if (farm != null)
			{
				farms.Remove(farm);
				serverManager.CommitChanges();
			}
		}
	}

	public static ConfigurationElement GetWebFarm(string name)
	{
		using (var serverManager = new ServerManager())
		{
			Configuration config = serverManager.GetApplicationHostConfiguration();

			ConfigurationSection section = config.GetSection("webFarms");
			ConfigurationElementCollection farms = section.GetCollection();

			return farms.FirstOrDefault(f => f.GetAttributeValue("name").ToString() == name);
		}
	}

	public static void CreateWebConfig(IDirectorySettings settings)
	{
		var framework = "net461";
#if NET461
            var framework = "net461";
#elif NETCOREAPP3_1
            var framework = "netcoreapp3.1";
#elif NET5_0
            var framework = "net5.0";
#endif
		var folder = Directory.GetCurrentDirectory().Replace("\\", "/").Replace($"/bin/Debug/{framework}", "/") + settings.PhysicalDirectory.FullPath;

		// Make sure the directory exists (for configs)
		Directory.CreateDirectory(folder);

		// Create the web.config
		const string webConfig = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<configuration>\r\n</configuration>";
		File.WriteAllText(Path.Combine(folder, "web.config"), webConfig);
	}

	//Rewrite
	public static void CreateRewriteRule(RewriteRuleSettings settings)
	{
		var rewriteRule = CreateRewriteManager();

		rewriteRule.CreateRule(settings);
	}

	public static bool ExistsRewriteRule(string name)
	{
		var rewriteRule = CreateRewriteManager();

		return rewriteRule.Exists(name);
	}

	public static bool DeleteRewriteRule(string name)
	{
		var rewriteRule = CreateRewriteManager();

		return rewriteRule.DeleteRule(name);
	}

	#endregion

	#region List
	public static List<SiteBaseResult> GetSiteList()
	{
		using (var server = new ServerManager())
		{
			var list = new List<SiteBaseResult>();
			var sites = server.Sites;
			foreach (var item in sites)
			{
				var site = new SiteBaseResult();
				site.Name = item.Name;
				site.Id = item.Id.ToSiteIdEncrypt();
				site.Status = item.State.ToString();
				list.Add(site);

			}
			return list;
		}
	}
	public static List<SiteResult> GetSiteAllList()
	{
		using (var server = new ServerManager())
		{
			var list = new List<SiteResult>();
			var sites = server.Sites;
			foreach (var item in sites)
			{
				var site = new SiteResult();
				site.Name = item.Name;
				site.Id = item.Id.ToSiteIdEncrypt();
				site.Status = item.State.ToString();
				//site.EnableDirectoryBrowsing = false,//这个暂不知道怎么获取
				site.PhysicalPath = item.Applications["/"].VirtualDirectories["/"].PhysicalPath;
				//site.AlternateEnabledProtocols = item.ApplicationDefaults.EnabledProtocols;
				//site.TraceFailedRequestsEnabled = item.TraceFailedRequestsLogging.Enabled;
				//site.TraceFailedRequestsDirectory = item.TraceFailedRequestsLogging.Directory;
				//site.TraceFailedRequestsMaxLogFiles = item.TraceFailedRequestsLogging.MaxLogFiles;
				//site.ServerAutoStart = item.ServerAutoStart;

				//邦定信息
				var bindlist = new List<BindingResult>();
				foreach (var itemBind in item.Bindings)
				{

					bindlist.Add(new BindingResult
					{

						IpAddress = (!object.Equals(itemBind.EndPoint.Address, IPAddress.Any) ? itemBind.EndPoint.Address.ToString() : "*"),
						Port = itemBind.EndPoint.Port,
						HostName = itemBind.Host,
						CertificateHash = itemBind.CertificateHash,
						CertificateStoreName = itemBind.CertificateStoreName,
						BindingProtocol = itemBind.Protocol,
						RequireServerNameIndication = itemBind.SslFlags == SslFlags.Sni,
					});
				}
				var appPool = new ApplicationPoolResult();
				var app = item.Applications[0];


				site.Binding = bindlist;
				site.ApplicationPool = new SiteBaseResult { Name = app.ApplicationPoolName, Status = GetApplicationPoolFind(app.ApplicationPoolName).Status };



				list.Add(site);

			}
			return list;
		}
	}
	/// <summary>
	/// 获取所有程序池
	/// </summary>
	/// <returns></returns>
	public static List<ApplicationPoolResult> GetApplicationPoolAllList()
	{
		using (var server = new ServerManager())
		{
			var list = new List<ApplicationPoolResult>();
			var pools = server.ApplicationPools;
			foreach (var item in pools)
			{
				var pool = new ApplicationPoolResult();
				pool.Name = item.Name;
				pool.ManagedRuntimeVersion = item.ManagedRuntimeVersion;
				pool.Autostart = item.AutoStart;
				pool.Status = item.State.ToString();

				pool.Enable32BitAppOnWin64 = item.Enable32BitAppOnWin64;
				pool.ClassicManagedPipelineMode = item.ManagedPipelineMode == ManagedPipelineMode.Classic;
				//进程模型
				pool.LoadUserProfile = item.ProcessModel.LoadUserProfile;
				pool.MaxProcesses = item.ProcessModel.MaxProcesses;
				pool.PingingEnabled = item.ProcessModel.PingingEnabled;
				pool.PingInterval = item.ProcessModel.PingInterval;
				pool.PingResponseTime = item.ProcessModel.PingResponseTime;
				pool.IdleTimeout = item.ProcessModel.IdleTimeout;
				pool.ShutdownTimeLimit = item.ProcessModel.ShutdownTimeLimit;
				pool.StartupTimeLimit = item.ProcessModel.StartupTimeLimit;

				switch (item.ProcessModel.IdentityType)
				{
					case ProcessModelIdentityType.LocalSystem:
						pool.IdentityType = IdentityType.LocalSystem.ToString();
						break;

					case ProcessModelIdentityType.LocalService:
						pool.IdentityType = IdentityType.LocalService.ToString();
						break;

					case ProcessModelIdentityType.NetworkService:
						pool.IdentityType = IdentityType.NetworkService.ToString();
						break;

					case ProcessModelIdentityType.ApplicationPoolIdentity:
						pool.IdentityType = IdentityType.ApplicationPoolIdentity.ToString();
						break;

					case ProcessModelIdentityType.SpecificUser:
						pool.IdentityType = IdentityType.SpecificUser.ToString();
						pool.Username = item.ProcessModel.UserName;
						pool.Password = item.ProcessModel.Password;
						break;


				}


				list.Add(pool);

			}
			return list;
		}
	}
	/// <summary>
	/// 根据池名，获取详细参数
	/// </summary>
	/// <param name="poolName"></param>
	/// <returns></returns>
	public static ApplicationPoolResult GetApplicationPoolFind(string poolName)
	{
		using (var server = new ServerManager())
		{
			var pools = server.ApplicationPools.FirstOrDefault(q => q.Name == poolName);
			var pool = new ApplicationPoolResult();

			pool.Name = pools.Name;
			pool.ManagedRuntimeVersion = pools.ManagedRuntimeVersion;
			pool.Autostart = pools.AutoStart;
			pool.Status = pools.State.ToString();

			pool.Enable32BitAppOnWin64 = pools.Enable32BitAppOnWin64;
			pool.ClassicManagedPipelineMode = pools.ManagedPipelineMode == ManagedPipelineMode.Classic;
			//进程模型
			pool.LoadUserProfile = pools.ProcessModel.LoadUserProfile;
			pool.MaxProcesses = pools.ProcessModel.MaxProcesses;
			pool.PingingEnabled = pools.ProcessModel.PingingEnabled;
			pool.PingInterval = pools.ProcessModel.PingInterval;
			pool.PingResponseTime = pools.ProcessModel.PingResponseTime;
			pool.IdleTimeout = pools.ProcessModel.IdleTimeout;
			pool.ShutdownTimeLimit = pools.ProcessModel.ShutdownTimeLimit;
			pool.StartupTimeLimit = pools.ProcessModel.StartupTimeLimit;

			switch (pools.ProcessModel.IdentityType)
			{
				case ProcessModelIdentityType.LocalSystem:
					pool.IdentityType = IdentityType.LocalSystem.ToString();
					break;

				case ProcessModelIdentityType.LocalService:
					pool.IdentityType = IdentityType.LocalService.ToString();
					break;

				case ProcessModelIdentityType.NetworkService:
					pool.IdentityType = IdentityType.NetworkService.ToString();
					break;

				case ProcessModelIdentityType.ApplicationPoolIdentity:
					pool.IdentityType = IdentityType.ApplicationPoolIdentity.ToString();
					break;

				case ProcessModelIdentityType.SpecificUser:
					pool.IdentityType = IdentityType.SpecificUser.ToString();
					pool.Username = pools.ProcessModel.UserName;
					pool.Password = pools.ProcessModel.Password;
					break;


			}
			return pool;

		}

	}
	#endregion

	#region 绑定管理
	/// <summary>
	/// 添加绑定
	/// </summary>
	/// <param name="id"></param>
	/// <param name="settings"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="Exception"></exception>
	public static bool AddBinding(long id, BindingSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("BindingSettings ");
		}



		using (var server = new ServerManager())
		{
			//Get Site
			Site site = server.Sites.SingleOrDefault(p => p.Id == id);

			if (site != null)
			{
				if (site.Bindings.FirstOrDefault(b => (b.Protocol == settings.BindingProtocol.ToString()) && (b.BindingInformation == settings.BindingInformation)) != null)
				{
					throw new Exception("具有相同ip、端口和主机标头的绑定已存在。");
				}

				//Add Binding
				Binding newBinding = site.Bindings.CreateElement();

				newBinding.Protocol = settings.BindingProtocol.ToString();
				newBinding.BindingInformation = settings.BindingInformation;

				if (settings.CertificateHash != null)
				{
					newBinding.CertificateHash = settings.CertificateHash;
				}

				if (!String.IsNullOrEmpty(settings.CertificateStoreName))
				{
					newBinding.CertificateStoreName = settings.CertificateStoreName;
				}

				if (settings.RequireServerNameIndication)
				{
					if (!string.Equals(settings.BindingProtocol.ToString(), BindingProtocol.Https.ToString(), StringComparison.Ordinal))
						throw new Exception("要求服务器名称指示（SNI）仅适用于HTTPS绑定");

					newBinding.SslFlags |= SslFlags.Sni;
				}

				site.Bindings.Add(newBinding);
				server.CommitChanges();


				return true;
			}
			else
			{
				throw new Exception("站点: " + id + " 不存在.");
			}
		}


	}
	/// <summary>
	/// 删除绑定
	/// </summary>
	/// <param name="id"></param>
	/// <param name="settings"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="Exception"></exception>
	public static bool DeleteBinding(long id, BindingSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}

		using (var server = new ServerManager())
		{
			Site site = server.Sites.SingleOrDefault(p => p.Id == id);
			if (site != null)
			{
				Binding binding = site.Bindings.FirstOrDefault(b => (b.Protocol == settings.BindingProtocol.ToString()) && (b.BindingInformation == settings.BindingInformation));

				if (binding != null)
				{
					//Remove Binding
					site.Bindings.Remove(binding);
					server.CommitChanges();


					return true;
				}
				else
				{
					throw new Exception("不存在具有相同ip、端口和主机标头的绑定。");
					//_Log.Information("不存在具有相同ip、端口和主机标头的绑定。");
					return false;
				}

			}
			else
			{
				throw new Exception("站点: " + id + " 不存在.");
			}
		}
	}


	#endregion

	#region 监测
	public static async Task<object> GetMonitoring(long id)
	{
		var site = GetWebsite(id);
		IWebSiteSnapshot snapshot = null;

		var _monitor = App.GetService<IWebSiteMonitor>();

		if (site != null)
		{
			snapshot = (await _monitor.GetSnapshots(new Microsoft.Web.Administration.Site[] { site })).FirstOrDefault();
		}
		if (snapshot == null)
		{
			throw Oops.Bah("监测snapshot：null");
		}

		dynamic obj = new ExpandoObject();

		//
		// id
		obj.id = site.Id.ToSiteIdEncrypt();
		//正常运行时间
		obj.uptime = snapshot.Uptime;
		//网络
		obj.network = new
		{
			bytes_sent_sec = snapshot.BytesSentSec,//字节发送秒
			bytes_recv_sec = snapshot.BytesRecvSec,//字节接收秒
			connection_attempts_sec = snapshot.ConnectionAttemptsSec,//连接尝试秒
			total_bytes_sent = snapshot.TotalBytesSent,//发送的总字节数
			total_bytes_recv = snapshot.TotalBytesRecv,//接收的总字节数
			total_connection_attempts = snapshot.TotalConnectionAttempts,//连接尝试总数
			current_connections = snapshot.CurrentConnections,//当前连接
		};
		//请求
		obj.requests = new
		{
			active = snapshot.ActiveRequests,//活动请求
			per_sec = snapshot.RequestsSec,//请求秒
			total = snapshot.TotalRequests//请求总数
		};
		//内存
		obj.memory = new
		{
			handles = snapshot.HandleCount,//句柄计数
			private_bytes = snapshot.PrivateBytes,//专有字节
			private_working_set = snapshot.PrivateWorkingSet,//专用工作集
			system_in_use = snapshot.SystemMemoryInUse,//系统内存使用中
			installed = snapshot.TotalInstalledMemory//总安装内存
		};
		//cpu
		obj.cpu = new
		{
			percent_usage = snapshot.PercentCpuTime,//Cpu时间百分比
			threads = snapshot.ThreadCount,//线程计数
			processes = snapshot.ProcessCount//进程计数
		};
		//磁盘
		obj.disk = new
		{
			io_write_operations_sec = snapshot.IOWriteSec,//IO写入秒
			io_read_operations_sec = snapshot.IOReadSec,//IO读取秒
			page_faults_sec = snapshot.PageFaultsSec//页面错误秒
		};
		//缓存
		obj.cache = new
		{
			file_cache_count = snapshot.CurrentFilesCached,//当前缓存的文件
			file_cache_memory_usage = snapshot.FileCacheMemoryUsage,//文件缓存内存使用
			file_cache_hits = snapshot.FileCacheHits,//文件缓存命中
			file_cache_misses = snapshot.FileCacheMisses,//文件缓存未命中
			total_files_cached = snapshot.TotalFilesCached,//缓存的文件总数
			output_cache_count = snapshot.OutputCacheCurrentItems,//输出缓存当前项
			output_cache_memory_usage = snapshot.OutputCacheCurrentMemoryUsage,//输出缓存当前内存使用情况
			output_cache_hits = snapshot.OutputCacheTotalHits,//输出缓存总命中数
			output_cache_misses = snapshot.OutputCacheTotalMisses,//输出缓存总计未命中
			uri_cache_count = snapshot.CurrentUrisCached,//当前缓存的URI
			uri_cache_hits = snapshot.UriCacheHits,//url缓存命中数
			uri_cache_misses = snapshot.UriCacheMisses,//Uri缓存未命中
			total_uris_cached = snapshot.TotalUrisCached//缓存的总URI
		};

		obj.website = new
		{
			name = site.Name,
			id = obj.id,
			status = site.State.ToString().ToLower()
		};
		return obj;
	}
	#endregion
}