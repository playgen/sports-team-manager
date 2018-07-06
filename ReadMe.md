# Overview
Sports Team Manager is a sailing team management simulator that tasks players with managing crew conflicts, goals and emotions while identifying and assigning the best possible team line-up in order to lead their team to victory.

It is part of the [RAGE project](http://rageproject.eu/).

# Licensing
See the [LICENCE](LICENCE.md) file included in this project.

# Cloning
- When the project is cloned you will need to rebuild the stm-logic solution so that the Unity project has the complied files required to run the game simulation.

# Key Project Structure
- **stm-logic**
  - **PlayGen.RAGE.SportsTeamManager**
    - **PlayGen.RAGE.SportsTeamManager.Simulation**
      - **NPC Templates**: *Base files used to create characters and provide dialogue using the [FAtiMA Toolkit](https://gitlab.com/playgen/FAtiMA-Toolkit).*
    - **PlayGen.RAGE.SportsTeamManager.sln**: *Solution for simulation logic.*  
  - **RAGE Assets**: *[Included Assets](#included-assets) - Precompiled files that make up the various components included in the FAtiMA Toolkit.*      
- **stm-unity**: *Project to be opened in the Unity Editor.*
  - **Assets**
    - **Editor**
      - **ExcelToJson**: *[Included Assets](#included-assets)*.
    - **Evaluation Asset**: *[Included Assets](#included-assets) - An asset which helps evaluate the pedagogical efficiency of a game.*    
    - **RAGE Analytics**: *[Included Assets](#included-assets) - An asset to log to the RAGE Analytics server.*  
    - **Resources**
      - **PlatformSettings**: *A ScriptableObject used as a config for how the game should play. 'RAGE' mode will, among other things, instantiate the prefabs provided in the 'RAGE Objects' list, while 'Demo Mode' will limit/provide default options to players and plays a 'Demo Video' after a set period of inactivity.*
    - **SUGAR**: *[Included Assets](#included-assets) - An asset which adds social gamification features, including account management, achievements and leaderboards.*
    - **StreamingAssets**
      - **SUGAR.config.json**: *SUGAR configuration.*
  - **stm-installer**: *Installer project.*  

# Included Assets
- [Integrated Authoring Tool](https://gamecomponents.eu/content/201)
- [Role-Play Character](https://www.gamecomponents.eu/content/196)
- [Emotional Appraisal Asset](https://www.gamecomponents.eu/content/224)
- [Emotional Decision Making Asset](https://www.gamecomponents.eu/content/218)
- [Social Importance Dynamics](https://www.gamecomponents.eu/content/207)
- [Client-Side Tracker](https://gamecomponents.eu/content/232)
- [Server-Side Interaction Storage and Analytics](https://www.gamecomponents.eu/content/220)
- [Server-Side Dashboard and Analysis](https://www.gamecomponents.eu/content/195)
- [Evaluation Component](https://gamecomponents.eu/content/338)
- [SUGAR](https://gamecomponents.eu/content/200)
- [PlayGen FAtiMA Toolkit Fork](https://gitlab.com/playgen/FAtiMA-Toolkit)
- [PlayGen Unity Utilities](https://github.com/playgen/unity-utilities) - a collection of simple Unity utilities.
- [ExcelToJsonConverter](https://github.com/Benzino/ExcelToJsonConverter) - used to convert Excel Localization files to JSON.

# Development
## Requirements
- Windows OS
- Unity Editor *(Version 2017.2.3f1) - Known bug in later versions of Unity with UI Anchoring.*

## Process
1. Build the [stm-logic solution](stm-logic/PlayGen.RAGE.SportsTeamManager/PlayGen.RAGE.SportsTeamManager.sln).

2. Open stm-unity in the Unity Editor. Note that this may take a while due to the included videos being transcoded.

3. To run the game with SUGAR functionality, please refer to the [SUGAR Unity Documentation](http://api.sugarengine.org/v1/unity-client/tutorials/index.html).

4. To run the game with RAGE Analytics functionality, please refer to the [RAGE Analytics Documentation](Assets/RAGE%20Analytics/ReadMe.md).

5. To run the game with Evaluation Asset functionality, please refer to the [Evaluation Asset Documentation](Assets/Evaluation%20Asset/ReadMe.md).

## Updating
### FAtiMA Toolkit 
Using PlayGen's forked version of the FAtiMA Toolkit, build solution and place new DLLs (found in FAtiMA-Toolkit\Assets\IntegratedAuthoringTool\bin\Debug) into stm-logic\RAGE Assets folder. Note that code changes could be needed as a result of updates to this asset.

**Commit hash: 66bc9cfba528f8cbdeee2bb4c67b4ee77afc4b6a**

### PlayGen Unity Utilities 
Build solution and place new DLLs (found in various folders in this project) into stm-unity\Assets\SUGAR\bin folder. Note that code changes could be needed as a result of updates to this asset. New prefabs may also need copying, although take care not to overwrite customised versions of previous prefabs.  

New DLLs should also be copied into the lib\PlayGen Utilities folder in the SUGAR Unity project.

**Commit hash: 99d0daaa429430b36807bc5c28e567a61fc75e7d**

### SUGAR Unity Asset
Build solution and place new DLLs (found in SUGAR-Unity\Assets\SUGAR\Plugins) into stm-unity\Assets\SUGAR\bin folder. Note that code changes could be needed as a result of updates to this asset. It is advised that you do not remove the prefabs for SUGAR, as these have been edited to match the styling of Sports Team Manager. Only replace these assets if they are no longer compatible with the latest version of the SUGAR Unity asset, and even then be aware that you will need to recreate the previous styling.

**Commit hash: cffef4ea25af213e80c0a01c55d4045da4152a81**

### RAGE Analytics
Follow the instructions provided in the [RAGE Analytics Documentation](Assets/RAGE%20Analytics/ReadMe.md).

**Commit hash: 652a562c11d3b2ddb85bae509a719d30ed6ecd0c**

### Evaluation Asset
Follow the instructions provided in the [Evaluation Asset Documentation](Assets/Evaluation%20Asset/ReadMe.md).

**Commit hash: 6c4551df61ac1a1829ed0cbf7b9788362ee1342a**

# Build
## Process
Standalone and Android are currently supported using the default Unity build system. iOS may also be supported but is currently untested.

# Installer
[Wix](http://wixtoolset.org/) is used to create the Windows installer.

Using the [Game Launcher](https://gitlab.com/playgen/game-launcher) repository, games can be launched using a URL.

## Requirements:
- Wix Toolset
- Visual Studio 2017
- Wix Visual Studio Extension
- [Game Launcher](https://gitlab.com/playgen/game-launcher) project

## Process
The process for setting up a game installer is detailed within the [Game Launcher documentation](https://gitlab.com/playgen/game-launcher/blob/master/ReadMe.md#game-installer).

Note that the current stm-installer structure, rather than that found within the Game Launcher project, should be used until the project is updated to Unity 2018.

## Developer Guide
See the [Developer Guide](DeveloperGuide.md) for further details about the game.

## Game Mechanics
See the [Key Game Mechanics](GameMechanics.md) documentation for details about game mechanics, how they work and how to make changes to the core game system. 