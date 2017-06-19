using System.Globalization;

public static class CultureExtensions
{
	/// <summary>
	/// Extension used to ensure the current culture based on the currently selected localization language is used when formatting strings
	/// </summary>
	public static CultureInfo GetSpecificCulture(this CultureInfo culture)
	{
		return culture.IsNeutralCulture ? CultureInfo.CreateSpecificCulture(culture.Name) : culture;
	}
}