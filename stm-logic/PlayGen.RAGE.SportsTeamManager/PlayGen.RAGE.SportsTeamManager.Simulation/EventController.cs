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

		internal EventController(IntegratedAuthoringToolAsset i, List<DialogueStateActionDTO> help)
		{
			iat = i;
			learningPills = help;
			PostRaceEvents = new List<List<PostRaceEventState>>();
		}

		/// <summary>
		/// Get the names of possible post race events
		/// </summary>
		public List<string> GetEventKeys()
		{
			return iat.GetDialogueActionsByState(IATConsts.AGENT, "PostRaceEventStart").Select(d => d.NextState).ToList();
		}

		/// <summary>
		/// Get the dialogue options for the current event for the player
		/// </summary>
		public List<PostRaceEventState> GetEventDialogues(Person manager)
		{
			var dialogueOptions = new List<PostRaceEventState>();
			foreach (var current in PostRaceEvents.First())
			{
				var dialogues = iat.GetDialogueActionsByState(IATConsts.PLAYER, current.Dialogue.NextState).ToList();
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
		/// Get all player dialogues strings with the eventKey provided
		/// </summary>
		public string[] GetEventStrings(string eventKey)
		{
			var dialogueOptions = iat.GetDialogueActionsByState(IATConsts.PLAYER, eventKey);
			return dialogueOptions.Select(dia => dia.Utterance).ToArray();
		}

		/// <summary>
		/// Get all player dialogue conflict management styles
		/// </summary>
		public string[] GetPlayerEventStyles()
		{
			return iat.GetDialogueActionsBySpeaker(IATConsts.PLAYER).Where(s => s.NextState != "-").SelectMany(s => s.Meaning).Distinct().ToArray();
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
		internal List<string> SendMeetingEvent(string eventName, CrewMember member, Team team)
		{
			return member.SendMeetingEvent(iat, eventName, team);
		}

		/// <summary>
		/// Send recruitment dialogue, getting their responses in return
		/// </summary>
		internal Dictionary<CrewMember, string> SendRecruitEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var replies = new Dictionary<CrewMember, string>();
			foreach (var member in members)
			{
				var reply = member.SendRecruitEvent(iat, skill);
				replies.Add(member, reply ?? string.Empty);
			}
			return replies;
		}

		/// <summary>
		/// Select event(s) to trigger post race
		/// </summary>
		internal void SelectPostRaceEvents(ConfigStore config, Team team, int chance)
		{
			//get the state of currently running events
			CheckLastingEvents(team);
			var selectedEvents = new List<List<PostRaceEventState>>();
			//get all possible post-race event starting dialogue
			var dialogueOptions = GetPossiblePostRaceDialogue();
			//get events that can be fired according to the game config
			var events = GetEvents(dialogueOptions, config.GameConfig.EventTriggers.ToList(), team);
			if (events.Any())
			{
				var allCrewInitial = team.CrewMembers;
				var allCrew = new Dictionary<string, CrewMember>();
				//remove those already involved in a running event to not be selected
				foreach (var crewMember in allCrewInitial.Values)
				{
					var expectsPos = crewMember.LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription());
					var expectsPosAfter = crewMember.LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription());
					var expectsSelection = crewMember.LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription());
					if (!(expectsPos != null && expectsPos != "null") && !(expectsPosAfter != null && expectsPosAfter != "null") && !(expectsSelection != null && expectsSelection != "null"))
					{
						allCrew.Add(crewMember.Name, crewMember);
					}
				}
				foreach (var ev in events)
				{
					//if there is no-one to select for an event, stop looking
					if (allCrew.Count == 0)
					{
						continue;
					}
					var selected = dialogueOptions.Where(a => a.NextState == ev.EventName).OrderBy(o => Guid.NewGuid()).First();
					if (ev.Random && StaticRandom.Int(0, (int)Math.Pow(chance, selectedEvents.Count + 1)) != 0)
					{
						continue;
					}
					var eventSelected = new List<PostRaceEventState>();
					switch (selected.NextState)
					{
						case "PW":
							//for this event, select a crew member who is placed in the wrong position or, if all are placed correctly, a crew member who is not placed
							var betterPlace = new List<KeyValuePair<CrewMember, Position>>();
							foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								var betterPosition = CrewMemberBestPosition(pair.Value, team);
								if (betterPosition.Key != Position.Null)
								{
									betterPlace.Add(new KeyValuePair<CrewMember, Position>(pair.Value, betterPosition.Key));
								}
								allCrew.Remove(pair.Value.Name);
							}
							//if nobody currently placed can be pout into a better position, select a placed crew member and position at random
							if (!betterPlace.Any())
							{
								var selectedCrewMember = allCrew.OrderBy(c => Guid.NewGuid()).First().Value;
								betterPlace.Add(new KeyValuePair<CrewMember, Position>(selectedCrewMember, CrewMemberBestPosition(selectedCrewMember, team).Key));
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
								if (allCrew.Count == 0)
								{
									continue;
								}
							}
							var selectedFor = allCrew.OrderBy(c => Guid.NewGuid()).First();
							selectedFor.Value.AddOrUpdateOpinion(selectedAgainst.Key, -10);
							selectedFor.Value.AddOrUpdateRevealedOpinion(selectedAgainst.Key, selectedFor.Value.CrewOpinions[selectedAgainst.Key]);
							selectedFor.Value.SaveStatus();
							foreach (var cm in team.CrewMembers)
							{
								if (cm.Key != selectedFor.Key && cm.Key != selectedAgainst.Key)
								{
									cm.Value.AddOrUpdateOpinion(selectedAgainst.Key, StaticRandom.Int(-3, 1));
									cm.Value.SaveStatus();
								}
							}
							eventSelected.Add(new PostRaceEventState(selectedFor.Value, selected, new[] { selectedAgainst.Key.NoSpaces() }.ToList()));
							break;
						case "NotPicked":
							//for this event, select a crew member who was not selected
							foreach (var pair in team.LineUpHistory.Last().PositionCrew)
							{
								allCrew.Remove(pair.Value.Name);
							}
							if (allCrew.Count == 0)
							{
								continue;
							}
							allCrew = allCrew.OrderBy(c => Guid.NewGuid()).ToDictionary(d => d.Key, d => d.Value);
							var randomCrewMember = allCrew.First();
							var randomBestPosition = CrewMemberBestPosition(randomCrewMember.Value, team);
							var randomPositionCurrent = team.LineUpHistory.Last().PositionCrew[randomBestPosition.Key].Name;
							eventSelected.Add(new PostRaceEventState(randomCrewMember.Value, selected,
												new[] { randomBestPosition.Key.ToString(), randomPositionCurrent.NoSpaces() }.ToList()));
							break;
						case "IPC":
							//for this event, select a crew member to have a conflict with another crew member
							if (!team.LineUpHistory.Last().PositionCrew.ContainsKey(Position.Skipper))
							{
								continue;
							}
							var skipper = team.LineUpHistory.Last().PositionCrew[Position.Skipper].Name;
							if (allCrew.ContainsKey(skipper))
							{
								allCrew.Remove(skipper);
							}
							if (allCrew.Count == 0)
							{
								continue;
							}
							var randomAdditional = allCrew.OrderBy(c => Guid.NewGuid()).First();
							allCrew.Remove(randomAdditional.Key);
							if (allCrew.Count == 0)
							{
								continue;
							}
							allCrew = allCrew.OrderBy(c => Guid.NewGuid()).ToDictionary(d => d.Key, d => d.Value);
							eventSelected.Add(new PostRaceEventState(allCrew.First().Value, selected,
												new[] { skipper.NoSpaces(), randomAdditional.Key.NoSpaces() }.ToList()));
							break;
					}
					eventSelected.ForEach(es => allCrew.Remove(es.CrewMember.Name));
					selectedEvents.Add(eventSelected);
				}
			}
			PostRaceEvents = selectedEvents;
			SaveEvents(team.Manager);
		}

		/// <summary>
		/// Get the best possible position for a crew member
		/// </summary>
		private KeyValuePair<Position, int> CrewMemberBestPosition(CrewMember cm, Team team)
		{
			var betterPosition = new KeyValuePair<Position, int>(Position.Null, 0);
			var currentPosition = team.LineUpHistory.Last().PositionCrew.SingleOrDefault(pair => pair.Value == cm).Key;
			foreach (var boatPosition in team.LineUpHistory.Last().Positions)
			{
				if (boatPosition == currentPosition)
				{
					continue;
				}
				var possiblePositionScore = boatPosition.GetPositionRating(cm);
				if ((currentPosition != Position.Null && possiblePositionScore > team.LineUpHistory.Last().PositionScores[currentPosition]) || possiblePositionScore > betterPosition.Value)
				{
					betterPosition = new KeyValuePair<Position, int>(boatPosition, possiblePositionScore);
				}
			}
			return betterPosition;
		}

		/// <summary>
		/// Get all post-race starting dialogue based on the last type of session
		/// </summary>
		internal List<DialogueStateActionDTO> GetPossiblePostRaceDialogue()
		{
			var dialogueOptions = GetPossibleAgentDialogue("PostRaceEventStart");
			dialogueOptions = dialogueOptions.Where(dia => dia.Style.Contains("Race")).ToList();
			return dialogueOptions;
		}

		/// <summary>
		/// Get all post-race starting dialogue based on the string provided
		/// </summary>
		internal List<DialogueStateActionDTO> GetPossibleAgentDialogue(string eventName)
		{
			return iat.GetDialogueActionsByState(IATConsts.AGENT, eventName).OrderBy(c => Guid.NewGuid()).ToList();
		}

		/// <summary>
		/// Check the currently running events for all CrewMembers
		/// </summary>
		private void CheckLastingEvents(Team team)
		{
			foreach (var crewMember in team.CrewMembers.Values)
			{
				crewMember.CurrentEventCheck(team, iat);
			}
		}

		/// <summary>
		/// Get a list of events that should be triggered
		/// </summary>
		private List<PostSessionEventTrigger> GetEvents(List<DialogueStateActionDTO> available, List<PostSessionEventTrigger> triggers, Team team)
		{
			var raceHistory = team.RaceHistory;
			var setEvents = new List<PostSessionEventTrigger>();
			var repeatedEvents = new List<PostSessionEventTrigger>();
			var randomEvents = new List<PostSessionEventTrigger>();
			foreach (var trigger in triggers)
			{
				if (available.Any(a => a.NextState == trigger.EventName))
				{
					if (trigger.StartBoatType == null || raceHistory.Any(h => h.Type == trigger.StartBoatType) || trigger.StartBoatType == team.Boat.Type)
					{
						var raceCount = raceHistory.Count;
						var racesNeeded = trigger.RaceTrigger;
						if (trigger.RepeatEvery == 0 && (trigger.StartBoatType == team.Boat.Type || trigger.StartBoatType == null))
						{
							if (trigger.StartBoatType != null)
							{
								raceCount = raceHistory.Count(h => h.Type == trigger.StartBoatType);
							}
							if (raceCount == racesNeeded)
							{
								setEvents.Add(trigger);
							}
						}
						else if (trigger.RepeatEvery > 0 && raceHistory.All(h => h.Type != trigger.EndBoatType))
						{
							if (trigger.StartBoatType != null)
							{
								raceCount = raceHistory.Count - raceHistory.FindIndex(h => h.Type == trigger.StartBoatType);
							}
							if (raceCount - racesNeeded >= 0 && (raceCount - racesNeeded) % trigger.RepeatEvery == 0)
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
		internal List<PostRaceEventState> SendPostRaceEvent(List<PostRaceEventState> selected, Team team)
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
				var subjects = response.Subjects.Count > 0 ? string.Join("_", response.Subjects.ToArray()) : "null";
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

		/// <summary>
		/// Set to null event records from the manager's RPC file beliefs
		/// </summary>
		private void RemoveEvents(Person manager)
		{
			for (var i = 0; i < PostRaceEvents.Count; i++)
			{
				for (var j = 0; j < PostRaceEvents[i].Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), "null");
					manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), "null");
					manager.UpdateSingleBelief(string.Format("PRESubject{0}({1})", i, j), "null");
				}
			}
			manager.SaveStatus();
		}

		/// <summary>
		/// Update the count of possible replies of each meaning based on the possible reopies that could have been given
		/// </summary>
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

		/// <summary>
		/// Update the count of possible replies of each style based on the possible reopies that could have been given
		/// </summary>
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

		/// <summary>
		/// Update the count of replies of each meaning based on the last reply given
		/// </summary>
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

		/// <summary>
		/// Update the count of replies of each style based on the last reply given
		/// </summary>
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

		/// <summary>
		/// Save the current state of events to the manager's RPC beliefs
		/// </summary>
		private void SaveEvents(Person manager)
		{
			for (var i = 0; i < PostRaceEvents.Count; i++)
			{
				for (var j = 0; j < PostRaceEvents[i].Count; j++)
				{
					manager.UpdateSingleBelief(string.Format("PRECrew{0}({1})", i, j), PostRaceEvents[i][j].CrewMember.Name.NoSpaces());
					manager.UpdateSingleBelief(string.Format("PREEvent{0}({1})", i, j), PostRaceEvents[i][j].Dialogue.NextState != "-" ?
												PostRaceEvents[i][j].Dialogue.NextState : PostRaceEvents[i][j].Dialogue.CurrentState);
					var subjects = PostRaceEvents[i][j].Subjects.Count > 0 ? string.Join("_", PostRaceEvents[i][j].Subjects.ToArray()) : "null";
					manager.UpdateSingleBelief(string.Format("PRESubject{0}({1})", i, j), subjects);
				}
			}
			manager.SaveStatus();
		}
	}
}
