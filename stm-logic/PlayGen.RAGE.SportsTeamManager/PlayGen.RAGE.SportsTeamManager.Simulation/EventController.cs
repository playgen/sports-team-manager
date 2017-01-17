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
		public List<List<PostRaceEventState>> PostRaceEvents { get; private set; }

		public EventController(IntegratedAuthoringToolAsset i, List<DialogueStateActionDTO> help)
		{
			iat = i;
			learningPills = help;
			PostRaceEvents = new List<List<PostRaceEventState>>();
		}

		public List<PostRaceEventState> GetEventDialogues(Person manager)
		{
			List<PostRaceEventState> dialogueOptions = new List<PostRaceEventState>();
			foreach (var current in PostRaceEvents.First())
			{
				var dialogues = iat.GetDialogueActions(IntegratedAuthoringToolAsset.PLAYER, current.Dialogue.NextState.ToName()).ToList();
				dialogues = dialogues.OrderBy(c => Guid.NewGuid()).ToList();
				var events = dialogues.Select(d => new PostRaceEventState(current.CrewMember, d, current.Subjects)).ToList();
				dialogueOptions.AddRange(events);
			}
			if (dialogueOptions.Count == 0)
			{
				RemoveEvents(manager);
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
		public List<string> SendMeetingEvent(string eventName, CrewMember member, Team team)
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
			var selectedEvents = new List<List<PostRaceEventState>>();
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
					var expectsPos = crewMember.LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription());
					var expectsPosAfter = crewMember.LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription());
					if ((expectsPos != null && expectsPos != "null") || (expectsPosAfter != null && expectsPosAfter != "null"))
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
					var eventSelected = new List<PostRaceEventState>();
					switch (selected.NextState)
					{
						case "PW":
							var betterPlace = new List<KeyValuePair<CrewMember, Position>>();
							foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								var betterPosition = new KeyValuePair<Position, int>(Position.Null, 0);
								foreach (Position boatPosition in team.LineUpHistory.Last().Positions)
								{
									if (boatPosition == betterPosition.Key || boatPosition == pair.Key)
									{
										continue;
									}
									int possiblePositionScore = boatPosition.GetPositionRating(pair.Value);
									if (possiblePositionScore > team.LineUpHistory.Last().PositionScores[pair.Key] && possiblePositionScore > betterPosition.Value)
									{
										betterPosition = new KeyValuePair<Position, int>(boatPosition, possiblePositionScore);
									}
								}
								if (betterPosition.Key != Position.Null)
								{
									betterPlace.Add(new KeyValuePair<CrewMember, Position>(pair.Value, betterPosition.Key));
								}
								allCrew.Remove(pair.Value.Name);
							}
							if (betterPlace.Count == 0)
							{
								var selectedCrewMember = allCrew.OrderBy(c => Guid.NewGuid()).First().Value;
								var betterPosition = new KeyValuePair<Position, int>(Position.Null, 0);
								foreach (Position boatPosition in team.LineUpHistory.Last().Positions)
								{
									int possiblePositionScore = boatPosition.GetPositionRating(selectedCrewMember);
									if (possiblePositionScore > betterPosition.Value)
									{
										betterPosition = new KeyValuePair<Position, int>(boatPosition, possiblePositionScore);
									}
								}
								betterPlace.Add(new KeyValuePair<CrewMember, Position>(selectedCrewMember, betterPosition.Key));
							}
							betterPlace = betterPlace.OrderBy(c => Guid.NewGuid()).ToList();
							eventSelected.Add(new PostRaceEventState(betterPlace.First().Key, selected, new[] { betterPlace.First().Value.ToString() }.ToList()));
							break;
						case "OO":
							//for this event, select a crew member who is placed with someone they do not get on with
							var selectedAgainst = team.CrewMembers.OrderBy(c => c.Value.RevealedSkills.Values.Sum()).First();
							if (allCrew.ContainsKey(selectedAgainst.Key))
							{
								allCrew.Remove(selectedAgainst.Key);
							}
							var selectedFor = allCrew.OrderBy(c => Guid.NewGuid()).First();
							selectedFor.Value.AddOrUpdateOpinion(selectedAgainst.Key, -10);
							selectedFor.Value.AddOrUpdateRevealedOpinion(selectedAgainst.Key, selectedFor.Value.CrewOpinions[selectedAgainst.Key]);
							selectedFor.Value.SaveStatus();
							foreach (var cm in team.CrewMembers)
							{
								if (cm.Key != selectedFor.Key && cm.Key != selectedAgainst.Key)
								{
									cm.Value.AddOrUpdateOpinion(team.Manager.Name, StaticRandom.Int(-3, 1));
									cm.Value.SaveStatus();
								}
							}
							eventSelected.Add(new PostRaceEventState(selectedFor.Value, selected, new[] { selectedAgainst.Key.NoSpaces() }.ToList()));
							break;
						case "NotPicked":
							//for this event, select a crew member who was not selected
							/*foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								allCrew.Remove(pair.Value.Name);
							}
							if (allCrew.Count == 0)
							{
								continue;
							}
							allCrew = allCrew.OrderBy(c => Guid.NewGuid()).ToDictionary(d => d.Key, d => d.Value);
							eventSelected.Add(new KeyValuePair<CrewMember, DialogueStateActionDTO>(allCrew.First().Value, selected));*/
							break;
						case "IPC":
							//for this event, select a crew member to have a conflict with another crew member
							break;
					}
					eventSelected.ForEach(es => allCrew.Remove(es.CrewMember.Name));
					selectedEvents.Add(eventSelected);
				}
			}
			PostRaceEvents = currentEvents.Concat(selectedEvents).ToList();
			SaveEvents(team.Manager);
		}

		/// <summary>
		/// Get all post-race starting dialogue based on the last type of session
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
		public List<List<PostRaceEventState>> GetLastingEvents(Team team, bool raceSession)
		{
			var reactionEvents = new List<List<PostRaceEventState>>();
			foreach (var crewMember in team.CrewMembers.Values)
			{
				var crewEvents = new List<PostRaceEventState>();
				var delayedReactions = crewMember.CurrentEventCheck(team, iat, raceSession);
				foreach (var reply in delayedReactions)
				{
					crewEvents.Add(reply);
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
		public List<PostRaceEventState> SendPostRaceEvent(List<PostRaceEventState> selected, Team team)
		{
			var manager = team.Manager;
			var replies = new List<PostRaceEventState>();
			var possibleDialogue = GetEventDialogues(manager);
			SavePossibleMeaningCount(manager, possibleDialogue.Select(d => d.Dialogue.Meaning).ToList());
			SavePossibleStyleCount(manager, possibleDialogue.Select(d => d.Dialogue.Style).ToList());
			foreach (var response in selected)
			{
				var replyCount = PostRaceEvents[0].FindIndex(pre => pre.CrewMember == response.CrewMember);
				var reply = response.CrewMember.SendPostRaceEvent(iat, response.Dialogue, team, response.Subjects);
				manager.UpdateSingleBelief(string.Format("PRECrew0({0})", replyCount), response.CrewMember.Name.NoSpaces());
				var subjects = response.Subjects.Count > 0 ? string.Join(",", response.Subjects.ToArray()) : "null";
				manager.UpdateSingleBelief(string.Format("PRESubject0({0})", replyCount), subjects);
				SaveMeaningSelected(manager, response.Dialogue.Meaning);
				SaveStyleSelected(manager, response.Dialogue.Style);
				if (reply != null)
				{
					var newPre = new PostRaceEventState(response.CrewMember, reply, response.Subjects);
					replies.Add(newPre);
					manager.UpdateSingleBelief(string.Format("PREEvent0({0})", replyCount), reply.CurrentState);
					PostRaceEvents[0][replyCount] = newPre;
				}
				else
				{
					replies.Add(new PostRaceEventState(response.CrewMember, PostRaceEvents[0][replyCount].Dialogue, response.Subjects));
				}
			}
			manager.SaveStatus();
			return replies;
		}

		public void RemoveEvents(Person manager)
		{
			for (int i = 0; i < PostRaceEvents.Count; i++)
			{
				for (int j = 0; j < PostRaceEvents[i].Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), "null");
					manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), "null");
					manager.UpdateSingleBelief(string.Format("PRESubject{0}({1})", i, j), "null");
				}
			}
			manager.SaveStatus();
		}

		private void SavePossibleMeaningCount(Person manager, List<string[]> possibleMeanings)
		{
			foreach (var meanings in possibleMeanings)
			{
				foreach (var meaning in meanings)
				{
					if (meaning == "-")
					{
						continue;
					}
					int currentCount;
					int.TryParse(manager.LoadBelief(string.Format("PossibleMeaning({0})", meaning)), out currentCount);
					manager.UpdateSingleBelief(string.Format("PossibleMeaning({0})", meaning), (currentCount + 1).ToString());
				}
			}
		}

		private void SavePossibleStyleCount(Person manager, List<string[]> possibleStyles)
		{
			foreach (var styles in possibleStyles)
			{
				foreach (var style in styles)
				{
					if (style == "-")
					{
						continue;
					}
					int currentCount;
					int.TryParse(manager.LoadBelief(string.Format("PossibleStyle({0})", style)), out currentCount);
					manager.UpdateSingleBelief(string.Format("PossibleStyle({0})", style), (currentCount + 1).ToString());
				}
			}
		}

		private void SaveMeaningSelected(Person manager, string[] meanings)
		{
			foreach (var meaning in meanings)
			{
				if (meaning == "-")
				{
					continue;
				}
				int currentCount;
				int.TryParse(manager.LoadBelief(string.Format("Meaning({0})", meaning)), out currentCount);
				manager.UpdateSingleBelief(string.Format("Meaning({0})", meaning), (currentCount + 1).ToString());
			}
		}

		private void SaveStyleSelected(Person manager, string[] styles)
		{
			foreach (var style in styles)
			{
				if (style == "-")
				{
					continue;
				}
				int currentCount;
				int.TryParse(manager.LoadBelief(string.Format("Style({0})", style)), out currentCount);
				manager.UpdateSingleBelief(string.Format("Style({0})", style), (currentCount + 1).ToString());
			}
		}

		public void SaveEvents(Person manager)
		{
			for (int i = 0; i < PostRaceEvents.Count; i++)
			{
				for (int j = 0; j < PostRaceEvents[i].Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), PostRaceEvents[i][j].CrewMember.Name.NoSpaces());
					manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i][j].Dialogue.NextState != "-" ?
												PostRaceEvents[i][j].Dialogue.NextState : PostRaceEvents[i][j].Dialogue.CurrentState);
					var subjects = PostRaceEvents[i][j].Subjects.Count > 0 ? string.Join(",", PostRaceEvents[i][j].Subjects.ToArray()) : "null";
					manager.UpdateSingleBelief(string.Format("PRESubject{0}({1})", i, j), subjects);
				}
			}
			manager.SaveStatus();
		}
	}
}
