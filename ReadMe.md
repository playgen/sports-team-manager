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

# Included Assets
- [Server-Side Dashboard and Analysis](https://www.gamecomponents.eu/content/195)
- [Role-Play Character](https://www.gamecomponents.eu/content/196)
- [SUGAR](https://gamecomponents.eu/content/200)
- [Integrated Authoring Tool](https://gamecomponents.eu/content/201): is used in by the emotional decisionmaking component.
- [Social Importance Dynamics](https://www.gamecomponents.eu/content/207)
- [Emotional Decision Making Asset](https://www.gamecomponents.eu/content/218)
- [Server-Side Interaction Storage and Analytics](https://www.gamecomponents.eu/content/220)
- [Emotional Appraisal Asset](https://www.gamecomponents.eu/content/224)
- [Client-Side Tracker](https://gamecomponents.eu/content/232)
- [Evaluation Component](https://gamecomponents.eu/content/338)
- [ExcelToJsonConverter](https://github.com/Benzino/ExcelToJsonConverter): is used to convert Excel Localization files to jSON.
- [PlayGen Unity Utilities](git@gitlab.com:playgen/unity-utilities.git): is a collection of simple game utilities.

# Development
## Requirements
- Windows OS
- Unity Editor *(Version 2017.2.3f1) - Known bug in later versions of Unity with UI Anchoring*

## Process
1. Build stm-logic solution (stm-logic\PlayGen.RAGE.SportsTeamManager\PlayGen.RAGE.SportsTeamManager.sln).

2. To get the game running in editor, Navigate to Tools/SUGAR/Set Auto Log-in Values and add the following [Custom Args](#Custom-Args):
`ingameq=true;lockafterq=true;feedback=2;forcelaunch=true`    
Note: Each argument should be separated with a `;`  

### Custom Args
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

## Updating
### FAtiMA-Toolkit 
Build solution and place new DLLs (found in FAtiMA-Toolkit\Assets\IntegratedAuthoringTool\bin\Debug) into lib\IntegratedAuthoringTool folder. Note that code changes could be needed as a result of updates to this asset.

### GameWork.Unity
Build solution and place new DLLs (found in GameWork.Unity/bin) into lib\GameWork folder. Note that code changes could be needed as a result of updates to this asset.

### PlayGen Unity Utilities 
Build solution and place new DLLs (found in various folders in this project) into lib\PlayGenUtilities folder. Note that code changes could be needed as a result of updates to this asset. New prefabs may also need copying, although take care not to overwrite customised versions of previous prefabs.  

New DLLs should also be copied into the lib\PlayGen Utilities folders in the PlayGen Unity Settings and SUGAR Unity projects. 

### RAGE Analytics Unity Tracker
Update the files found in the unity-tracker-master folder in stm-unity. Note that code changes could be needed as a result of updates to this asset.

### SUGAR Unity Asset
Update the DLLs found in the lib\SUGAR folder in stm-unity  

Note: It is advised that you do not remove the Prefabs for SUGAR, as these have been edited to match the styling of Space Modules Inc. Only replace these assets if they are no longer compatible with the latest version of the SUGAR Unity asset, and even then be aware that you will need to recreate the previous styling if this is done.

# Build
## Process
Standalone, Android and iOS are currently supported using the default Unity build system.

# Installer
[Wix](http://wixtoolset.org/) is used to create the Windows installer.

## Requirements
- Wix Toolset
- Visual Studio 2017
- Wix Visual Studio Extension

## Process
1. Create a Unity PC Build at stm-unity\Build\Sports Team Manager called “SportsTeamManager”.

2. Once built, go to the stm-unity solution (stm-unity\stm-unity.sln) and build the stm-setup project.

3. The resulting windows installer can be found at stm-unity\stm-installer\bin\Debug\Sports_Team_Manager_Setup.msi.

## Developer Guide
See the [Developer Guide](DeveloperGuide.md) for further details about the game.