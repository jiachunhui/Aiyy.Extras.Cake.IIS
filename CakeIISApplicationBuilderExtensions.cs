using Microsoft.AspNetCore.Builder;

namespace Aiyy.Extras.Cake.IIS;

public static class CakeIISApplicationBuilderExtensions
{
	public static IApplicationBuilder UseCakeIIS(this IApplicationBuilder app)
	{

		app.UseMiddleware<CakeIISAdminSafeListMiddleware>();//IP 白名单
		app.UseMiddleware<CakeIISAdminBlackListMiddleware>();//IP 黑名单
		return app;
	}

}
