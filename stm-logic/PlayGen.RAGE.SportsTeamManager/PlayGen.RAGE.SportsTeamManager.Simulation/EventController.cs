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

		public List<string> SelectBoatMemberEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendBoatMemberEvent(iat, selected.CurrentState, selected.Style, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}

		public List<string> SelectBoatMemberEvent(IntegratedAuthoringToolAsset iat, string eventType, string eventName, List<CrewMember> crewMembers, Boat boat)
		{
			List<string> replies = new List<string>();
			foreach (CrewMember member in crewMembers)
			{
				var reply = member.SendBoatMemberEvent(iat, eventType, eventName, boat);
				if (reply != null)
				{
					replies.Add(reply);
				}
			}
			return replies;
		}

		public Dictionary<CrewMember, string> SelectRecruitEvent(IntegratedAuthoringToolAsset iat, CrewMemberSkill skill, List<CrewMember> crewMembers)
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
	}
}
