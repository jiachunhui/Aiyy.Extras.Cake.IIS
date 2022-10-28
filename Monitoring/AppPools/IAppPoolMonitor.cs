using Microsoft.Web.Administration;

namespace Aiyy.Extras.Cake.IIS.Monitoring;

public interface IAppPoolMonitor
{
	Task<IEnumerable<IAppPoolSnapshot>> GetSnapshots(IEnumerable<ApplicationPool> pools);
}
