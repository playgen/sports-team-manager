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
		public List<List<KeyValuePair<CrewMember, DialogueStateActionDTO>>> PostRaceEvents { get; private set; }

		public EventController(IntegratedAuthoringToolAsset i, List<DialogueStateActionDTO> help)
		{
			iat = i;
			learningPills = help;
			PostRaceEvents = new List<List<KeyValuePair<CrewMember, DialogueStateActionDTO>>>();
		}

		/// <summary>
		/// Get all player dialogues for their current state
		/// </summary>
		public Dictionary<CrewMember, List<DialogueStateActionDTO>> GetEventDialogues(Person manager)
		{
			Dictionary<CrewMember, List<DialogueStateActionDTO>> dialogueOptions = new Dictionary<CrewMember, List<DialogueStateActionDTO>>();
			foreach (var current in PostRaceEvents.First())
			{
				dialogueOptions.Add(current.Key, iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, current.Value.NextState.ToName()).OrderBy(c => Guid.NewGuid()).ToList());
			}
			if (dialogueOptions.Values.Sum(dos => dos.Count) == 0)
			{
				for (int i = 0; i < PostRaceEvents[0].Count; i++)
				{
					manager.EmotionalAppraisal.RemoveBelief(string.Format("PRECrew{0}({1})", PostRaceEvents.Count - 1, i), "SELF");
					manager.EmotionalAppraisal.RemoveBelief(string.Format("PREEvent{0}({1})", PostRaceEvents.Count - 1, i), "SELF");
				}
				PostRaceEvents.RemoveAt(0);
				SaveEvents(manager);
			}
			return dialogueOptions;
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
			var selectedEvents = new List<List<KeyValuePair<CrewMember, DialogueStateActionDTO>>>();
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
					var eventSelected = new List<KeyValuePair<CrewMember, DialogueStateActionDTO>>();
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
							eventSelected.Add(new KeyValuePair<CrewMember, DialogueStateActionDTO>(allCrew.OrderBy(c => Guid.NewGuid()).First().Value, selected));
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
							eventSelected.Add(new KeyValuePair<CrewMember, DialogueStateActionDTO>(allCrew.OrderBy(c => Guid.NewGuid()).First().Value, selected));
							eventSelected.ForEach(es => es.Key.UpdateSingleBelief("Event(Retire)", "1"));
							break;
					}
					eventSelected.ForEach(es => allCrew.Remove(es.Key.Name));
					selectedEvents.Add(eventSelected);
				}
			}
			PostRaceEvents = currentEvents.Concat(selectedEvents).ToList();
			SaveEvents(team.Manager);
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
		public List<List<KeyValuePair<CrewMember, DialogueStateActionDTO>>> GetLastingEvents(Team team, bool raceSession)
		{
			var reactionEvents = new List<List<KeyValuePair<CrewMember, DialogueStateActionDTO>>>();
			foreach (var crewMember in team.CrewMembers.Values)
			{
				var crewEvents = new List<KeyValuePair<CrewMember, DialogueStateActionDTO>>();
				var delayedReactions = crewMember.CurrentEventCheck(team, iat, raceSession);
				foreach (var reply in delayedReactions)
				{
					crewEvents.Add(new KeyValuePair<CrewMember, DialogueStateActionDTO>(crewMember, reply));
				}
				if (crewEvents.Count > 0)
				{
					reactionEvents.Add(crewEvents);
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
		public Dictionary<CrewMember, DialogueStateActionDTO> SendPostRaceEvent(Dictionary<CrewMember, DialogueStateActionDTO> selected, Team team, Boat previous)
		{
			var replies = new Dictionary<CrewMember, DialogueStateActionDTO>();
			var replyCount = 0;
			foreach (var response in selected)
			{
				var reply = response.Key.SendPostRaceEvent(iat, response.Value, team, previous);
				team.Manager.UpdateSingleBelief(string.Format("PRECrew0({0})", replyCount), response.Key.Name.NoSpaces());
				if (reply != null)
				{
					replies.Add(response.Key, reply);
					team.Manager.UpdateSingleBelief(string.Format("PREEvent0({0})", replyCount), reply.CurrentState);
				}
				else
				{
					replies.Add(response.Key, null);
					team.Manager.UpdateSingleBelief(string.Format("PREEvent0({0})", replyCount), "-");
				}
				PostRaceEvents[0][replyCount] = new KeyValuePair<CrewMember, DialogueStateActionDTO>(response.Key, reply);
				replyCount++;
			}
			team.Manager.SaveStatus();
			return replies;
		}

		public void SaveEvents(Person manager)
		{
			for (int i = 0; i < PostRaceEvents.Count; i++)
			{
				for (int j = 0; j < PostRaceEvents[i].Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), PostRaceEvents[i][j].Key.Name.NoSpaces());
					if (PostRaceEvents[i][j].Value.NextState != "-")
					{
						manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i][j].Value.NextState);
					} else
					{
						manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i][j].Value.CurrentState);
					}
				}
			}
			manager.SaveStatus();
		}
	}
}
