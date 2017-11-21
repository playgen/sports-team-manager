using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Allows private properties to be gathered when converting from JSON
	/// </summary>
	internal class PrivatePropertyResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(
		MemberInfo member,
		MemberSerialization memberSerialization)
		{
			var prop = base.CreateProperty(member, memberSerialization);

			if (!prop.Writable)
			{
				var property = member as PropertyInfo;
				if (property != null)
				{
					var hasPrivateSetter = property.GetSetMethod(true) != null;
					prop.Writable = hasPrivateSetter;
				}
			}

			return prop;
		}
	}
}
