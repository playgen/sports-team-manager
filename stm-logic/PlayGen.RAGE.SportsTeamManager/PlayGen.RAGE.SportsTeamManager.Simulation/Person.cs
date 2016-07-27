using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }

		public RolePlayCharacterAsset RolePlayCharacter { get; set; }
	}
}
