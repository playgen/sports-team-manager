# Key Game Mechanics
Below are details of the key game mechanics that are present in Sports Team Manager, how they work and how to make changes

## Saving and Loading
Game saves are saved to the [PersistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) which can be found at Users\[Name]\AppData\LocalLow\PlayGen\Sports Team Manager_ Sailing Edition\GameSaves. 

Each save file must have the following data to be able to continue from the right point:
- IntegratedAuthoringToolAsset (.iat) data for the current game, containing all config for relationship change between characters and managers, and management style. Provided by [Integrated Authoring Tool](https://www.gamecomponents.eu/content/201)
- RolePlayCharacter (.rpc) data for each member of the crew, to save relationships between crew and their skills across sessions
- The manager RolePlayCharacter data, which contains information on the following; BoatType, TutorialProgress, QuestionnaireCompletedStatus, Nationality, ActionAllowance, CrewEditAllowance, TeamColorPrimary, TeamColorSecondary. 

## State Manager
The [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs) contains reference to all the states that are present in the game, and contains explicit calls to move between states. In order to create a new state, and follow the current design, the UIStateManager must be edited to include the new state and its transitions specified using the GoToState() format. Then the menu should be added in the inspector. 

## Position Configurations
Sports Team Manager contains 4 different position configurations, increasing in required crew members. An example of this can be seen in the[BoatConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/BoatConfig.json), the required skills for each position are then defined in [Position.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Position.cs). The order of team positions to fill are then defined in the [GameConfig](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json), with each PromotionTrigger ScoreMetSinceLast used to evaluate whether the player has earned the promotion.

## Team Management
In order to manage effectively, players must understand the dynamics between crew members, their skillsets and their opinion of the manager that will affect their performance.

### Character Skills
There are 6 skills which characters can have in the game: Charisma, Perception, Quickness, Strength, Willpower and Wisdom. When a character is created, they are given random values for each of the skills, which falls between the *RandomSkillHigh* and *RandomSkillLow* in the [config](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json). When a player is lacking a key crew member for a certain position, the new characters will be better off for the required skill for that position, but their other skills will not be worse. 

### Character Generation
After new characters have been generated, they are given an avatar based on their skill set (Stronger characters are more bulky than quick characters). The [Avatar Generation](stm-unity\Assets\Scripts\AvatarDisplay.cs) system handles the creation of the character UI and contains both UI for icons and full body view.

### Character Dialogue
During the game, there are 3 times that you can interact with your crew;
- During Recruitment
- During Conversations to find their skills and relationships
- After a race

The dialogue is defined in the [SportsTeamManagerScenario](stm-unity/Assets/Editor/Localization/SportsTeamManagerScenarioLocalization.xlsx) spreadsheet, with each key defined in the Integrated Authoring Tool with its corresponding state. Upon entering a state where there is character dialogue, the keys are checked based on characters opinions of each other and the manager, this influences the responses that are available to the player and how each of the characters speak.

### Enforced Team Restrictions
The following restrictions are currently enforced on players

Restriction | Variable   | Location
--- | --- | ---
Max team size | (Boat.Positions.Count+1) * 2 | [Team.cs](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Team.cs) 
Talk Time Allowance | TicksPerSession | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Race Sessions | RaceSessionLength | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Season Length | Sum(PromotionTriggers.ScoreMetSinceLast) | [GameConfig.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameConfig.json) 
Time needed to Recruit | RecruitmentCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 
Time needed to fire | FiringCost | [Config.json](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/config.json) 

### Post-Race Standings
The final position for each team after the race is determined by the how well players have chosen the sum of crew ratings in each position and whether it surpasses the threshold for reaching 1st, (8 * position count) + 1, this can be found in GetCupPosition() in [GameManagement.cs](stm-unity/Assets/Scripts/GameManagement.cs).

### Session Feedback
After each session, players are shown feedback for which of their crew were in their best position and which skills were lacking in order to have an effective team.

#### Crew Placements
The crew placement is checked if it is the best available from the available crew members that are currently hired. A correct placement is one which is nearest to the ideal match for the team. Yellow ratings are given for crew members who are close to the ideal score, and red ratings are for crew members who are not in the best position. 
- [Calculating Position Ratings](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\Boat.cs) - FindAssignmentMistakes()
- [Showing Position Ratings](stm-unity\Assets\Scripts\TeamSelectionUI.cs) - SetMistakeIcons()

#### Crew Skills
The crew skill icon shows the skills which were lacking, this uses the same list of mistake placements as mentioned above, but provides more feedback to guide the player to help them identify which of the crew members are not ideal.

### Tutorial States
The tutorial is skippable from the new game screen, it contains a guided step through of the game, providing instructions and tips for how to be a better manager. After the tutorial, players are launched straight into the game with their tutorial progress carrying over. 

Each step in the tutorial are defined by the following variables

Variable | Type | Description
--- | --- | ---
SectionName | string | Section Name is only used in inspector to identify each step easily
SectionTextHolder | List\<LanguageKeyValuePair> | Text to be displayed to the user, contains text for all lanuages
ShowOnLeft | bool | If the popup should be shown on the left side of the screen
HighlightedObject | List\<string> | Path to the object that will be highlighted to guide the player (from Canvas level)
Triggers | List\<TriggerKeyValuePair> | The events that must happen for the player to complete this step of the tutorial
UniqueEvents | bool | Whether the events that happen before must be unique and in the correct order
EventTriggerCountRequired | int | The number of times players must complete activate the trigger event to continue to the next step
SafeToSave | bool | If the current step in the tutorial can be saved, sometimes UI must be open for the current step to activate/finish properly
BlacklistButtons | List\<StringList> | Buttons which cannot be interacted with at this stage in the tutorial
CustomAttributes | List\<string> | Additional attributes to help set up the stage in the tutorial

The current tutorial steps are defined in the [SportsTeamManagerTutorial](stm-unity/Assets/Resources/Tutorial/SportsTeamManagerTutorial.xlsxExcel File), which is converted to JSON and serialized   to a list of TutorialObjects using the [Context Menu](https://unity3d.com/sites/default/files/styles/original/public/learn/MenuItems06.png?itok=NZTNMINK) for TutorialController. 

### Inter-team and Manager Relationships
Characters opinions of each other and the manager can change throughout the game based on interactions through conversationa and being picked for the team. Each opinion is saved as an NPC belief.

The following events will cause opinions of crew members to change
Event | Impact
--- | ---
Crew Member Not Picked | -3
Not In Expected Position | -3
In Expected Position | +1
Crew Promised Position In Team | +1
Reveal 2 Skills | +1
Reveal 4 Skills | +3
Improve Conflicts Greatly | +2 
Improve Conflict Opinion | +1
Learn About Conflict | +1
Promise Selection in next team | +1 
Change Whole Team | +4
Placed With Someone They Dont Like | -10
Team With Conflicting Members | -3..0

There are also a number of generic opinion changes that can occur based on manager choices, these values range from -5 to +5. Full breakdown can be seen in [CrewMember.cs](stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation\CrewMember.cs)

Brief overview of the way in which crew memebers relationships with other crew and the manager affect their performance and how this is calculated, link to FAtiMA

## Post-Season Questionnaire
Once all races are completed in the season, players are forwarded to the post-season questionnaire. The questions are defined in the [Sports Team Manager Questionnaire](stm-unity/Assets/Resources/Questionnaire/SportsTeamManagerQuestionnaireLocalization.xlsx) spreadsheet. This is compiled to JSON and read at runtime. The spreadsheet contains 2 sheets, Questions and Answer Styles, the answer styles map directly to the question choices to calculate the players management style.

Questions are read from file and populated in game in [QuestionnaireUI.cs](stm-unity/Assets/Scripts/QuestionnaireUI.cs). Upon completion of the questionnaire, the count for each style type are saved and the prominent management style is calculated

Upon completing the questionnaire, players are presented with information and an overview of their calculated management style for reflection.