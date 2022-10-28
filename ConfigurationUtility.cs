using Microsoft.Web.Administration;

namespace Aiyy.Extras.Cake.IIS;

public static class ConfigurationUtility
{
	public static bool HasAttribute(this ConfigurationElementSchema schema, string name)
	{
		return schema.AttributeSchemas.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}
}
