using System.Diagnostics;
using Cake.Core.Diagnostics;

namespace Aiyy.Cake.IIS;

public class CakeIISLog : ICakeLog
{
	public Verbosity Verbosity
	{
		get { return Verbosity.Diagnostic; }
		set { }
	}

	public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
	{
		try
		{
			format = string.Format(format, args);

			if (Debugger.IsAttached)
			{
				Debug.WriteLine(format);
			}
			else
			{
				Console.WriteLine(format);
			}
		}
		catch { }
	}
}