namespace Aiyy.Extras.Cake.IIS.Monitoring;
class PerfCounter : IPerfCounter
{
	public PerfCounter(string name, string instanceName, string categoryName)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (string.IsNullOrEmpty(instanceName))
		{
			throw new ArgumentNullException(nameof(instanceName));
		}

		if (string.IsNullOrEmpty(categoryName))
		{
			throw new ArgumentNullException(nameof(categoryName));
		}

		Name = name;
		InstanceName = instanceName;
		CategoryName = categoryName;
		Path = $@"\{CategoryName}({InstanceName})\{Name}";
	}

	public string Name { get; private set; }
	public string InstanceName { get; private set; }
	public string CategoryName { get; private set; }
	public string Path { get; private set; }
	public long Value { get; set; }
}