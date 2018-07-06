# Developer Guide
Below are more details about getting started when making changes in Sports Team Manager.

# Unity Game
## Key Scene Structure
- **Canvas**: Main gameplay canvas. Contains [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs), which manages the UI state, and BestFit from the Text Unity Utility, which helps ensure text sizing remains consistent.
  - **Main Menu**: Main hub interface to access starting a new game, loading saved games, signing into SUGAR and changing language settings.
  - **New Game**: Interface for creating a New Game. Contains [NewGameUI](stm-unity/Assets/Scripts/NewGameUI.cs), which manages this interface, and FormKeyboardControls, which allows for tabbing between input fields.
  - **Load Game**: Interface for loading a previously started game. Managed by [LoadGameUI](stm-unity/Assets/Scripts/LoadGameUI.cs).
  - **Team Management**: Majority of the gameplay interface is contained within this GameObject. Contains [TeamSelectionUI](stm-unity/Assets/Scripts/TeamSelectionUI.cs), which among other things manages displaying historical line-ups and submitting line-ups, and [EscapeAction](stm-unity/Assets/Scripts/EscapeAction.cs), which controls what logic should be performed when the Escape/Back key is pressed depending on what UI is currently active.
    - **Top Details**: Interface displayed at the top section of the screen, including Team Name and progress through the season. Managed by [ScreenSideUI](stm-unity/Assets/Scripts/ScreenSideUI.cs).
    - **Boat Container Bounds**
      - **Viewport**
        - **Content**: Contains 'Historic Boat' prefabs, which are used to display results from previous races, a 'Boat' prefab, which is used to display and adjust the line-up for the current race/practice session, and a 'Final' object which displays season results and buttons for progressing to the post-game questionnaire or feedback when in RAGE mode.
    - **Crew Container**
      - **Crew Sort**: Dropdown used to select the order crew members should be listed in.
      - **Crew Container**
        - **Viewport**
          - **Crew Container**: Instantiated [CrewMemberUI](stm-unity/Assets/Scripts/CrewMemberUI.cs) objects, used to list current crew members and to place crew members into boat positions, are children of this object.
    - **Pop-up Bounds**
      - **Position Pop-Up**: Interface which displays information on a position, including a short description, the skills required, the crew member currently in that position (if any) and a history of who has been placed in the position in the past sorted by number of appearences. Managed by [PositionDisplayUI](stm-unity/Assets/Scripts/PositionDisplayUI.cs).
      - **Meeting Pop-Up**: Interface which contains the functionality to ask questions to a crew member and dismiss a crew member from the team. Also displays information on a crew member, including their skills, their opinion of you as a manager (if known) and their current position (if any). The crew member's opinion of other crew (if known) is displayed on their [CrewMemberUI](stm-unity/Assets/Scripts/CrewMemberUI.cs) objects when this pop-up is active. Managed by [MemberMeetingUI](stm-unity/Assets/Scripts/MemberMeetingUI.cs).
      - **Recruit Pop-Up**: Interface which allows the player to ask questions to potential crew members in order to discover their strengths and weaknesses and to hire crew members into the team. Managed by [RecruitMemberUI](stm-unity/Assets/Scripts/RecruitMemberUI.cs).
      - **Event Pop-Up**: Contains two interfaces which are used to select and receive dialogue when in a post-race event; one for 'solo' events, which involve the player and one crew member, and one for 'pair' events, which involve the player and two crew members. Each interface is managed by [PostRaceEventUI](stm-unity/Assets/Scripts/PostRaceEventUI.cs), with each NPC UI within the event interface managed by [PostRacePersonUI](stm-unity/Assets/Scripts/PostRacePersonUI.cs).
      - **Event Impact Pop-Up**: Interface that lists the effects the post-race event has just had, such as a crew member's opinion of you as a manager changing or a crew member expecting to be selected for the next race. Managed by [PostRaceEventImpactUI](stm-unity/Assets/Scripts/PostRaceEventImpactUI.cs).
      - **Fire Warning Pop-Up**: Interface that is displayed when the 'Dismiss' button is selected on the *Meeting Pop-Up*, allowing players to confirm their decision. Managed by [MemberMeetingUI](stm-unity/Assets/Scripts/MemberMeetingUI.cs).
      - **Hire Warning Pop-Up**: Interface that is displayed when a crew member is selected on the *Recruit Pop-Up*, allowing players to confirm their decision. Managed by [RecruitMemberUI](stm-unity/Assets/Scripts/RecruitMemberUI.cs).
      - **Pre-Race Pop-Up**: Interface that is displayed when the player selects the 'Race' button if their line-up is the same as the previous line-up or if it is a race and they have not used their full allocation of talk time. Managed by [PreRaceConfirmUI](stm-unity/Assets/Scripts/PreRaceConfirmUI.cs).
      - **Post-Race Pop-Up**: Interface that is displayed after a race has taken place, showing the team's final position in the race and all of the crew members who took part reacting to the result. Managed by [RaceResultUI](stm-unity/Assets/Scripts/RaceResultUI.cs).
      - **End Result Pop-Up**: Interface that is displayed after all of the races in a season, showing the team's final position in the season standings and all of the crew members reacting to the result. Managed by [CupResultUI](stm-unity/Assets/Scripts/CupResultUI.cs).
      - **Promotion Pop-Up**: Interface that displays all of the positions in the line-up that have been added and removed as a result of a change in boat type. Managed by [BoatPromotionUI](stm-unity/Assets/Scripts/BoatPromotionUI.cs).
      - **Notes Pop-Up**: Interface which allows players to make notes on a crew member or position. Managed by [NotesUI](stm-unity/Assets/Scripts/NotesUI.cs).
      - **Blocker**: Used to block background UI for some pop-ups. Active state and ordering in heirarchy to ensure blocking is controlled by [UIManagement](stm-unity/Assets/Scripts/UIManagement.cs).
      - **Bigger Blocker**: Used to block background UI for some pop-ups. Active state and ordering in heirarchy to ensure blocking is controlled by [UIManagement](stm-unity/Assets/Scripts/UIManagement.cs).
    - **Tutorial**: All interfaces related to the tutorial are under this object. Managed by [TutorialController](stm-unity/Assets/Scripts/TutorialController.cs).
      - **Tutorial Section**: Interface used to display the tutorial text and highlight certain parts of the UI. Managed by [TutorialSectionUI](stm-unity/Assets/Scripts/TutorialSectionUI.cs).
      - **End Close**: UI displayed after the tutorial has been completed, offering players the chance to continue or to start a new game.
    - **Hover**: Displayed near some UI objects when hovered over for a period of time or clicked/tapped. Managed by [HoverPopUpUI](stm-unity/Assets/Scripts/HoverPopUpUI.cs).
  - **Questionnaire**: Questionnaire available after the season is completed when playing in 'RAGE' mode. Managed by [QuestionnaireUI](stm-unity/Assets/Scripts/QuestionnaireUI.cs).
  - **Feedback**: Feedback about player decisions within the game and the questionnaire which is made available after the questionnaire has been completed in 'RAGE' mode. Managed by [FeedbackUI](stm-unity/Assets/Scripts/FeedbackUI.cs).
  - **Settings Blocker**
    - **Settings**: UI for selecting the langauge the game should be played in. Managed by [SettingsUI](stm-unity/Assets/Scripts/SettingsUI.cs).
  - **Quit Blocker**: Pop-up and its blocker displayed when attempting to exit back to the main menu. Managed by [TeamSelectionUI](stm-unity/Assets/Scripts/TeamSelectionUI.cs).
  - **Loading**: UI displayed when an asynchronous method is taking place. Uses the Loading Unity Utility. 
- **Learning Pill Canvas**: In a separate canvas as animations can result in heavy slow down on mobile platforms.
  - **Pop-up Bounds**
    - **Learning Pill Pop-Up**: Interface displayed after post-race events to give advice to the player based on the decision they just took. Managed by [LearningPillUI](stm-unity/Assets/Scripts/TeamSelectionUI.cs).
- **Drag Canvas**: Objects being dragged are moved into this separate canvas as moving them in the main 'Canvas' can result in slow down on mobile platforms.
- **Demo Canvas**: Using [DemoVideo](stm-unity/Assets/Scripts/DemoVideo.cs), a video is displayed on this canvas during periods of inactivity when the game is in 'Demo Mode'.

## Key Game Logic Classes
- [**Avatar**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Avatar.cs): Handles everything related to the appearance of NPCs, including the generation of new avatars, the loading and saving of values and the storage of values at runtime.
- [**Boat**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Boat.cs): Handles all logic related to line-up information, including what positions are currently in use, the assigning of crew members into positions, the calculation of 'ideal' line-ups and comparing how the current line-up matches up to the possible ideal combinations. 
- [**ConfigStore**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/ConfigStore.cs): Loads all assets required for the FAtiMA Toolkit and all other config classes at start-up.
- [**CrewMember**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/CrewMember.cs): Handles all logic related to a crew member NPC. This includes generating new crew members and loading existing crew members from FAtiMA files, storing their skills and opinions (and those currently revealed to the player) and anything related to the player communicating with a crew member during gameplay.
- [**EventController**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/EventController.cs): Manages the triggering of post-race events, saving and loading current event progress, the sending and receiving of dialogue between the player and NPCs and the gathering of possible dialogue options during recruitment, crew member meetings and post-race events.
- [**GameManager**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/GameManager.cs): The main access point between the Unity layer and the logic layer. Handles the creation of new games and the loading of existing games. Logic classes should never know of the GameManager.
- [**Person**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Person.cs): Stores basic character information. Used to load and save information using FAtiMA RolePlayCharacter files.
- [**Team**](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation/Team.cs): Handles all logic related to the management of the team, including adding and removing crew members, updating the boat type to be used and storing the history of all race results for this team.

## Key Unity Classes
- [**AvatarDisplay**](stm-unity/Assets/Scripts/AvatarDisplay.cs): Using the information from an Avatar object, creates the NPC avatar displayed in Unity. Functionality includes setting the facial expression to be updated according to current NPC mood or a passed string or int value.
- [**CrewMemberUI**](stm-unity/Assets/Scripts/CrewMemberUI.cs): Placed on each crew member icon. Handles displaying the meeting pop-up when clicked and dragging the crew member into/out of positions.
- [**GameManagement**](stm-unity/Assets/Scripts/GameManagement.cs): Static class that makes commonly used information, such as the number of crew members or line-up history for the team, or common functionality, such as if an action is affordable, more easily accessible.
- [**MemberMeetingUI**](stm-unity/Assets/Scripts/MemberMeetingUI.cs): Class which manages the crew member meeting interface.
- [**PlatformSettings**](stm-unity/Assets/Scripts/PlatformSettings.cs): ScriptableObject that exists as a config for how the game should run.
- [**PositionDisplayUI**](stm-unity/Assets/Scripts/PositionDisplayUI.cs): Class which manages the interface which displays information on a position.
- [**PositionUI**](stm-unity/Assets/Scripts/PositionUI.cs): Placed on each position icon. Handles displaying the position pop-up when clicked and the placing of a crew member into a position.
- [**PostRaceEventUI**](stm-unity/Assets/Scripts/PostRaceEventUI.cs): Class which manages the post-race event interface.
- [**PostRacePersonUI**](stm-unity/Assets/Scripts/PostRacePersonUI.cs): Class which manages the post-race event interface for each NPC involved in the event.
- [**RecruitMemberUI**](stm-unity/Assets/Scripts/RecruitMemberUI.cs): Class which manages the recruitment interface.
- [**TeamSelectionUI**](stm-unity/Assets/Scripts/TeamSelectionUI.cs): This is the main class on the Unity side. Manages displaying historical session information, including placed crew members, scores and feedback, the instantiation and sorting of CrewMemberUIs and triggering sessions to be performed.
- [**TutorialController**](stm-unity/Assets/Scripts/TutorialController.cs): Handles the generation, displaying and progression of the tutorial.
- [**UIManagement**](stm-unity/Assets/Scripts/UIManagement.cs): Static class which makes all UI classes and the blockers more easily accessible to all other classes.
- [**UIStateManager**](stm-unity/Assets/Scripts/UIStateManager.cs): Handles signing into SUGAR, moving between UI states and the buttons made available on the main menu.

## Other Key Details
- All of the default assets used by the FAtiMA Toolkit can be found in the [NPC Templates folder](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.Simulation).
- The tutorial is managed by the [Tutorial.json file](stm-unity\Assets\Resources\Tutorial\Tutorial.json). To recreate the tutorial, use the option available on the Canvas\Team Management\Tutorial\Tutorial Collection object.

## Setting up your game with SUGAR
For information on setting up Sports Team Manager to use SUGAR, see the [SUGAR Quick Start Guide](http://api.sugarengine.org/v1/unity-client/tutorials/quick-start.html). **Make sure that [*Assets\StreamingAssets\SUGAR.config.json*](Assets/StreamingAssets/SUGAR.config.json) exists and the BaseUri value matches the Base Address in the SUGAR Prefab.** 

## Running SUGAR Locally
Using Sports Team Manager with a local version of SUGAR is as simple as changing the Base Address in the SUGAR Prefab, and the BaseUri value in [*Assets\StreamingAssets\SUGAR.config.json*](Assets/StreamingAssets/SUGAR.config.json).

## Game Mechanics
See the [Key Game Mechanics](GameMechanics.md) documentation for details about game mechanics, how they work and how to make changes to the core game system. 