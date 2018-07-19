using System;
using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Handles NPC dialogue events
	/// </summary>
	public class EventController
	{
		private readonly IntegratedAuthoringToolAsset iat;
		public List<List<PostRaceEventState>> PostRaceEvents { get; private set; }

		internal EventController(IntegratedAuthoringToolAsset i)
		{
			iat = i;
			PostRaceEvents = new List<List<PostRaceEventState>>();
		}

		/// <summary>
		/// Get the names of possible post race events
		/// </summary>
		public List<string> GetEventKeys()
		{
			return iat.GetDialogueActionsByState("NPC_PostRaceEventStart").Select(d => d.NextState.Split('_')[1]).ToList();
		}

		/// <summary>
		/// Get the dialogue options for the current event for the player
		/// </summary>
		public List<PostRaceEventState> GetEventDialogues(Person manager)
		{
			var dialogueOptions = new List<PostRaceEventState>();
			foreach (var current in PostRaceEvents.First())
			{
				var dialogues = iat.GetDialogueActionsByState(current.Dialogue.NextState).OrderBy(c => Guid.NewGuid()).ToList();
				dialogueOptions.AddRange(dialogues.Select(d => new PostRaceEventState(current.CrewMember, d, current.Subjects)).ToList());
			}
			if (dialogueOptions.Count == 0)
			{
				RemoveEvents(manager);
			}
			return dialogueOptions;
		}

		/// <summary>
		/// Get all player dialogues strings with the eventKey provided
		/// </summary>
		public string[] GetEventStrings(string eventKey)
		{
			return iat.GetDialogueActionsByState("Player_" + eventKey).Select(dia => dia.Utterance).ToArray();
		}

		/// <summary>
		/// Get all player dialogue conflict management styles
		/// </summary>
		public string[] GetPlayerEventStyles()
		{
			return iat.GetAllDialogueActions().Where(s => s.CurrentState.StartsWith("Player_") && s.NextState != Name.NIL_STRING).SelectMany(s => s.Meaning.Split('_').Where(sp => !string.IsNullOrEmpty(sp))).Distinct().ToArray();
		}

		/// <summary>
		/// Get help dialogue from those available
		/// </summary>
		public string GetHelpText(string key)
		{
			return iat.GetDialogueActionsByState("LearningPill").Where(hd => hd.NextState == key).OrderBy(o => Guid.NewGuid()).FirstOrDefault()?.Utterance ?? string.Empty;
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
		internal Dictionary<CrewMember, string> SendRecruitEvent(Skill skill, List<CrewMember> members)
		{
			return members.ToDictionary(m => m, m => m.SendRecruitEvent(iat, skill) ?? string.Empty);
		}

		/// <summary>
		/// Select event(s) to trigger post race
		/// </summary>
		internal void SelectPostRaceEvents(Team team, int chance)
		{
			//get the state of currently running events
			foreach (var crewMember in team.CrewMembers.Values)
			{
				crewMember.CurrentEventCheck(team, iat);
			}
			var selectedEvents = new List<List<PostRaceEventState>>();
			//get all possible post-race event starting dialogue
			var dialogueOptions = GetPossiblePostRaceDialogue();
			//get events that can be fired according to the game config
			var events = GetEvents(dialogueOptions, ConfigStore.GameConfig.EventTriggers.ToList(), team);
			if (events.Any())
			{
				var allCrewInitial = team.CrewMembers;
				var allCrew = new Dictionary<string, CrewMember>();
				//remove those already involved in a running event to not be selected
				foreach (var crewMember in allCrewInitial.Values)
				{
					var expectsPos = crewMember.LoadBelief(NPCBelief.ExpectedPosition);
					var expectsPosAfter = crewMember.LoadBelief(NPCBelief.ExpectedPosition);
					var expectsSelection = crewMember.LoadBelief(NPCBelief.ExpectedSelection);
					if (expectsPos == null && expectsPosAfter == null && expectsSelection == null)
					{
						allCrew.Add(crewMember.Name, crewMember);
					}
				}
				foreach (var ev in events)
				{
					var eventCrew = new Dictionary<string, CrewMember>(allCrew);
					//if there is no-one to select for an event, stop looking
					if (eventCrew.Count == 0)
					{
						continue;
					}
					var selected = dialogueOptions.Where(a => a.NextState == "Player_" + ev.EventName).OrderBy(o => Guid.NewGuid()).First();
					if (ev.Random && StaticRandom.Int(0, (int)Math.Pow(chance, selectedEvents.Count + 1)) != 0)
					{
						continue;
					}
					var eventSelected = new List<PostRaceEventState>();
					switch (selected.NextState.Replace("Player_", string.Empty))
					{
						case "PW":
							//for this event, select a crew member who feels that they were placed in the wrong position
							var betterPlace = new List<KeyValuePair<CrewMember, Position>>();
							foreach (var pair in team.PreviousSession.PositionCrew)
							{
								var betterPosition = CrewMemberBestPosition(pair.Value, team);
								if (betterPosition.Key != Position.Null)
								{
									betterPlace.Add(new KeyValuePair<CrewMember, Position>(pair.Value, betterPosition.Key));
								}
								eventCrew.Remove(pair.Value.Name);
							}
							//if nobody currently placed can be pout into a better position, select a placed crew member and position at random
							if (!betterPlace.Any())
							{
								var selectedCrewMember = eventCrew.OrderBy(c => Guid.NewGuid()).First().Value;
								betterPlace.Add(new KeyValuePair<CrewMember, Position>(selectedCrewMember, CrewMemberBestPosition(selectedCrewMember, team).Key));
							}
							betterPlace = betterPlace.OrderBy(c => Guid.NewGuid()).ToList();
							eventSelected.Add(new PostRaceEventState(betterPlace.First().Key, selected, new[] { betterPlace.First().Value.ToString() }.ToList()));
							break;
						case "OO":
							//for this event, select a crew member who is placed with someone they do not get on with
							var selectedAgainst = team.CrewMembers.OrderByDescending(c => c.Value.RevealedSkills.Values.Sum()).First();
							if (eventCrew.ContainsKey(selectedAgainst.Key))
							{
								eventCrew.Remove(selectedAgainst.Key);
								if (eventCrew.Count == 0)
								{
									continue;
								}
							}
							var selectedFor = eventCrew.OrderBy(c => Guid.NewGuid()).First();
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
							foreach (var pair in team.PreviousSession.PositionCrew)
							{
								eventCrew.Remove(pair.Value.Name);
							}
							if (eventCrew.Count == 0)
							{
								continue;
							}
							var randomCrewMember = eventCrew.OrderBy(c => Guid.NewGuid()).First();
							var randomBestPosition = CrewMemberBestPosition(randomCrewMember.Value, team);
							var randomPositionCurrent = team.PreviousSession.PositionCrew[randomBestPosition.Key].Name;
							eventSelected.Add(new PostRaceEventState(randomCrewMember.Value, selected, new[] { randomBestPosition.Key.ToString(), randomPositionCurrent.NoSpaces() }.ToList()));
							break;
						case "IPC":
							//for this event, select a crew member to have a conflict with the skipper
							if (!team.PreviousSession.PositionCrew.ContainsKey(Position.Skipper))
							{
								continue;
							}
							var skipper = team.PreviousSession.PositionCrew[Position.Skipper].Name;
							if (eventCrew.ContainsKey(skipper))
							{
								eventCrew.Remove(skipper);
							}
							if (eventCrew.Count == 0)
							{
								continue;
							}
							var randomAdditional = eventCrew.OrderBy(c => Guid.NewGuid()).First();
							eventCrew.Remove(randomAdditional.Key);
							if (eventCrew.Count == 0)
							{
								continue;
							}
							eventSelected.Add(new PostRaceEventState(eventCrew.OrderBy(c => Guid.NewGuid()).First().Value, selected, new[] { skipper.NoSpaces(), randomAdditional.Key.NoSpaces() }.ToList()));
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
		/// Get the best possible position for a crew member ohter than their current position
		/// </summary>
		private KeyValuePair<Position, int> CrewMemberBestPosition(CrewMember cm, Team team)
		{
			var betterPosition = new KeyValuePair<Position, int>(Position.Null, 0);
			var currentPosition = team.PreviousSession.PositionCrew.SingleOrDefault(pair => pair.Value == cm).Key;
			foreach (var boatPosition in team.PreviousSession.Positions)
			{
				if (boatPosition == currentPosition)
				{
					continue;
				}
				var possiblePositionScore = boatPosition.GetPositionRating(cm);
				if ((currentPosition != Position.Null && possiblePositionScore > team.PreviousSession.PositionScores[currentPosition]) || possiblePositionScore > betterPosition.Value)
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
			return iat.GetDialogueActionsByState("NPC_PostRaceEventStart").Where(dia => dia.Style.Contains("Race")).OrderBy(c => Guid.NewGuid()).ToList();
		}

		/// <summary>
		/// Get all post-race starting dialogue based on the string provided
		/// </summary>
		internal List<DialogueStateActionDTO> GetPossibleAgentDialogue(string eventName)
		{
			return iat.GetAllDialogueActions().Where(d => d.NextState == eventName).OrderBy(c => Guid.NewGuid()).ToList();
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
				if (available.Any(a => a.NextState == "Player_" + trigger.EventName))
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
			SavePossibleMeaningCount(manager, possibleDialogue.Select(d => d.Dialogue.Meaning.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToArray()).ToList());
			SavePossibleStyleCount(manager, possibleDialogue.Select(d => d.Dialogue.Style.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToArray()).ToList());
			foreach (var response in selected)
			{
				var replyCount = PostRaceEvents[0].FindIndex(pre => pre.CrewMember == response.CrewMember);
				var reply = response.CrewMember.SendPostRaceEvent(iat, response.Dialogue, team, response.Subjects);
				manager.UpdateSingleBelief($"PRECrew0({replyCount})", response.CrewMember.Name);
				var subjects = response.Subjects.Count > 0 ? string.Join("_", response.Subjects.ToArray()) : null;
				manager.UpdateSingleBelief($"PRESubject0({replyCount})", subjects);
				SaveMeaningSelected(manager, response.Dialogue.Meaning.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToArray());
				SaveStyleSelected(manager, response.Dialogue.Style.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToArray());
				if (reply != null)
				{
					var newPre = new PostRaceEventState(response.CrewMember, reply, response.Subjects);
					replies.Add(newPre);
					manager.UpdateSingleBelief($"PREEvent0({replyCount})", reply.NextState);
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
					manager.UpdateSingleBelief($"PRECrew{i}({j})");
					manager.UpdateSingleBelief($"PREEvent{i}({j})");
					manager.UpdateSingleBelief($"PRESubject{i}({j})");
				}
			}
			PostRaceEvents.RemoveAt(0);
			SaveEvents(manager);
		}

		/// <summary>
		/// Update the count of possible replies of each meaning based on the possible replies that could have been given
		/// </summary>
		private void SavePossibleMeaningCount(Person manager, List<string[]> possibleMeanings)
		{
			foreach (var meanings in possibleMeanings)
			{
				foreach (var meaning in meanings)
				{
					SaveBelief(manager, "PossibleMeaning({0})", meaning);
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
					SaveBelief(manager, "PossibleStyle({0})", style);
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
				SaveBelief(manager, "Meaning({0})", meaning);
			}
		}

		/// <summary>
		/// Update the count of replies of each style based on the last reply given
		/// </summary>
		private void SaveStyleSelected(Person manager, string[] styles)
		{
			foreach (var style in styles)
			{
				SaveBelief(manager, "Style({0})", style);
			}
		}

		private void SaveBelief(Person manager, string saveType, string saveValue)
		{
			if (saveValue == Name.NIL_STRING)
			{
				return;
			}
			int.TryParse(manager.LoadBelief(string.Format(saveType, saveValue)), out var currentCount);
			manager.UpdateSingleBelief(string.Format(saveType, saveValue), (currentCount + 1));
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
					manager.UpdateSingleBelief($"PRECrew{i}({j})", PostRaceEvents[i][j].CrewMember.Name);
					manager.UpdateSingleBelief($"PREEvent{i}({j})", PostRaceEvents[i][j].Dialogue.NextState != Name.NIL_STRING ? PostRaceEvents[i][j].Dialogue.NextState : PostRaceEvents[i][j].Dialogue.CurrentState);
					var subjects = PostRaceEvents[i][j].Subjects.Count > 0 ? string.Join("_", PostRaceEvents[i][j].Subjects.ToArray()) : null;
					manager.UpdateSingleBelief($"PRESubject{i}({j})", subjects);
				}
			}
			manager.SaveStatus();
		}

		public string GetNotes(string subject)
		{
			return iat.GetDialogueActionsByState("Player_Note").FirstOrDefault(s => s.NextState == subject.NoSpaces())?.Utterance ?? string.Empty;
		}

		public void SaveNote(string subject, string note)
		{
			var newNote = new DialogueStateActionDTO
			{
				CurrentState = "Player_Note",
				NextState = subject.NoSpaces(),
				Meaning = Name.NIL_STRING,
				Style = Name.NIL_STRING,
				Utterance = note
			};
			var savedNote = iat.GetDialogueActionsByState("Player_Note").FirstOrDefault(s => s.NextState == subject.NoSpaces());
			if (savedNote == null)
			{
				iat.AddDialogAction(newNote);
			}
			else
			{
				iat.EditDialogAction(savedNote, newNote);
			}
			iat.Save();
		}
	}
}
