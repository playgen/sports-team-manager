using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class EventController
	{
		public IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }

		public DialogueStateActionDTO[] PreRaceEvents { get; set; }
		public DialogueStateActionDTO[] MidRaceEvents { get; set; }
		public DialogueStateActionDTO[] PostRaceEvents { get; set; }
		public DialogueStateActionDTO[] SoloInterviewEvents { get; set; }
		public DialogueStateActionDTO[] PairedInterviewEvents { get; set; }
		public DialogueStateActionDTO[] TeamInterviewEvents { get; set; }

		public EventController(IntegratedAuthoringToolAsset iat)
		{
			IntegratedAuthoringTool = iat;

			PreRaceEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "PreRace").ToArray();
			MidRaceEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "MidRace").ToArray();
			PostRaceEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "PostRace").ToArray();
			SoloInterviewEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "SoloInterview").ToArray();
			PairedInterviewEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "PairedInterview").ToArray();
			TeamInterviewEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "TeamInterview").ToArray();
		}

		public List<string> SelectEvent(DialogueStateActionDTO selected, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendEvent(IntegratedAuthoringTool, selected, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}
	}
}
