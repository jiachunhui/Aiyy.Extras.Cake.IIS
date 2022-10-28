namespace Aiyy.Extras.Cake.IIS.Monitoring;

public class MissingCountersException : Exception
{
	public MissingCountersException(IEnumerable<IPerfCounter> counters, string message = null) : base(message)
	{
		Counters = counters;
	}

	public IEnumerable<IPerfCounter> Counters { get; private set; }
}
