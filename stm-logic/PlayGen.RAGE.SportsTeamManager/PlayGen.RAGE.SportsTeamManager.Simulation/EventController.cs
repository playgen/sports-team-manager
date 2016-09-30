using System;
using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Handles NPC dialogue events
	/// </summary>
	public class EventController
	{
		/// <summary>
		/// Get all player dialogues strings with the eventKey provided set as CurrentState
		/// </summary>
		public string[] GetEventStrings(IntegratedAuthoringToolAsset iat, string eventKey)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey.ToName());
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		/// <summary>
		/// Get all player dialogues with the eventKey provided set as CurrentState
		/// </summary>
		public DialogueStateActionDTO[] GetEvents(IntegratedAuthoringToolAsset iat, string eventKey)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey.ToName());
			return dialogueOptions.ToArray();
		}

		/// <summary>
		/// Select a random (if any) event to trigger post race
		/// </summary>
		public DialogueStateActionDTO SelectPostRaceEvent(IntegratedAuthoringToolAsset iat, int chance, int sessionEventCount, Random random, bool raceSession = false)
		{
			List<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "PostRaceEventStart".ToName()).ToList();
			if (raceSession)
			{
				dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains("Race")).ToList();
			} else
			{
				dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains("Practice")).ToList();
			}
			if (dialogueOptions.Any())
			{
				chance = (int)Math.Pow(chance, sessionEventCount + 1);
				int dialogueIndex = random.Next(0, dialogueOptions.Count * chance);
				if (dialogueIndex % chance == 0)
				{
					DialogueStateActionDTO selectedDialogue = dialogueOptions.ToArray()[dialogueIndex / chance];
					return selectedDialogue;
				}
			}
			return null;
		}

		/// <summary>
		/// Send dialogue from the player to a NPC in a meeting and get the reply from the NPC
		/// </summary>
		public string SendMeetingEvent(IntegratedAuthoringToolAsset iat, string eventName, CrewMember crewMember, Boat boat)
		{
			return crewMember.SendMeetingEvent(iat, eventName, boat);
		}

		/// <summary>
		/// Send dialogue from the player to all recruit NPCs and get their replies 
		/// </summary>
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

		/// <summary>
		/// Send dialogue from the player to (an) NPC(s) and get their replies 
		/// </summary>
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
