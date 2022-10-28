using System.Net;
using Furion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aiyy.Extras.Cake.IIS;

public class CakeIISAdminBlackListMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<CakeIISAdminBlackListMiddleware> _logger;

	public CakeIISAdminBlackListMiddleware(
   RequestDelegate next,
   ILogger<CakeIISAdminBlackListMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		var remoteIp = context.Connection.RemoteIpAddress;

		var options = App.GetOptionsMonitor<CakeIISOptions>();

		string[] ip = string.IsNullOrWhiteSpace(options.AdminBlackList) ? Array.Empty<string>() : options.AdminBlackList.Split(';');
		if (!string.IsNullOrWhiteSpace(options.AdminBlackList))
		{
			var bytes = remoteIp.GetAddressBytes();
			var badIp = false;
			foreach (var address in ip)
			{
				var testIp = IPAddress.Parse(address);

				if (testIp.GetAddressBytes().SequenceEqual(bytes))
				{
					badIp = true;

					break;
				}
			}

			if (badIp)
			{
				_logger.LogInformation(
					$"禁止来自远程IP地址的请求【黑名单】: {remoteIp}");
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return;
			}
		}

		await _next.Invoke(context);
	}
}
