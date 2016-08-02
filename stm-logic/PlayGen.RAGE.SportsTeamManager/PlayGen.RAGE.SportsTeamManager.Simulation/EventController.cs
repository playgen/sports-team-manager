using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class EventController
	{
		public IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }

		public DialogueStateActionDTO[] PreRaceEvents { get; }
		public DialogueStateActionDTO[] MidRaceEvents { get; }
		public DialogueStateActionDTO[] PostRaceEvents { get; }
		public DialogueStateActionDTO[] SoloInterviewEvents { get; }
		public DialogueStateActionDTO[] PairedInterviewEvents { get; }
		public DialogueStateActionDTO[] TeamInterviewEvents { get; }

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

		public List<string> SelectEvent(DialogueStateActionDTO selected, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in boat.GetAllCrewMembers())
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
