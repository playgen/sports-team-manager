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
		private readonly IntegratedAuthoringToolAsset iat;
		private readonly List<DialogueStateActionDTO> learningPills;
		public List<KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>> PostRaceEvents { get; private set; }

		public EventController(IntegratedAuthoringToolAsset i, List<DialogueStateActionDTO> help)
		{
			iat = i;
			learningPills = help;
			PostRaceEvents = new List<KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>>();
		}

		/// <summary>
		/// Set the player dialogue state
		/// </summary>
		public void SetPlayerState(DialogueStateActionDTO currentEvent)
		{
			iat.SetDialogueState("Player", currentEvent.NextState);
		}

		/// <summary>
		/// Get all player dialogues for their current state
		/// </summary>
		public DialogueStateActionDTO[] GetEventDialogues(Person manager)
		{
			var state = iat.GetCurrentDialogueState("Player");
			var dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, state.ToName()).ToList();
			if (!dialogueOptions.Any())
			{
				for (int i = 0; i < PostRaceEvents[PostRaceEvents.Count - 1].Key.Count; i++)
				{
					manager.EmotionalAppraisal.RemoveBelief(string.Format("PRECrew{0}({1})", PostRaceEvents.Count - 1, i), "SELF");
					manager.EmotionalAppraisal.RemoveBelief(string.Format("PREEvent{0}({1})", PostRaceEvents.Count - 1, i), "SELF");
				}
				PostRaceEvents.RemoveAt(0);
				SaveEvents(manager);
				if (PostRaceEvents.Count > 0) {
					if (PostRaceEvents.First().Value.Count == 1)
					{
						SetPlayerState(PostRaceEvents.First().Value.First());
					} else
					{
						//TODO: Way of calculating player state from multiple previous dialogues
					}
				}
			}
			return dialogueOptions.OrderBy(c => Guid.NewGuid()).ToArray();
		}

		/// <summary>
		/// Get all player dialogues strings with the eventKey provided set as CurrentState
		/// </summary>
		public string[] GetEventStrings(string eventKey)
		{
			var dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, eventKey.ToName());
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		/// <summary>
		/// Get help dialogue from those available
		/// </summary>
		public string GetHelpText(string key)
		{
			var dialogueOptions = learningPills.Where(hd => hd.NextState == key).OrderBy(o => Guid.NewGuid()).ToList();
			return dialogueOptions.Count != 0 ? dialogueOptions.First().Utterance : null;
		}

		/// <summary>
		/// Send player meeting dialogue to a CrewMember, getting their response in return
		/// </summary>
		public string SendMeetingEvent(string eventName, CrewMember member, Team team)
		{
			return member.SendMeetingEvent(iat, eventName, team);
		}

		/// <summary>
		/// Send recruitment dialogue, getting their responses in return
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var replies = new Dictionary<CrewMember, string>();
			foreach (var member in members)
			{
				var reply = member.SendRecruitEvent(iat, skill);
				replies.Add(member, reply ?? "");
			}
			return replies;
		}

		/// <summary>
		/// Select a random (if any) event to trigger post race
		/// </summary>
		public void SelectPostRaceEvents(ConfigStore config, Team team, int chance, bool raceSession)
		{
			//get the state of currrently running events
			var currentEvents = GetLastingEvents(team, raceSession);
			var selectedEvents = new List<KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>>();
			//get all possible post-race event starting dialogue
			var dialogueOptions = GetPossiblePostRaceDialogue(raceSession);
			var events = GetEvents(dialogueOptions, config.GameConfig.EventTriggers.ToList(), team, (int)config.ConfigValues[ConfigKeys.RaceSessionLength]);
			if (events.Any())
			{
				var allCrew = team.CrewMembers;
				var allCrewRemovals = new List<CrewMember>();
				//remove those already involved in a running event to not be selected
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
				foreach (var ev in events)
				{
					//if there is no-one to select for an event, stop looking
					if (allCrew.Count == 0)
					{
						continue;
					}
					var selected = dialogueOptions.Where(a => a.NextState == ev.EventName).OrderBy(o => Guid.NewGuid()).First();
					if (ev.Random)
					{
						if (StaticRandom.Int(0, (int)Math.Pow(chance, selectedEvents.Count + 1)) != 0)
						{
							continue;
						}
					}
					var crewSelected = new List<CrewMember>();
					var eventSelected = new List<DialogueStateActionDTO>();
					switch (selected.NextState)
					{
						case "NotPicked":
							//for this event, select a crew member who was not selected in the previous race
							foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								allCrew.Remove(pair.Value.Name);
							}
							if (allCrew.Count == 0)
							{
								continue;
							}
							crewSelected.Add(allCrew.OrderBy(c => Guid.NewGuid()).First().Value);
							eventSelected.Add(selected);
							break;
						case "Retirement":
							//for this event, select a crew member who has not been selected in the past five race sessions
							allCrew = allCrew.Where(cm => cm.Value.RestCount <= -4).ToDictionary(ac => ac.Key, ac => ac.Value);
							foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								allCrew.Remove(pair.Value.Name);
							}
							if (allCrew.Count == 0)
							{
								continue;
							}
							crewSelected.Add(allCrew.OrderBy(c => Guid.NewGuid()).First().Value);
							eventSelected.Add(selected);
							crewSelected.ForEach(es => es.UpdateSingleBelief("Event(Retire)", "1"));
							break;
					}
					crewSelected.ForEach(es => allCrew.Remove(es.Name));
					selectedEvents.Add(new KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>(crewSelected, eventSelected));
				}
			}
			PostRaceEvents = currentEvents.Concat(selectedEvents).ToList();
			SaveEvents(team.Manager);
			if (PostRaceEvents.Count > 0)
			{
				if (PostRaceEvents.First().Value.Count == 1)
				{
					SetPlayerState(PostRaceEvents.First().Value.First());
				}
				else
				{
					//TODO: Way of calculating player state from multiple previous dialogues
				}
			}
		}

		/// <summary>
		/// get all post-race starting dialogue based on the last type of session
		/// </summary>
		public List<DialogueStateActionDTO> GetPossiblePostRaceDialogue(bool raceSession)
		{
			var dialogueOptions = GetPossibleAgentDialogue("PostRaceEventStart");
			dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains(raceSession ? "Race" : "Practice")).ToList();
			return dialogueOptions;
		}

		/// <summary>
		/// Get all post-race starting dialogue based on the string provided
		/// </summary>
		public List<DialogueStateActionDTO> GetPossibleAgentDialogue(string eventName)
		{
			return iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventName.ToName()).OrderBy(c => Guid.NewGuid()).ToList();
		}

		/// <summary>
		/// get the currently running events for all CrewMembers
		/// </summary>
		public List<KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>> GetLastingEvents(Team team, bool raceSession)
		{
			var reactionEvents = new List<KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>>();
			foreach (var crewMember in team.CrewMembers.Values)
			{
				var delayedReactions = crewMember.CurrentEventCheck(team, iat, raceSession);
				foreach (var reply in delayedReactions)
				{
					reactionEvents.Add(new KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>(new List<CrewMember> { crewMember }, new List<DialogueStateActionDTO> { reply }));
				}
			}
			return reactionEvents;
		}

		public List<PostSessionEventTrigger> GetEvents(List<DialogueStateActionDTO> available, List<PostSessionEventTrigger> triggers, Team team, int sessionsPerRace)
		{
			var history = team.LineUpHistory;
			var setEvents = new List<PostSessionEventTrigger>();
			var repeatedEvents = new List<PostSessionEventTrigger>();
			var randomEvents = new List<PostSessionEventTrigger>();
			foreach (var trigger in triggers)
			{
				if (available.Any(a => a.NextState == trigger.EventName))
				{
					if (trigger.StartBoatType == null || history.Any(h => h.Type == trigger.StartBoatType) || trigger.StartBoatType == team.Boat.Type)
					{
						var sessionCount = history.Count;
						var sessionsNeeded = (trigger.RaceTrigger * sessionsPerRace) + trigger.SessionTrigger;
						if (trigger.RepeatEvery == 0 && (trigger.StartBoatType == team.Boat.Type || trigger.StartBoatType == null))
						{
							if (trigger.StartBoatType != null)
							{
								sessionCount = history.Count(h => h.Type == trigger.StartBoatType);
							}
							if (sessionCount == sessionsNeeded)
							{
								setEvents.Add(trigger);
							}
						}
						else if (trigger.RepeatEvery > 0 && history.All(h => h.Type != trigger.EndBoatType))
						{
							if (trigger.StartBoatType != null)
							{
								sessionCount = history.Count - history.FindIndex(h => h.Type == trigger.StartBoatType);
							}
							if (sessionCount - sessionsNeeded >= 0 && (sessionCount - sessionsNeeded) % trigger.RepeatEvery == 0)
							{
								if (trigger.Random)
								{
									randomEvents.Add(trigger);
								}
								else
								{
									repeatedEvents.Add(trigger);
								}
							}
						}
					}
				}
			}
			return setEvents.Concat(repeatedEvents).Concat(randomEvents).ToList();
		}

		/// <summary>
		/// Send dialogue from the player to (an) NPC(s) and get their replies 
		/// </summary>
		public Dictionary<CrewMember, DialogueStateActionDTO> SendPostRaceEvent(DialogueStateActionDTO selected, List<CrewMember> crewMembers, Team team, Boat previous)
		{
			var replies = new Dictionary<CrewMember, DialogueStateActionDTO>();
			var replyCount = 0;
			foreach (var member in crewMembers)
			{
				var reply = member.SendPostRaceEvent(iat, selected, team, previous);
				team.Manager.UpdateSingleBelief(string.Format("PRECrew0({0})", replyCount), member.Name.NoSpaces());
				PostRaceEvents[0].Key[replyCount] = member;
				if (reply != null)
				{
					replies.Add(member, reply);
					team.Manager.UpdateSingleBelief(string.Format("PREEvent0({0})", replyCount), reply.CurrentState);
				}
				else
				{
					team.Manager.UpdateSingleBelief(string.Format("PREEvent0({0})", replyCount), "-");
				}
				PostRaceEvents[0].Value[replyCount] = reply;
				replyCount++;
			}
			team.Manager.SaveStatus();
			return replies;
		}

		public void SaveEvents(Person manager)
		{
			for (int i = 0; i < PostRaceEvents.Count; i++)
			{
				for (int j = 0; j < PostRaceEvents[i].Key.Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), PostRaceEvents[i].Key[j].Name.NoSpaces());
					if (PostRaceEvents[i].Value[j].NextState != "-")
					{
						manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i].Value[j].NextState);
					} else
					{
						manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i].Value[j].CurrentState);
					}
				}
			}
			manager.SaveStatus();
		}
	}
}
