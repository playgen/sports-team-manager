# Key Game Mechanics

Below are details of the key game mechanics that are present in Sports Team Manager, how they work and how to make changes

## <span style="color:red">**TODO**</span> Saving and Loading Teams
outline briefly how the game is saved and loaded

## <span style="color:red">**TODO**</span> State Manager
brief description of the state manager with details of how to change state and create a new state

## <span style="color:red">**TODO**</span> 'Boat' Configurations
worked example of how boat configurations are loaded along with how character skills are integrated

## <span style="color:red">**TODO**</span> Team Management

### <span style="color:red">**TODO**</span> Skills
Charisma, Perception, Quickness, Strength, Willpower, Wisdom
Where the characters skills are defined and any limitations (eg. is there 1 main skill, or is it possible (unlikely) to have 1 person be top in every skill)

### <span style="color:red">**TODO**</span> Character Dialogue

### <span style="color:red">**TODO**</span> Enforced Team Management Restrictions
Max team size, talk time, Number of practice sessions, season length. Links to be able to make   changes to these.

### <span style="color:red">**TODO**</span> Post-Race Standings
Describe how the post race standings are calculated, and show example code

### <span style="color:red">**TODO**</span> Tutorial States
How the tutorial is designed/created, how each step is defined and how to add/remove a step, included details of spreadsheet.

Each step in the tutorial are defined by the following variables

Variable | Type | Description
:--- | :--- | :---
SectionName | string | Section Name is only used in inspector to identify each step easily
SectionTextHolder | List\<LanguageKeyValuePair> | Text to be displayed to the user, contains text for all lanuages
ShowOnLeft | bool | If the popup should be shown on the left side of the screen
HighlightedObject | List\<string> | Path to the object that will be highlighted to guide the player (from Canvas level)
Triggers | List\<TriggerKeyValuePair> | The events that must happen for the player to complete this step of the tutorial
UniqueEvents | bool | Whether the events that happen before must be unique and in the correct order
EventTRiggerCountRequired | int | The number of times players must complete activate the trigger event to continue to the next step
SafeToSave | bool | If the current step in the tutorial can be saved, sometimes UI must be open for the current step to activate/finish properly
BlacklistButtons | List\<StringList> | Buttons which cannot be interacted with at this stage in the tutorial
CustomAttributes | List\<string> | Additional attributes to help set up the stage in the tutorial

The current tutorial steps are defined in the [SportsTeamManagerTutorial Excel File](stm-unity/Assets/Resources/Tutorial/SportsTeamManagerTutorial.xlsx), which is converted to JSON and converted to a list of TutorialObjects using the [Context Menu](https://unity3d.com/sites/default/files/styles/original/public/learn/MenuItems06.png?itok=NZTNMINK) for TutorialController. 

### <span style="color:red">**TODO**</span> Inter-team and Manager Relationships
Brief overview of the way in which crew memebers relationships with other crew and the manager affect their performance and how this is calculated, link to FAtiMA


### <span style="color:red">**TODO**</span>


### <span style="color:red">**TODO**</span>


### <span style="color:red">**TODO**</span>