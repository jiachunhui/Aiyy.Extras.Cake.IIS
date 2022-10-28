namespace Aiyy.Extras.Cake.IIS.Monitoring;
public interface ICounterTranslator
{
	string TranslateCategory(string counterName);
}