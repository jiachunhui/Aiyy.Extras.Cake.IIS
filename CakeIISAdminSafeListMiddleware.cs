using System.Net;
using Furion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aiyy.Extras.Cake.IIS;

public class CakeIISAdminSafeListMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<CakeIISAdminSafeListMiddleware> _logger;

	public CakeIISAdminSafeListMiddleware(
   RequestDelegate next,
   ILogger<CakeIISAdminSafeListMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		var remoteIp = context.Connection.RemoteIpAddress;

		var options = App.GetOptionsMonitor<CakeIISOptions>();
		string[] ip = string.IsNullOrWhiteSpace(options.AdminSafeList) ? Array.Empty<string>() : options.AdminSafeList.Split(';');
		if (!string.IsNullOrWhiteSpace(options.AdminSafeList))
		{
			var bytes = remoteIp.GetAddressBytes();
			var badIp = true;
			foreach (var address in ip)
			{
				var testIp = IPAddress.Parse(address);
				if (testIp.GetAddressBytes().SequenceEqual(bytes))
				{
					badIp = false;
					break;
				}
			}

			if (badIp)
			{
				_logger.LogInformation(
					$"禁止来自远程IP地址的请求: {remoteIp}");
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return;
			}
		}

		await _next.Invoke(context);
	}
}
