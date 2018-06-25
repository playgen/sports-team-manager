# Overview
Sports Team Manager is a sailboat racing game where the player has to manage crew conflicts, goals and moods while identifying and assigning the best possible team lineup.

It is part of the [RAGE project](http://rageproject.eu/).

# Licensing
See the [LICENCE](LICENCE.md) file included in this project.

# Key Project Structure
- **stm-logic**
  - **PlayGen.RAGE.SportsTeamManager**
    - **PlayGen.RAGE.SportsTeamManager.sln**: *Simulation Logic*  
  - **RAGE Assets**: *precompiled [Included-Assets](#Included-Assets) libraries that make up the Integrated Authoring Tool*      
- **stm-unity**
  - **Assets**
    - **Evaluation Asset**: *[Included-Assets](#Included-Assets) asset that evaluates the pedagogical efficiency of the game*  
    - **RAGE Analytics**: *[Included-Assets](#Included-Assets) asset to log to the RAGE analytics server*  
    - **SUGAR**: *[Included-Assets](#Included-Assets) Social Gamification Backend*    
    - **StreamingAssets**
      - **SUGAR.config.json**: *[Included-Assets](#Included-Assets) SUGAR configuration*
  - **stm-installer**: *installer project*  

# Included Assets:
- [SUGAR](http://www.sugarengine.org/) is a Social Gamification Backend.
- [Evaluation Asset](https://gamecomponents.eu/content/338) asset that evaluates the pedagogical efficiency of the game.  
- [RAGE Analytics](https://gamecomponents.eu/content/232) asset to log to the RAGE analytics server.
- [ExcelToJsonConverter](https://github.com/Benzino/ExcelToJsonConverter): is used to convert Excel Localization files to jSON.
- [GameWork](https://github.com/Game-Work/GameWork.Unity) is a game development framework. 
- [Integrated Authoring Tool](https://gamecomponents.eu/content/201): is used in by the emotional decisionmaking component.

todo

# Development:
## Requirements:
- Windows OS
- Unity Editor

## Process
1. Build stm-logic solution (stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.sln).

2. To get the game running in editor, Navigate to Tools/SUGAR/Set Auto Log-in Values and add the following [Custom Args](#Custom-Args):
`ingameq=true;lockafterq=true;feedback=2;forcelaunch=true`    
Note: Each argument should be separated with a `;`  

### Custom Args: 
- `ingameq`: Should the in game questionnaire be shown
- `lockafterq`: Should the game be locked after the questions have been complete
- `feedback`: The feedback level:  
  - level 1: End game only  
  - level 2: End game and conversation review  
  - Level 3: End game, conversation review and in game feedback
- `forcelaunch` If the game should ignore time stamp
- `round`: which round of scenarios to playgi

## Getting Started
- Rebuild the stm-logic project at stm-logic/PlayGen.RAGE.SportsTeamManager/Playgen.RAGE.SportsTeamManager.sln
- Open stm-unity in Unity Editor, this can take a while to transcode the included video files.
- Set up the game for SUGAR, for more information see the [SUGAR Quick Start Guide](http://api.sugarengine.org/v1/unity-client/tutorials/quick-start.html)
- Optionally, set up credentials for logging in to SUGAR on game start at SUGAR -> Set Auto Log-in Values

## Updating:
### FAtiMA-Toolkit 
Build solution and place new DLLs (found in FAtiMA-Toolkit\Assets\IntegratedAuthoringTool\bin\Debug) into lib\IntegratedAuthoringTool folder. Note that code changes could be needed as a result of updates to this asset.

### GameWork.Unity ha
Build solution and place new DLLs (found in GameWork.Unity/bin) into lib\GameWork folder. Note that code changes could be needed as a result of updates to this asset.

### PlayGen Unity Utilities 
Build solution and place new DLLs (found in various folders in this project) into lib\PlayGenUtilities folder. Note that code changes could be needed as a result of updates to this asset. New prefabs may also need copying, although take care not to overwrite customised versions of previous prefabs.  

New DLLs should also be copied into the lib\PlayGen Utilities folders in the PlayGen Unity Settings and SUGAR Unity projects. 

### RAGE Analytics Unity Tracker
Update the files found in the unity-tracker-master folder in stm-unity. Note that code changes could be needed as a result of updates to this asset.

### SUGAR Unity Asset
Update the DLLs found in the lib\SUGAR folder in stm-unity  

Note: It is advised that you do not remove the Prefabs for SUGAR, as these have been edited to match the styling of Space Modules Inc. Only replace these assets if they are no longer compatible with the latest version of the SUGAR Unity asset, and even then be aware that you will need to recreate the previous styling if this is done.

# Build:
## Process:
Standalone, Android and iOS are currently supported using the default Unity build system.

# Installer:
[Wix](http://wixtoolset.org/) is used to create the Windows installer.

## Requirements:
- Wix Toolset
- Visual Studio 2017
- Wix Visual Studio Extension

## Process:
1. Create a Unity PC Build at stm-unity\Build\Sports Team Manager called “SportsTeamManager”.

2. Once built, go to the stm-unity solution (stm-unity\stm-unity.sln) and build the stm-setup project.

3. The resulting windows installer can be found at stm-unity\stm-installer\bin\Debug\Sports_Team_Manager_Setup.msi.


# Unity Game
## Key Scene Structure
- Main Camera
- Canvas - contains UI State Manager (manages UI state) and Best Fit (used to keep font sizing consistent)
  - Main Menu
  - New Game - contains New Game, New Game UI and Form Keyboard Controls (allowing tabbing between input fields).
  - Load Game - contains Load Game and Load Game UI.
  - Team Management - main gameplay screen. Contains Team Selection, Team Selection UI and Escape Action.
    - Top Details - UI displayed at the top section of the screen. Contains Screen Side UI.
    - Boat Container Bounds - contains Boat Prefabs, used to display results and allow for managing of current line-up.
    - Crew Container - used to contain crew member icons.
    - Pop-up Bounds
      - Position Pop-Up - managed by and contains Position Display and Position Display UI.
      - Meeting Pop-Up - managed by and contains Member Meeting and Member Meeting UI.
      - Recruit Pop-Up - managed by and contains Recruit Member and Recruit Member UI.
      - Event Pop-Up - managed by and contains Post Race Event, Post Race Event UI and Post Race Person UI for ‘solo’ and ‘pair’ events.
      - Fire Warning Pop-Up - managed by Member Meeting UI.
      - Hire Warning Pop-Up - managed by Recruit Member UI.
      - Pre-Race Pop-Up - managed by Team Selection UI.
      - Post-Race Pop-Up - managed by Team Selection UI.
      - End Result Pop-Up - managed by Team Selection UI.
      - Promotion Pop-Up - managed by Team Selection UI.
      - Blocker - used to block background UI for some pop-ups.
      - Bigger Blocker - used to block background UI for some pop-ups.
      - Learning Pill Pop-Up - managed by and contains Learning Pill and Learning Pill UI.
    - Tutorial
      - Tutorial Collection - managed by and contains Tutorial Controller.
    - Hover - used to manage pop-up displayed by hovering over some UI.
  - Questionnaire - questionnaire displayed after scenario is completed. Contains Questionnaire and Questionnaire UI.
  - Feedback - information displayed about the dialogue options selected and questionnaire answers given. Contains Feedback and Feedback UI.
  - Settings Blocker - used to display the settings panel and block background UI.
  - Quit Blocker - used to display the quit to main menu panel and block background UI.
- EventSystem
- Persistent Object - contains the Game Manager Object (which contains the connection to the GameManager on the logic side) and the Tracker controller.
- SUGAR - prefab containing all components relating to SUGAR.
- Music - contains audio source and Music Control script, which ensuring that music tracks change and continue looping.
- Reaction Sound -contains audio source and Reaction Sound Control, used to play NPC reactions to dialogue. Currently unused.

## Key Game Logic Classes
- Avatar - holds all of the properties used to create the appearance of the NPCs as seen in game in Unity.
- Boat - stores and updates line-up information, including positions, who is currently in each position and how they compare to a possible ‘ideal’ line-up. 
- ConfigStore - used to load game details, including possible names and FAtiMA assets.
- CrewMember - stores the skills, opinions and avatar for each NPC. Used to get and update skills and opinion and return responses to dialogue spoken to that NPC.
- EventController - manages dialogue options in recruitment, talking to crew members and post-race events
- GameManager - used as an access point for Unity and allows for creating and loading game saves. Note: logic classes should never know of the Game Manager.
- Person - stores basic NPC information. Used to load and save information on each NPC stored in their FAtiMA RolePlayCharacter class.
- Team - used to add, remove and update the crew members and update the boat type being used for this team.

## Key UI Classes
- AvatarDisplay - uses information stored in Avatar class to create version displayed in Unity. Allows facial expression to be updated according to current NPC mood or passed value.
- CrewMemberUI - placed on each crew member icon. Used to open the meeting pop-up and drag the crew member into/out of a position.
- MemberMeetingUI - used to set up the crew member meeting pop-up. Allows the player to see all known information for the crew member and ask them questions on their skills and opinions.
- PositionDisplayUI - used to set up the position pop-up. Allows the player to see a brief description of the position, the skills required for it and a history of who has been in the position in the past.
- PositionUI - placed on each position icon. Used to open the position pop-up and place crew members into this position.
- PostRaceEventUI - used to set up the post-race event pop-up.
- PostRacePersonUI - used to set up the post-race event UI for each crew member involved in the event.
- TeamSelectionUI - used to display boat information, including placed crew members, scores, feedback etc. Also manages and set-up multiple pop-ups, including pre-race checks and post-race results. This is the main class on the Unity side.

## Other Key Details
- All of the default assets used by the FAtiMA-Toolkit can be found in the NPC Templates folder in stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.Simulation
- The tutorial is managed by the tutorial.json file in stm-unity\Assets\Resources\Tutorial. To recreate the tutorial, use the option available on the Canvas\Team Management\Tutorial\Tutorial Collection object.

### Setting up your game with SUGAR
For information on Setting up Space Modules Inc. using SUGAR, see [SUGAR Quick Start Guide](http://api.sugarengine.org/v1/unity-client/tutorials/quick-start.html). *make sure that Assets\StreamingAssets\SUGAR.config.json exists and the BaseUri value matches the Base Address in the SUGAR Prefab.* 

### Running SUGAR Locally
Using Space Modules inc. with a local version of SUGAR is as simple as changing the Base Address in the SUGAR Prefab, and the BaseUri value in *Assets\StreamingAssets\SUGAR.config.json*