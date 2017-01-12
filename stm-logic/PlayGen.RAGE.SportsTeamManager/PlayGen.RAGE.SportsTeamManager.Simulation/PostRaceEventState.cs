using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class PostRaceEventState
	{
		public CrewMember CrewMember;
		public DialogueStateActionDTO Dialogue;
		public List<string> Subjects;

		public PostRaceEventState(CrewMember crewMember, DialogueStateActionDTO dialogue, List<string> subs = null)
		{
			if (subs == null)
			{
				subs = new List<string>();
			}
			CrewMember = crewMember;
			Dialogue = dialogue;
			Subjects = subs;
		}
	}
}