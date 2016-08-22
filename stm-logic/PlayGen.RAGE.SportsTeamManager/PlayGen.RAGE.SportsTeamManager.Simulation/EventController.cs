using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class EventController
	{
		public IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }

		public DialogueStateActionDTO[] MidRaceEvents { get; set; }
		public DialogueStateActionDTO[] PostRaceEvents { get; set; }
		public DialogueStateActionDTO[] RecruitInterviewEvents { get; set; }

		public EventController(IntegratedAuthoringToolAsset iat)
		{
			IntegratedAuthoringTool = iat;

			MidRaceEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "MidRace").ToArray();
			PostRaceEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "PostRace").ToArray();
			RecruitInterviewEvents = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, "RecruitInterview").ToArray();
		}

		public string[] GetEventStrings(string eventKey)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = IntegratedAuthoringTool.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey);
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		public List<string> SelectEvent(DialogueStateActionDTO selected, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendEvent(IntegratedAuthoringTool, selected.CurrentState, selected.Style, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}

		public List<string> SelectEvent(string eventType, string eventName, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendEvent(IntegratedAuthoringTool, eventType, eventName, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}
	}
}
