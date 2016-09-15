using System;
using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class EventController
	{
		public string[] GetEventStrings(IntegratedAuthoringToolAsset iat, string eventKey)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey);
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		public DialogueStateActionDTO[] GetEvents(IntegratedAuthoringToolAsset iat, string eventKey)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey);
			return dialogueOptions.ToArray();
		}

		public DialogueStateActionDTO SelectPostRaceEvent(IntegratedAuthoringToolAsset iat, int chance)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "PostRaceEventStart");
			if (dialogueOptions.Count() > 0)
			{
				Random random = new Random();
				int dialogueIndex = random.Next(0, dialogueOptions.Count() * chance);
				if (dialogueIndex % chance == 0)
				{
					DialogueStateActionDTO selectedDialogue = dialogueOptions.ToArray()[dialogueIndex / chance];
					return selectedDialogue;
				}
			}
			return null;
		}

		public List<string> SendMeetingEvent(IntegratedAuthoringToolAsset iat, string eventType, string eventName, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendMeetingEvent(iat, eventType, eventName, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}

		public Dictionary<CrewMember, string> SendRecruitEvent(IntegratedAuthoringToolAsset iat, CrewMemberSkill skill, List<CrewMember> crewMembers)
		{
			Dictionary<CrewMember, string> replies = new Dictionary<CrewMember, string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendRecruitEvent(iat, skill);
				if (reply != null)
				{
					replies.Add(member, reply);
				}
			}
			return replies;
		}

		public Dictionary<CrewMember, string> SendPostRaceEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, List<CrewMember> crewMembers, Boat boat, Boat previous)
		{
			Dictionary<CrewMember, string> replies = new Dictionary<CrewMember, string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendPostRaceEvent(iat, selected, boat, previous);
				if (reply != null)
				{
					replies.Add(member, reply);
				}
			}
			return replies;
		}
	}
}
