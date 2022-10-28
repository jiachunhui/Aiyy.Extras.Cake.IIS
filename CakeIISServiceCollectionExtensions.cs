using Aiyy.Extras.Cake.IIS.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Aiyy.Extras.Cake.IIS;

public static class CakeIISServiceCollectionExtensions
{
	private static CounterProvider _provider = null;
	/// <summary>
	/// Cake
	/// </summary>
	/// <param name="services"></param>
	/// <returns></returns>
	public static IServiceCollection AddCakeIISServer(this IServiceCollection services)
	{


		var finder = new CounterFinder((ICounterTranslator)new CounterTranslator());
		_provider = new CounterProvider(finder);
		var siteMonitor = new WebSiteMonitor(_provider);
		services.AddSingleton<IWebSiteMonitor>(siteMonitor);
		//选项
		services.AddConfigurableOptions<CakeIISOptions>();
		//虚拟文件
		services.AddVirtualFileServer();


		return services;
	}
}
