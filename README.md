# Aiyy.Extras.Cake.IIS

## 功能：
> IIS里的那些设置都可以实现。
* 添加网站
* 删除网站
* 添加绑定
* 删除绑定
* 添加程序池
* 删除程序池
* 网站列表
* 网站监测（CPU,网络、内存等）
* 网站文件管理（列表、删除、上传、查看）

## 使用

~~~ .net core
public void ConfigureServices(IServiceCollection services)
{
//监测必须
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//iis
			services.AddCakeIISServer();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
{
//监测用到，代码
			HttpHelper.HttpContextAccessor = contextAccessor;
}
~~~

demo

### 添加站点
> 可以先get默认设置项，也可以自己设置项
~~~

/// <summary>
	/// 添加站点
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public async Task<bool> AddSite(SiteInput input)
	{


		//查找是否已经存在的站点
		var isSite = CakeIISHelper.GetWebsiteExists(input.Name);
		if (isSite)
		{
			throw Oops.Bah($"站点：{input.Name}.已经存在");
		}
		//添加站点
		//1、先获取默认配置
		var appPoolSetting = CakeIISHelper.GetAppPoolSettings(input.Name);
		appPoolSetting.IdentityType = IdentityType.ApplicationPoolIdentity;//标识
		appPoolSetting.LoadUserProfile = true;//加载用户配置文件

		var siteSetting = CakeIISHelper.GetWebsiteSettings(input.Name);

		//物理目录：配置+站点名
		var physicalDirectory = System.IO.Path.Combine(cakeIISOptions.PhysicalDirectory, input.Name);
		//站点免费域名：站点名+二级域名
		var domin = cakeIISOptions.HostName.Substring(cakeIISOptions.HostName.IndexOf('.'));
		var hostName = $"{input.Name}{domin}";

		siteSetting.PhysicalDirectory = physicalDirectory;
		siteSetting.ApplicationPool = appPoolSetting;
		siteSetting.Binding = IISBindings.Http
			.SetHostName(hostName)
			.SetIpAddress("*")
			.SetPort(80);
		//2、创建站点

		CakeIISHelper.CreateWebsite(siteSetting);
		

		return await Task.FromResult(true);
	}
~~~

~~~
/// <summary>
	/// 删除站点
	/// </summary>
	/// <param name="id">加密后ID</param>
	/// <returns></returns>
	public async Task<bool> DeleteSite(string id)
	{
		var siteId = GetSiteId(id);
		var site = CakeIISHelper.GetWebsite(siteId.ToLong());
		if (site == null)
		{
			throw Oops.Bah($"站点ID：{id}.不存在");
		}
		//删除
		CakeIISHelper.DeleteWebsite(site.Name);
		//程序池和站点名是一样的
		CakeIISHelper.DeletePool(site.Name);

		return await Task.FromResult(true);
	}
~~~


## 日志：

### 2022-10-28
现阶段我自己项目中使用的功能有限，只对网站管理这块进行了扩展。像FTP,Server Farms,程序池，规则，都已经实现。
