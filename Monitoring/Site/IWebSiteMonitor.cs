using Microsoft.Web.Administration;

namespace Aiyy.Extras.Cake.IIS.Monitoring;

public interface IWebSiteMonitor
{
	Task<IEnumerable<IWebSiteSnapshot>> GetSnapshots(IEnumerable<Site> sites);
}
