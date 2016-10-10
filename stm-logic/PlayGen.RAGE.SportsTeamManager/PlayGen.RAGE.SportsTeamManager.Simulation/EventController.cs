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

		private IntegratedAuthoringToolAsset _iat;

		public EventController(IntegratedAuthoringToolAsset iat)
		{
			_iat = iat;
		}

		/// <summary>
		/// Set the player dialogue state
		/// </summary>
		public void SetPlayerState(DialogueStateActionDTO currentEvent)
		{
			_iat.SetDialogueState("Player", currentEvent.NextState);
		}

		/// <summary>
		/// Get all player dialogues with the eventKey provided set as CurrentState
		/// </summary>
		public DialogueStateActionDTO[] GetEvents()
		{
			var state = _iat.GetCurrentDialogueState("Player");
			var dialogueOptions = _iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, state.ToName());
			return dialogueOptions.OrderBy(c => Guid.NewGuid()).ToArray();
		}

		/// <summary>
		/// Get all player dialogues strings with the eventKey provided set as CurrentState
		/// </summary>
		public string[] GetEventStrings(string eventKey)
		{
			var dialogueOptions = _iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey.ToName());
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		/// <summary>
		/// Send player meeting dialogue to a CrewMember, getting their response in return
		/// </summary>
		public string SendMeetingEvent(string eventName, CrewMember member, Team team)
		{
			return member.SendMeetingEvent(_iat, eventName, team);
		}

		public Dictionary<CrewMember, string> SendRecruitEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var replies = new Dictionary<CrewMember, string>();
			foreach (var member in members)
			{
				var reply = member.SendRecruitEvent(_iat, skill);
				replies.Add(member, reply ?? "");
			}
			return replies;
		}

		/// <summary>
		/// Select a random (if any) event to trigger post race
		/// </summary>
		public List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> SelectPostRaceEvents(Team team, int chance, Random random, bool raceSession)
		{
			var postRaceEvents = GetLastingEvents(team, raceSession);
			var selectedEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
			var dialogueOptions = GetPossiblePostRaceDialogue(raceSession);
			if (dialogueOptions.Any())
			{
				var findEvents = true;
				var allCrew = team.CrewMembers;
				var allCrewRemovals = new List<CrewMember>();
				foreach (var crewMember in allCrew.Values)
				{
					if ((crewMember.LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "true")
					{
						allCrewRemovals.Add(crewMember);
					}
					else if (crewMember.LoadBelief("Event(Retire)") != null)
					{
						allCrewRemovals.Add(crewMember);
					}
				}
				foreach (var crewMember in allCrewRemovals)
				{
					allCrew.Remove(crewMember.Name);
				}
				while (findEvents)
				{
					if (allCrew.Count == 0)
					{
						findEvents = false;
						continue;
					}
					var selected = GetRandomEvent((int)Math.Pow(chance, selectedEvents.Count + 1), dialogueOptions);
					if (selected == null)
					{
						findEvents = false;
						continue;
					}
					var eventSelected = new List<CrewMember>();
					switch (selected.NextState)
					{
						case "NotPicked":
							//for this event, select a crew member who was not selected in the previous race
							foreach (var bp in team.LineUpHistory.Last().BoatPositionCrew)
							{
								allCrew.Remove(bp.Value.Name);
							}
							if (allCrew.Count == 0)
							{
								findEvents = false;
								continue;
							}
							eventSelected.Add(allCrew.OrderBy(c => Guid.NewGuid()).First().Value);
							break;
						case "Retirement":
							allCrew = allCrew.Where(cm => cm.Value.RestCount <= -5).ToDictionary(ac => ac.Key, ac => ac.Value);
							if (allCrew.Count == 0)
							{
								findEvents = false;
								continue;
							}
							eventSelected.Add(allCrew.OrderBy(c => Guid.NewGuid()).First().Value);
							eventSelected.ForEach(es => es.UpdateSingleBelief("Event(Retire)", "1"));
							break;
						default:
							findEvents = false;
							break;
					}
					eventSelected.ForEach(es => allCrew.Remove(es.Name));
					selectedEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(eventSelected, selected));
				}
			}
			return postRaceEvents.Concat(selectedEvents).ToList();
		}

		public List<DialogueStateActionDTO> GetPossiblePostRaceDialogue(bool raceSession)
		{
			var dialogueOptions = _iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "PostRaceEventStart".ToName()).OrderBy(c => Guid.NewGuid()).ToList();
			if (raceSession)
			{
				dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains("Race")).ToList();
			}
			else
			{
				dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains("Practice")).ToList();
			}
			return dialogueOptions;
		}

		public List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> GetLastingEvents(Team team, bool raceSession)
		{
			var reactionEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
			foreach (var crewMember in team.CrewMembers.Values)
			{
				var delayedReactions = crewMember.CurrentEventCheck(team, _iat, raceSession);
				foreach (var reply in delayedReactions)
				{
					reactionEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(new List<CrewMember> { crewMember }, reply));
				}
			}
			return reactionEvents;
		}

		public DialogueStateActionDTO GetRandomEvent(int chance, List<DialogueStateActionDTO> availableDialogue)
		{
			var rand = new Random();
			var dialogueIndex = rand.Next(0, availableDialogue.Count * chance);
			if (dialogueIndex % chance == 0)
			{
				var selectedDialogue = availableDialogue.ToArray()[dialogueIndex / chance];
				return selectedDialogue;
			}
			return null;
		}

		/// <summary>
		/// Send dialogue from the player to (an) NPC(s) and get their replies 
		/// </summary>
		public Dictionary<CrewMember, string> SendPostRaceEvent(DialogueStateActionDTO selected, List<CrewMember> crewMembers, Team team, Boat previous)
		{
			var replies = new Dictionary<CrewMember, string>();
			foreach (var member in crewMembers)
			{
				var reply = member.SendPostRaceEvent(_iat, selected, team, previous);
				if (reply != null)
				{
					replies.Add(member, reply);
				}
			}
			return replies;
		}
	}
}
