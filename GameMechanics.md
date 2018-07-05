# Key Game Mechanics
Below are details of the key game mechanics that are present in Sports Team Manager, how they work and how to make changes

## Saving and Loading
Game saves are saved to the [PersistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) which can be found at Users\[Name]\AppData\LocalLow\PlayGen\Sports Team Manager_ Sailing Edition\GameSaves. 

Each save file must have the following data to be able to continue from the right point:
- IntegratedAuthoringToolAsset (.iat) data for the current game, which contains the file path for all of the characters and all of the dialogue for the player and the NPCs.
- RolePlayCharacterAsset (.rpc) data for each member of the crew, which stores all of their 'beliefs', such as skills, opinions, age and appearence, and knowledge of events involving them.
- The manager RolePlayCharacterAsset data, which contains information relating to the team in its 'beliefs' and stores a record of all session results.

## State Manager
The [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs) contains reference to all the states that are present in the game, and contains explicit calls to move between states. In order to create a new state, and follow the current design, the UIStateManager must be edited to include the new state and its transitions specified using the GoToState() and StaticGoToState() format. Then the menu should be added in the inspector. 

## Position Configurations
Sports Team Manager contains 4 different position configurations with differing number of positions. All available configurations are added to the [BoatConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/BoatConfig.json).

At the moment, the game does not support having two of the same position in the same configuration. The skills required for each position is defined in an attribute for each value in the position in the [Position](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Position.cs) enum.

When a configuration should be used is defined in the [GameConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json), with each PromotionTrigger ScoreMetSinceLast used to evaluate whether the player has earned the promotion. There is also a ScoreRequired requirement if the player has to perform to a certain standard to be promoted, but at the moment that value is not set and as such is set to 0.

## Team Management
In order to manage effectively, players must understand the dynamics between crew members, their skillsets and their opinion of the manager and how that will affect their performance.

### Character Skills
There are six skills which characters can have in the game: Charisma, Perception, Quickness, Strength, Willpower and Wisdom. When a character is created, they are given random values for each of the skills, which falls between the *RandomSkillHigh* and *RandomSkillLow* in the [config](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json). When a player is lacking a key crew member for a certain position, the new characters will be better off for the required skills for that position, but their other skills will also be worse. 

### Character Generation
After new characters have been generated, they are given an avatar based on their skill set (Stronger characters are more bulky than quick characters). The [Avatar Generation](stm-unity\Assets\Scripts\AvatarDisplay.cs) system handles the creation of the character UI and contains both UI for icons and full body view.

### Character Dialogue
During the game, there are 3 times that you can interact with your crew:
- During recruitment
- During conversations to find their skills and relationships
- In a post-race conflict event

The dialogue text for Localization is provided within the 'Utterance' for each piece of dialogue, with the text itself defined in the [SportsTeamManagerScenario](stm-unity/Assets/Editor/Localization/SportsTeamManagerScenarioLocalization.xlsx) spreadsheet. The greeting given by the NPC at the beginning of a conversation is determined using the [Social Importance Dynamics Asset](https://www.gamecomponents.eu/content/207), with different dialogue provided if the NPC has a high or low opinion of the manager.

### Enforced Team Restrictions
The following restrictions are currently enforced on players:

Restriction | Variable   | Location
--- | --- | ---
Max team size | (Boat.Positions.Count + 1) * 2 | [Team.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Team.cs) 
Talk time allowance | DefaultActionAllowance + (ActionAllowancePerPosition * Boat.Positions.Count) | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) [GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Crew edit allowance | CrewEditAllowancePerPosition * Boat.Positions.Count | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) [GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Race sessions | isTutorial ? TutorialRaceSessionLength : RaceSessionLength | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) [GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Season length | Sum(PromotionTriggers.ScoreMetSinceLast) | [GameConfig.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json)
Time to reveal skill value | ((int)(RevealedSkills.Count(!= 0) * StatRevealCost)) + (RevealedSkills.All(!= 0) ? 0 : 1) | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) [GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal role rating | RoleRevealCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal positive opinion | OpinionRevealPositiveCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to reveal negative opinion | OpinionRevealNegativeCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time to ask recruitment question | SendRecruitmentQuestionCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time needed to recruit | RecruitmentCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time needed to fire | FiringCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Races until crew members can race again | PostRaceRest | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Ticks per session | if (CurrentRaceSession == 0) Team.TickCrewMembers(TicksPerSession, false); | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) [GameManager.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Recruit count | RecruitCount | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)
Chance of recruits being replaced | RecruitChangeChance | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json)

### Post-Race Standings
The final position for each team after the race is determined by the how well players have chosen the sum of crew ratings in each position and whether it surpasses the threshold for reaching 1st, (8 * position count) + 1, this can be found in GetCupPosition() in [GameManagement.cs](stm-unity/Assets/Scripts/GameManagement.cs).

### Session Feedback
After each session, players are shown feedback for which of their crew were in their best position and which skills were lacking in order to have an effective team.

- [Calculating Ideal Line-ups](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\Boat.cs) - GetIdealCrew()

#### Crew Placements
The crew placement is checked to see if it is one of the best available from the available crew members, with feedback given based on the closest match. Green ratings (1) are given when the ideal crew member is in a position. Yellow ratings (0.1) are given when a crew member should be selected, but not in their current position. Red (0) ratings are given for crew members who should not be selected at all in the line-up.

As ideal ratings are stored as a float using the numbers above for each type of rating. Note that this system will no longer work if the number of ratings goes above 9, as 10 'yellow' ratings will result in one 'green' rating instead.
- [Calculating Position Ratings](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\Boat.cs) - UpdateIdealScore()
- [Showing Position Ratings](stm-unity\Assets\Scripts\TeamSelectionUI.cs) - SetMistakeIcons()

#### Crew Skills
The crew skill icon shows the areas of the line-up which can be the most improved. This is calculated by comparing the current line-up with the closest ideal line-up in each area. In cases where an amount of the information (set with the HiddenMistakeLimit and HiddenOpinionLimit values in the config) is currently unknown to the player, this area is recorded as 'Hidden' instead of the actual area.

- [Calculating istakes Made](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\Boat.cs) - FindAssignmentMistakes()
- [Showing Mistake Icons](stm-unity\Assets\Scripts\TeamSelectionUI.cs) - SetMistakeIcons()

### Tutorial States
Outside of demo mode, the tutorial is skippable from the new game screen after the first game on that machine has been started. In demo mode, the tutorial is not available as an option.

The tutorial contains a guided step through of the game, providing instructions and tips for how to be a better manager. After the tutorial, players can choose to continue from the end of the tutorial or restart from the beginning with a new crew.

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

The current tutorial steps are defined in the [SportsTeamManagerTutorial File](stm-unity/Assets/Resources/Tutorial/SportsTeamManagerTutorial.xlsxExcel), which is converted to JSON using ExcelToJSON and serialized to a list of TutorialObjects using the [Context Menu](https://unity3d.com/sites/default/files/styles/original/public/learn/MenuItems06.png?itok=NZTNMINK) for TutorialController. 

### Inter-team and Manager Relationships
Characters opinions of each other and the manager can change throughout the game based on interactions through conversations and promises made in those conversations later being broken. Each opinion is saved as an NPC belief.

The effects of a dialogue option are set as the 'Style', with multiple effects split using an underscore separator. A full list of potential dialogue events can be found in the [PostRaceEventImpact.cs](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\PostRaceEventImpact.cs) enum, with the functionality implemented in the *PostRaceFeedback* method in [CrewMember.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/CrewMember.cs).

The following events can occur and are processed in the *PostRaceFeedback* method:

EventName | Description | Impact
--- | --- | ---
ExpectedPosition | Crew Member Told They Will Be Picked Next Race | +1 to Crew Member's Opinion of Manager, Adds ExpectedPosition Belief
ExpectedPositionAfter | Crew Promised Told They Will Be Picked In Race After Next Race | +1 to Crew Member's Opinion of Manager, Adds ExpectedPositionAfter Belief
ManagerOpinionWorse | Generic event | -1 to Crew Member's Opinion of Manager
ManagerOpinionAllCrewWorse | Generic event | -2 to All Crew Member's Opinion of Manager
ManagerOpinionBetter | Generic event | +1 to Crew Member's Opinion of Manager
ManagerOpinionAllCrewBetter | Generic event | +2 to All Crew Member's Opinion of Manager
ManagerOpinionMuchWorse | Generic event | -5 to Crew Member's Opinion of Manager
ManagerOpinionMuchBetter | Generic event | +5 to Crew Member's Opinion of Manager
RevealTwoSkills | Spoke to Crew Member About Skills | +1 to Crew Member's Opinion of Manager, two skill values revealed
RevealFourSkills | Spoke to Crew Member About Skills | +3 to Crew Member's Opinion of Manager, four skill values revealed
ImproveConflictOpinionGreatly | Conflict Between Crew Members Handled Well | +2 to All Crew Member's Opinion of the Focus of Conflict
ImproveConflictOpinionTeamOpinion | Conflict Between Crew Members Handled OK | +1 to All Crew Member's Opinion of the Focus of Conflict
ImproveConflictKnowledge | Conflict Between Crew Members Not Resolved But More is Now Known | +1 to Crew Member's Opinion of Manager, All Crew Member's Opinion of the Focus of Conflict Now Known
CausesSelectionAfter | Crew Member Will Swap with Another Crew Member Over Next Two Races | +1 to Both Crew Member's Opinion of Manager, Adds ExpectedPosition and ExpectedPositionAfter Beliefs
WholeTeamChange | Promise Made to Try Out Completely Different Line-up | +4 to Crew Member's Opinion of Manager, +1 to Every Unplaced Crew Member's Opinion of Member, Adds ExpectedSelection Belief to Every Unplaced Crew Member

The MoodChange event should always come with a value in brackets ranging from -7 to 7 and is handled via the *RolePlayCharacter.Perceive()* at the beginning of *PostRaceFeedback*, which results in additional emotions being 'felt' by the crew member and their mood changing as a result. The values assigned to each level of MoodChange can be found in the [Template Emotional Appraisal file](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/NPC Templates/Template.ea).

As manager you can make promises to players in conflict events that they will be picked in the future. Not following through on your promise has the following events:

EventName | Description | Impact
--- | --- | ---
ExpectedSelection | Broke promise made earlier than they would be selected | -3 to manager opinion
ExpectedPosition | Broke promise made earlier than they would be placed in a particular position | -3 to manager opinion

The triggering of events also results in some forced changes:
EventName | Description | Impact
--- | --- | ---
Involved in "Placed With Someone They Don't Like" event | -10
Event Triggered With Members Conflicting | -3..0

In addition to this, 

## Post-Season Questionnaire
Once all races are completed in the season, players are forwarded to the post-season questionnaire. The questions are defined in the [Sports Team Manager Questionnaire](stm-unity/Assets/Resources/Questionnaire/SportsTeamManagerQuestionnaireLocalization.xlsx) spreadsheet. This is compiled to JSON and read at runtime. The spreadsheet contains 2 sheets, Questions and Answer Styles, the answer styles map directly to the question choices to calculate the players management style.

Questions are read from file and populated in game in [QuestionnaireUI.cs](stm-unity/Assets/Scripts/QuestionnaireUI.cs). Upon completion of the questionnaire, the count for each style type are saved and the prominent management style is calculated

Upon completing the questionnaire, players are presented with information and an overview of their calculated management style for reflection.