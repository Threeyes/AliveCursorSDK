<h1 align='center'>Threeyes Action</h1>

## Description
Use ScriptableObject to store&execute waitable Action configs(such as changing AnimatorState, or tweening using DOTween), support reuse and dynamic modification of configurations.


## Setup
Before running the demo, please ensure that you have downloaded the latest [EventPlayer](https://assetstore.unity.com/packages/tools/visual-scripting/event-player-116731) plugin through AssetStore or Github.

You can also download the following optional plugins according to project requirements:
- [DoTween](https://github.com/Demigiant/dotween): A free animation engine. This plugin is used to execute specific animations.
- [NaughtyAttributes](https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996): A free extension for the Unity Inspector, used to present fields of SOAction on demand.

Please ensure that you have link to their's asmdef file (if exists) correctly.


## Samples
Make sure you have installed the required plugins, then open&play the scenes in that directory.

## Trouble Shooting
### Some files conflicts after update to newer version?
I might move/rename the folder, please delete the old plugin and reimport the latest plugin (always backup your project first!).

### Sample's material become purple on Built-In Pipeline?
Sample's default material is for URP. You need to import 'MaterialseBuildInRP. unitypackage' from the Sample folder to cover all existing materials.

### Sample's material become purple on HDRP Pipeline?
First, inport all materials of Built-In Pipeline as mentioned above, and then select all materials, then click on the menu 'Edit/Rendering/Materials/Convert selected Built-in Materials to HDRP'.

### (Advanced) How to prevent plugins from automatically detecting and adding DefineSymbol?
Click 'Tools/Threeyes/Conditional Compilation Setting' menu, then disable the AutoUpdate field in the Inspector. If you find that the plugin does not automatically recognize the installed dependent plugins, you can also try clicking 'Manual Update'.