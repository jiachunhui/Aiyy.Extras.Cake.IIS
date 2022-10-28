namespace Aiyy.Extras.Cake.IIS.Monitoring;

public interface IPerfCounter
{
	string Name { get; }
	string InstanceName { get; }
	string CategoryName { get; }
	string Path { get; }
	long Value { get; set; }
}
