# Aiyy.Extras.Cake.IIS

## ���ܣ�
> IIS�����Щ���ö�����ʵ�֡�
* �����վ
* ɾ����վ
* ��Ӱ�
* ɾ����
* ��ӳ����
* ɾ�������
* ��վ�б�
* ��վ��⣨CPU,���硢�ڴ�ȣ�
* ��վ�ļ������б�ɾ�����ϴ����鿴��

## ʹ��

~~~ .net core
public void ConfigureServices(IServiceCollection services)
{
//������
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//iis
			services.AddCakeIISServer();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
{
//����õ�������
			HttpHelper.HttpContextAccessor = contextAccessor;
}
~~~

demo

### ���վ��
> ������getĬ�������Ҳ�����Լ�������
~~~

/// <summary>
	/// ���վ��
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public async Task<bool> AddSite(SiteInput input)
	{


		//�����Ƿ��Ѿ����ڵ�վ��
		var isSite = CakeIISHelper.GetWebsiteExists(input.Name);
		if (isSite)
		{
			throw Oops.Bah($"վ�㣺{input.Name}.�Ѿ�����");
		}
		//���վ��
		//1���Ȼ�ȡĬ������
		var appPoolSetting = CakeIISHelper.GetAppPoolSettings(input.Name);
		appPoolSetting.IdentityType = IdentityType.ApplicationPoolIdentity;//��ʶ
		appPoolSetting.LoadUserProfile = true;//�����û������ļ�

		var siteSetting = CakeIISHelper.GetWebsiteSettings(input.Name);

		//����Ŀ¼������+վ����
		var physicalDirectory = System.IO.Path.Combine(cakeIISOptions.PhysicalDirectory, input.Name);
		//վ�����������վ����+��������
		var domin = cakeIISOptions.HostName.Substring(cakeIISOptions.HostName.IndexOf('.'));
		var hostName = $"{input.Name}{domin}";

		siteSetting.PhysicalDirectory = physicalDirectory;
		siteSetting.ApplicationPool = appPoolSetting;
		siteSetting.Binding = IISBindings.Http
			.SetHostName(hostName)
			.SetIpAddress("*")
			.SetPort(80);
		//2������վ��

		CakeIISHelper.CreateWebsite(siteSetting);
		

		return await Task.FromResult(true);
	}
~~~

~~~
/// <summary>
	/// ɾ��վ��
	/// </summary>
	/// <param name="id">���ܺ�ID</param>
	/// <returns></returns>
	public async Task<bool> DeleteSite(string id)
	{
		var siteId = GetSiteId(id);
		var site = CakeIISHelper.GetWebsite(siteId.ToLong());
		if (site == null)
		{
			throw Oops.Bah($"վ��ID��{id}.������");
		}
		//ɾ��
		CakeIISHelper.DeleteWebsite(site.Name);
		//����غ�վ������һ����
		CakeIISHelper.DeletePool(site.Name);

		return await Task.FromResult(true);
	}
~~~


## ��־��

### 2022-10-28
�ֽ׶����Լ���Ŀ��ʹ�õĹ������ޣ�ֻ����վ��������������չ����FTP,Server Farms,����أ����򣬶��Ѿ�ʵ�֡�
