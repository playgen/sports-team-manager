# Developer Guide
Below are more details about getting started when making changes in Sports Team Manager.

# Unity Game
## Key Scene Structure
- **Main Camera**
- **Canvas**: Camera used by the Canvas, meaning all UI and thus all gameplay is displayed on this camera. Contains [UIStateManager](stm-unity/Assets/Scripts/UIStateManager.cs), which manages the UI state, and BestFit, which helps ensure text sizing remains consistent.
  - **Main Menu**: Main hub UI to access starting a new game, loading saved games, signing into SUGAR and changing language settings.
  - **New Game**: UI for creating a New Game. Contains [NewGameUI](stm-unity/Assets/Scripts/NewGameUI.cs), which manages this UI, and FormKeyboardControls, which allows for tabbing between input fields.
  - **Load Game**: UI for loading a previously started game. Contains [LoadGameUI](stm-unity/Assets/Scripts/LoadGameUI.cs), which manages this UI.
  - **Team Management**: Main gameplay UI is contained within this GameObject. Contains [TeamSelectionUI](stm-unity/Assets/Scripts/TeamSelectionUI.cs), which manages displaying historical line-ups and submitting line-ups, and [EscapeAction](stm-unity/Assets/Scripts/EscapeAction.cs), which controls what logic should be performed when the Escape/Back key is pressed depending on what UI is currently visible.
    - **Top Details**: UI displayed at the top section of the screen. Contains ScreenSideUI.
    - **Boat Container Bounds**: Contains Boat Prefabs, used to display results and allow for managing of current line-up.
    - **Crew Container**: Used to contain crew member icons.
    - **Pop-up Bounds**
      - **Position Pop-Up**: Managed by and contains PositionDisplayUI.
      - **Meeting Pop-Up**: Managed by and contains MemberMeetingUI.
      - **Recruit Pop-Up**: Managed by and contains RecruitMemberUI.
      - **Event Pop-Up**: Managed by and contains PostRaceEventUI and PostRacePersonUI for ‘solo’ and ‘pair’ events.
      - **Event Impact Pop-Up**: Managed by and contains PostRaceEventImpactUI.
      - **Fire Warning Pop-Up**: Managed by Member Meeting UI.
      - **Hire Warning Pop-Up**: managed by Recruit Member UI.
      - **Pre-Race Pop-Up**: managed by Team Selection UI.
      - **Post-Race Pop-Up**: managed by Team Selection UI.
      - **End Result Pop-Up**: managed by Team Selection UI.
      - **Promotion Pop-Up**: managed by Team Selection UI.
      - **Blocker**: used to block background UI for some pop-ups.
      - **Bigger Blocker**: used to block background UI for some pop-ups.
      - **Learning Pill Pop-Up**: managed by and contains Learning Pill and Learning Pill UI.
    - **Tutorial**
      - **Tutorial Collection**: managed by and contains Tutorial Controller.
    - **Hover**: used to manage pop-up displayed by hovering over some UI.
  - **Questionnaire**: questionnaire displayed after scenario is completed. Contains Questionnaire and Questionnaire UI.
  - **Feedback**: information displayed about the dialogue options selected and questionnaire answers given. Contains Feedback and Feedback UI.
  - **Settings Blocker**: used to display the settings panel and block background UI.
  - **Quit Blocker**: used to display the quit to main menu panel and block background UI.
- **EventSystem**
- **Persistent Object**: contains the Game Manager Object (which contains the connection to the GameManager on the logic side) and the Tracker controller.
- **SUGAR**: prefab containing all components relating to SUGAR.
- **Music**: contains audio source and Music Control script, which ensuring that music tracks change and continue looping.
- **Reaction Sound**:contains audio source and Reaction Sound Control, used to play NPC reactions to dialogue. Currently unused.

## Key Game Logic Classes
- **Avatar**: holds all of the properties used to create the appearance of the NPCs as seen in game in Unity.
- **Boat**: stores and updates line-up information, including positions, who is currently in each position and how they compare to a possible ‘ideal’ line-up. 
- **ConfigStore**: used to load game details, including possible names and FAtiMA assets.
- **CrewMember**: stores the skills, opinions and avatar for each NPC. Used to get and update skills and opinion and return responses to dialogue spoken to that NPC.
- **EventController**: manages dialogue options in recruitment, talking to crew members and post-race events
- **GameManager**: used as an access point for Unity and allows for creating and loading game saves. Note: logic classes should never know of the Game Manager.
- **Person**: stores basic NPC information. Used to load and save information on each NPC stored in their FAtiMA RolePlayCharacter class.
- **Team**: used to add, remove and update the crew members and update the boat type being used for this team.

## Key UI Classes
- **AvatarDisplay**: uses information stored in Avatar class to create version displayed in Unity. Allows facial expression to be updated according to current NPC mood or passed value.
- **CrewMemberUI**: placed on each crew member icon. Used to open the meeting pop-up and drag the crew member into/out of a position.
- **MemberMeetingUI**: used to set up the crew member meeting pop-up. Allows the player to see all known information for the crew member and ask them questions on their skills and opinions.
- **PositionDisplayUI**: used to set up the position pop-up. Allows the player to see a brief description of the position, the skills required for it and a history of who has been in the position in the past.
- **PositionUI**: placed on each position icon. Used to open the position pop-up and place crew members into this position.
- **PostRaceEventUI**: used to set up the post-race event pop-up.
- **PostRacePersonUI**: used to set up the post-race event UI for each crew member involved in the event.
- **TeamSelectionUI**: used to display boat information, including placed crew members, scores, feedback etc. Also manages and set-up multiple pop-ups, including pre-race checks and post-race results. This is the main class on the Unity side.

## Other Key Details
- All of the default assets used by the FAtiMA-Toolkit can be found in the NPC Templates folder in stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation
- The tutorial is managed by the tutorial.json file in stm-unity\Assets\Resources\Tutorial. To recreate the tutorial, use the option available on the Canvas\Team Management\Tutorial\Tutorial Collection object.

# Setting up your game with SUGAR
For information on Setting up Space Modules Inc. using SUGAR, see [SUGAR Quick Start Guide](http://api.sugarengine.org/v1/unity-client/tutorials/quick-start.html). *make sure that Assets\StreamingAssets\SUGAR.config.json exists and the BaseUri value matches the Base Address in the SUGAR Prefab.* 

## Running SUGAR Locally
Using Space Modules inc. with a local version of SUGAR is as simple as changing the Base Address in the SUGAR Prefab, and the BaseUri value in *Assets\StreamingAssets\SUGAR.config.json*

# Game Mechanics
See [Key Game Mechanics](GameMechanics.md) for details about game mechanics, how they work and how to make changes to the core game system. 