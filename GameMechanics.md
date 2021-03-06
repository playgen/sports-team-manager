# Key Game Mechanics
Below are details of the key game mechanics that are present in Sports Team Manager, how they work and how to make changes.

## Saving and Loading
Game saves are saved to the [PersistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) which can be found at Users\\[Name]\AppData\LocalLow\PlayGen\Sports Team Manager_ Sailing Edition\GameSaves. 

Each save file must have the following data to be able to continue from the right point:
- IntegratedAuthoringToolAsset (.iat) data for the current game, which contains the file path for all of the characters and all of the dialogue for the player and the NPCs.
- RolePlayCharacterAsset (.rpc) data for each member of the crew, which stores all of their 'beliefs', such as skills, opinions, age and appearence, and knowledge of events involving them.
- The manager RolePlayCharacterAsset data, which contains information relating to the team 'beliefs' and stores a record of all session results.

## State Manager
The [State](stm-unity/Assets/Scripts/State.cs) enum is used to reference all of the UI states within the game. The [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs), a component on the 'Canvas' GameObject, contains reference to all the states and their relevant GameObjects. In order to add a new state, a new value will need to be added to the State enum, with an item relating to this also added to the [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs) States list with a reference to the GameObject related to this state. 

In order to change state, use the following method call:

```c#
UIManagement.StateManager.GoToState(State state)
```

## Position Configurations
Sports Team Manager contains four different position configurations with differing number of positions. All available configurations are added to the [BoatConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/BoatConfig.json).

At the moment, the game does not support having two of the same position in the same configuration. The skills required for each position are defined in an attribute for each value in the [Position](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Position.cs) enum.

When a configuration should be used is defined in the [GameConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json) using [BoatPromotionTriggers](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/BoatPromotionTriggers.cs), with ScoreMetSinceLast used to evaluate whether the player has earned the promotion. There is also a ScoreRequired requirement if the player has to perform to a certain standard to be promoted, but at the moment that value is not set and as such is set to 0.

## Team Management
In order to manage effectively, players must understand the dynamics between crew members, their skillsets and their opinion of the manager and how that will affect their performance.

### Character Skills
There are six skills characters have in the game: Charisma, Perception, Quickness, Strength, Willpower and Wisdom. When a character is created, they are given random values for each of the skills, with possible values defined using the *RandomSkillHigh* and *RandomSkillLow* values set in the [config](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json). When a team is lower in potential good choices for a position, new characters will be created to be better off for the required skills for that position, but their other skills will also be worse. 

### Character Generation
After new characters have been generated, they are given an avatar based on their skill set (for example, stronger characters are designed to be more bulky than quick characters). The [Avatar Generation](stm-unity/Assets/Scripts/AvatarDisplay.cs) system handles the creation of the character UI for both icons and full body view.

### Character Dialogue
During the game, there are 3 times that you can interact with your crew:
- During recruitment
- During conversations to find their skills and relationships
- In a post-race conflict event

The dialogue text for Localization is provided within the 'Utterance' for each piece of dialogue, with the text itself defined in the [SportsTeamManagerScenario](stm-unity/Assets/Editor/Localization/SportsTeamManagerScenarioLocalization.xlsx) spreadsheet. 

Dialogue is purely determined by the situation it is being used in and where it is within the conversation chain. A crew member's opinion of the manager or emotional state has no impact on how they will reply. The one exception to this is the greeting given by the NPC at the beginning of a conversation, which is determined using the [Social Importance Dynamics Asset](https://www.gamecomponents.eu/content/207), with different dialogue provided if the NPC has a high or low opinion of the manager.

### Set Configuration Values
The following values are set within code or config files and as such determine how the game plays:

Restriction | Variable   | Location
--- | --- | ---
Max team size | (Boat.Positions.Count + 1) * 2 | [Team.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Team.cs) 
Talk time allowance | DefaultActionAllowance + (ActionAllowancePerPosition * Boat.Positions.Count) | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)</br>[GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Crew edit allowance | CrewEditAllowancePerPosition * Boat.Positions.Count | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)</br>[GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Race sessions | isTutorial ? TutorialRaceSessionLength : RaceSessionLength | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)</br>[GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Season length | Sum(PromotionTriggers.ScoreMetSinceLast) | [GameConfig.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json)
Time to reveal skill value | ((int)(RevealedSkills.Count(!= 0) * StatRevealCost)) + (RevealedSkills.All(!= 0) ? 0 : 1) | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)</br>[GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal role rating | RoleRevealCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal positive opinion | OpinionRevealPositiveCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal negative opinion | OpinionRevealNegativeCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to ask recruitment question | SendRecruitmentQuestionCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time needed to recruit | RecruitmentCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time needed to fire | FiringCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Races until crew members can race again | PostRaceRest | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Ticks per session | if (CurrentRaceSession == 0) Team.TickCrewMembers(TicksPerSession, false) | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)</br>[GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Recruit count | RecruitCount | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Chance of recruits being replaced | RecruitChangeChance | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)

### Scoring and Post-Race Standings
Once a line-up has been selected and submitted, a score is calculated for each crew member selected, with all of these scores combined to make a team score, for sailing edition, this is referred to as a 'boat score'. A crew member's score is calculated by adding together their average rating for all the skills required for the position, their current mood value, their average opinion of the other crew members selected and their opinion of the manager. All of these values also have a weighting set in the [Config](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) that they are multipled by before they are combined into one score. 

- [Calculate Crew Member Score](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Boat.cs) - UpdateCrewMemberScore()
- [Calculate Crew Member Position Score](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Position.cs) - GetPositionRating()
- [Get Crew Member Mood](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/CrewMember.cs) - GetMood()

The time shown in practice sessions is calculated by: 1800 - (score x 10). A random 'offset' value is then added, in order to create slight variations in times shown. This value is then converted into minutes and seconds.

- [Calculate Practice Time](stm-unity/Assets/Scripts/TeamSelectionUI.cs) - GetResult()

The score required to get first place in a race is calculated as (8 * number of boat positions) + 1. If the boat at least matched that value, then they finished in first place. Otherwise, the score is reduced by the number of boat positions in order to check if the boat finished in second. This process is repeated until the 9th failure, at which point the boat will be classed as finishing in 10th place. 

- [Calculate Race Position](stm-unity/Assets/Scripts/GameManagement.cs) - GetRacePosition()
- [Calculate Expected Score](stm-unity/Assets/Scripts/GameManagement.cs) - GetExpectedScore()

### Session Feedback
After each session, players are shown feedback for which of their crew were positioned correctly and which skills were lacking in order to have an effective team. In order to do this, every combination is reviewed after every race in order to work out what the most effective line-up would have been.

- [Calculating Ideal Line-ups](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Boat.cs) - GetIdealCrew()

#### Crew Placements
The current line-up is checked to see how closely it matches with all possible 'ideal' line-ups, with the final rating being the highest. Green ratings (1) are given when the ideal crew member is in a position. Yellow ratings (0.1) are given when a crew member should be selected, but not in their current position. Red (0) ratings are given for crew members who should not be selected at all in the line-up.

Ideal ratings are stored as a float using the numbers above for each type of rating. Note that this system will no longer work if the number of positions goes above 9, as 10 'yellow' ratings will result in one 'green' rating instead.
- [Calculating Position Ratings](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Boat.cs) - UpdateIdealScore()
- [Showing Position Ratings](stm-unity/Assets/Scripts/TeamSelectionUI.cs) - SetMistakeIcons()

#### Crew Skills
The crew skill icon shows the areas of the line-up which can be the most improved. This is calculated by comparing the current line-up with the closest ideal line-up in each area. In cases where an amount of the information (set with the HiddenMistakeLimit and HiddenOpinionLimit values in the config) is currently unknown to the player, this area is recorded as 'Hidden' instead of the actual area.

- [Calculating istakes Made](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Boat.cs) - FindAssignmentMistakes()
- [Showing Mistake Icons](stm-unity/Assets/Scripts/TeamSelectionUI.cs) - SetMistakeIcons()

### Tutorial States
After the first game on that machine has been started, the tutorial is skippable from the new game screen. In demo mode, the tutorial is not available as an option.

The tutorial contains a guided step through of the game, providing instructions and tips for how to play the game. After the tutorial, players can choose to continue from the end of the tutorial or restart from the beginning with a new crew.

Each step in the tutorial is defined using the following variables:

Variable | Type | Description
--- | --- | ---
SectionName | string | Section Name is only used in inspector to identify each step easily.
SectionTextHolder | List\<LanguageKeyValuePair> | Text to be displayed to the user, contains text for multiple languages.
ShowOnLeft | bool | If the pop-up should be shown on the left side of the screen.
HighlightedObject | List\<string> | Path to the object that will be highlighted to guide the player (from Canvas level).
Triggers | List\<TriggerKeyValuePair> | The events that must happen for the player to complete this step of the tutorial.
UniqueEvents | bool | Whether the events that happen before must be unique.
EventTriggerCountRequired | int | The number of times players must complete the trigger event to continue to the next step.
SafeToSave | bool | If the current step in the tutorial can be saved, as sometimes UI must be open for the current step to activate/finish properly.
BlacklistButtons | List\<StringList> | Buttons which cannot be interacted with at this stage in the tutorial.
CustomAttributes | List\<string> | Additional attributes to help set up the stage in the tutorial.

The current tutorial steps are defined in the [SportsTeamManagerTutorial File](stm-unity/Assets/Resources/Tutorial/SportsTeamManagerTutorial.xlsx), which is converted to JSON using ExcelToJSON and serialized to a list of TutorialObjects using the [Context Menu](https://unity3d.com/sites/default/files/styles/original/public/learn/MenuItems06.png?itok=NZTNMINK) for TutorialController. 

### Inter-team and Manager Relationships
Characters mood and opinions of each other and the manager can change throughout the game based on interactions through conversations and promises made in those conversations later being broken. Each opinion is saved as an NPC belief.

The effects of a dialogue option are set as the 'Style', with multiple effects split using an underscore separator. A full list of potential dialogue events can be found in the [PostRaceEventImpact.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/PostRaceEventImpact.cs) enum, with the functionality implemented in the *PostRaceFeedback* method in [CrewMember.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/CrewMember.cs).

The following events can occur and are processed in the *PostRaceFeedback* method:

EventName | Description | Impact
--- | --- | ---
ManagerOpinionMuchBetter | Generic event | +5 to Crew Member's Opinion of Manager
ManagerOpinionBetter | Generic event | +1 to Crew Member's Opinion of Manager
ManagerOpinionAllCrewBetter | Generic event | +2 to All Crew Member's Opinion of Manager
ManagerOpinionMuchWorse | Generic event | -5 to Crew Member's Opinion of Manager
ManagerOpinionWorse | Generic event | -1 to Crew Member's Opinion of Manager
ManagerOpinionAllCrewWorse | Generic event | -2 to All Crew Member's Opinion of Manager
ExpectedPosition | Crew Member Told They Will Be Picked Next Race | +1 to Crew Member's Opinion of Manager</br>Adds ExpectedPosition Belief
ExpectedPositionAfter | Crew Promised Told They Will Be Picked In Race After Next Race | +1 to Crew Member's Opinion of Manager</br>Adds ExpectedPositionAfter Belief
CausesSelectionAfter | Crew Member Will Swap with Another Crew Member Over Next Two Races | +1 to Both Crew Member's Opinion of Manager</br>Adds ExpectedPosition and ExpectedPositionAfter Beliefs
WholeTeamChange | Promise Made to Try Out Completely Different Line-up | +4 to Crew Member's Opinion of Manager</br>+1 to Every Unplaced Crew Member's Opinion of Member</br>Adds ExpectedSelection Belief to Every Unplaced Crew Member
RevealTwoSkills | Spoke to Crew Member About Skills | +1 to Crew Member's Opinion of Manager</br>Two Skill Values Revealed
RevealFourSkills | Spoke to Crew Member About Skills | +3 to Crew Member's Opinion of Manager</br>Four Skill Values Revealed
ImproveConflictOpinionGreatly | Conflict Between Crew Members Handled Well | +2 to All Crew Member's Opinion of the Focus of Conflict
ImproveConflictOpinionTeamOpinion | Conflict Between Crew Members Handled OK | +1 to All Crew Member's Opinion of the Focus of Conflict
ImproveConflictKnowledge | Conflict Between Crew Members Not Resolved But More is Now Known | +1 to Crew Member's Opinion of Manager</br>All Crew Member's Opinion of the Focus of Conflict Now Known

The MoodChange event should always come with a value in brackets ranging from -7 to 7 and is handled via the *RolePlayCharacter.Perceive()* at the beginning of *PostRaceFeedback*, which updates the character's emotional state and mood based on the value provided. The Desirability and Praiseworthiness values assigned to each level of MoodChange, which affect how they change the emotinal change of the character, can be found in the [Template Emotional Appraisal file](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/NPC%20Templates/Template.ea).

As manager you can make promises to players in conflict events that they will be picked in the future. Not following through on your promise has the following events:

EventName | Description | Impact
--- | --- | ---
ExpectedSelection | Broke promise made earlier than they would be selected | -3 to manager opinion
ExpectedPosition | Broke promise made earlier than they would be placed in a particular position | -3 to manager opinion

The triggering of events also results in some forced changes that plays into the narrative of that event:
EventName | Description | Impact
--- | --- | ---
OO | Two crew members in the line-up aren't getting along | -10 to 'selectedFor' opinion of 'selectedAgainst'</br>Between 0 and -3 change in every other crew member's opinion of 'selectedAgainst'

## Post-Season Questionnaire
Once all races are completed in the season, if the game is in 'RAGE' mode the player is forwarded to the post-season questionnaire. The questions are defined in the [Sports Team Manager Questionnaire](stm-unity/Assets/Resources/Questionnaire/SportsTeamManagerQuestionnaireLocalization.xlsx) spreadsheet. This is compiled to JSON and read at runtime. The spreadsheet contains 2 sheets; Questions, which provides localized text for each question, and Answer Styles, which maps the question choices to management styles in order to calculate the players management style.

Questions are read from file and populated in game in [QuestionnaireUI.cs](stm-unity/Assets/Scripts/QuestionnaireUI.cs). Upon completion of the questionnaire, the count for each style type is saved and the prominent management style calculated.

Upon completing the questionnaire, players are presented with an overview of all the different management styles and which ones they predominantly used.