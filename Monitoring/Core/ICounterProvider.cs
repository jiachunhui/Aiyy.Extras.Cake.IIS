namespace Aiyy.Extras.Cake.IIS.Monitoring;

public interface ICounterProvider
{
	Task<IEnumerable<string>> GetInstances(string category);

	Task<IEnumerable<IPerfCounter>> GetCounters(string category, string instance, IEnumerable<string> counterNames);

	Task<IEnumerable<IPerfCounter>> GetSingletonCounters(string category, IEnumerable<string> counterNames);
}
