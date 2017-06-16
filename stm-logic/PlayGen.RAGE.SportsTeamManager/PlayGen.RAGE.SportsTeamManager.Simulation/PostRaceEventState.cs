using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// The CrewMember, Dialogue option and subjects (if any) involved in a post-race event
	/// </summary>
	public class PostRaceEventState
	{
		public CrewMember CrewMember { get; set; }
		public DialogueStateActionDTO Dialogue { get; set; }
		public List<string> Subjects { get; set; }

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