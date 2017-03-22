using System.Globalization;

public static class CultureExtensions
{
	public static CultureInfo GetSpecificCulture(this CultureInfo culture)
	{
		return culture.IsNeutralCulture ? CultureInfo.CreateSpecificCulture(culture.Name) : culture;
	}
}