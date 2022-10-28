namespace Aiyy.Extras.Cake.IIS.Monitoring;
class SingletonPerfCounter : IPerfCounter
{
	public SingletonPerfCounter(string name, string categoryName)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (string.IsNullOrEmpty(categoryName))
		{
			throw new ArgumentNullException(nameof(categoryName));
		}

		Name = name;
		InstanceName = string.Empty;
		CategoryName = categoryName;
		Path = $@"\{CategoryName}\{Name}";
	}

	public string Name { get; private set; }
	public string InstanceName { get; private set; }
	public string CategoryName { get; private set; }
	public virtual string Path { get; protected set; }
	public long Value { get; set; }
}