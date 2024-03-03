# Changelog

## [4.3.0]
- Export Mod File with unique name.

## [4.2.2]
- Fix SOEventPlayerSettingManager's version update error on program start.

## [4.2.1]
- Fix build error.

## [4.2.0]
- Add namespace for Core scripts;
- Improve appearance for LiquidController and ClockController.

## [4.1.6]
- Fix: AC_RigBuilderHelper not update joints after size related state change.
- Item Manager add ShowOutputDirectory toggle.
- Delete wasted scripts.

## [4.1.5]
- Fix: After updating the SDK, it still prompts for resource expiration.

## [4.1.4]
- Components inherited from IHubSystemAudio_XXXHandler need to manual Register to receive callback.
- Code optimization.

## [4.1.2]
- PersistentData_Gradient add option: useHDR
- Add interface for each Controller.

## [4.1.1]
- Move CalculateLoudness method from AC_ManagerHolder.SystemAudioManager to AudioVisualizerTool.
- Move some common Manager interface from Threeyes.Steamworks.ManagerHolder to the one on their namespace(e.g. LogManager is now located at LogManagerHolder).
- Reorganize Codes.

## [4.0.2]
- Fix Joints using AC_RigBuilderHelper will be misplaced when cursor reactive.
- Update Facepunch.Steamworks.

## [4.0.1]
- Update Facepunch.Steamworks.
- Mod Scripts improvement.

## [4.0.0]
- Add 'USE_VFX' Define on Project Settings.
- Move most common script into 'Assets/Threeyes/Plugins/ThreeyesPluginExpert/Module/Steamworks'.

## [3.9.0]
Note: Mod using old Unity Version (Unity 2021.3.5f1LTS) is still supported, however it is recommended to submit using the latest version.
- Update to Unity 2022.3.4f1 LTS
- com.unity.2d.animation: 7.0.7→9.0.3
- com.unity.2d.psdimporter: 6.0.5→8.0.2
- com.unity.animation.rigging: 1.1.1→1.2.1
- com.unity.nuget.newtonsoft-json: 3.0.2→3.2.1
- com.unity.probuilder: 5.0.6→5.1.0
- com.unity.timeline: 1.6.4→1.7.5
- com.unity.visualeffectgraph/com.unity.render-pipelines.universal: 12.1.7→14.0.8
- com.unity.visualscripting: 1.7.8→1.8.0
- Add com.unity.splines: 2.3.0

## [3.8.2]
- Improve shadow resolution.
- Update AC_ObjectMovement_FollowTarget.ConfigInfo.

## [3.8.1]
- Internal SDK changed.

## [3.8.0]
Warning: breaking change! you may need to reupload your mod item and fix the missing link about scripts inside Module folder!
- ThreeyesPlugin: Move 'Module' folder outside of folder 'Core', and add extra asmdef file.

## [3.7.0]
- Fix missing characters in NotoSansSC-Regular SDF fonts.
- Ignore fields' serialization in AC_CharacterAnimatorController.RandomParamInfo.
- Add ground config on AC_DefaultEnvironmentController.
- Add TransparentShadowReceiver shader.
- Update shadow resolution on High UrpPipeline.

## [3.6.1]
- Fix error type define about AC_CharacterAnimatorController.RandomParamInfo.defaultValue.

## [3.5.4]
- Fix error UIcolor in 'WhiteboardToolbar Canvas.prefab'.
- Add Character Controller related Scripts.

## [3.5.3]
- Add Whiteboard feature (Including presets).

## [3.5.2]
- AC_TransformControllerBase require exter bored action, with extra dimension setting.
- AC_CommonSettingBehaviour add cursor size callback.
- Update Creeper related Components.

## [3.5.0]
- Fix ItemManager error on editor start.
- Fix Rigidbody moving lag problem.
- Add config for skylight's shadow.

## [3.4.0]
- Add AC_ImagePlayer for external image or gif files.
- Add support for Mod's Run&Build.

## [3.3.8]
- Update Hair Plugin version to 0.10.0-exp.1 (Warning: You need to manual relpace the latest manifest.json file).

## [3.3.7]
- Fix GizmoDrawer missing Mesh problem.

## [3.3.6]
-Fix AC_TiltByTargetMovement not flexible.
-Add callback events.

## [3.3.5]
- Update hair's git to official site.
- Increase AudioManager's fftSize to 4096.
- Change realte file path's Naming rules.
- Add HDR/Alpha support for RuntimeEdit Color field.
- Fix RuntimeEdit not support nested List type.

## [3.3.4]
- Fix：Simulator scene not using the reflection probe.

## [3.3.3]
- Add support for com.unity.demoteam.hair. (Warning: you should update your project's manifest.json file)
- Delete Plugin 'UnityFurURP'.


## [3.3.2]
- Reopen Mod scene after build.
- AudioManager won't invoke event if not related Handler.
- AudioManager will invoke event event if not audio input.
- InputSimulator add support for XButton1/ XButton2.

## [3.3.1]
- Add Creeper Controller’s Configs.

## [3.3.0]
- Add Creeper Controller.

## [3.2.5]
- Fix Liquid Controller not init with PD data on load.

## [3.2.4]
- Fix LazyExtension_Common.InstantiatePrefab param num change cause method not found problem.

## [3.2.3]
- Simulator add "Stay At Center" Toggle;
- Change AC_WSItemAgeRating to Flag (Breaking change!).

## [3.2.2]
- Fix Simulator scene's reflection missing problem.

## [3.2.1]
- Rename AC_Clock to AC_ClockController.
- Add ViewBoundsFitter prefab.
- Add Zibra Liquids related Controller.

## [3.2.0]
- Sync version with the AliveCursor program.
- Rename and add some fields for AC_DefaultEnvironmentController.ConfigInfo (Breaking change!).
- Add PostProcessing support.
- Update "com.unity.2d.animation" to "7.0.7".
- Update "com.unity.2d.psdimporter" to "6.0.5".

## [1.1.3]
- Fix ItemManager failed to get the latest SDK verison.

## [1.1.2]
- Hide AssistantManagerSimulator's UI on Capture preview image;
- Simulator scene's default EnvironmentControllerConfig's skybox type changed to default.

## [1.1.1]
- Update "com.yasirkula.beziersolution" to "2.3.2".
- Disable Most Hub's Manager function when AC is disabled.
- Add AC_CommonSettingBehaviour Component.
- Improve UI hints on Simulator.

## [1.1.0]
- Update AC_VisualEffectHelper.

## [1.0.9]
- Add shortcut key to change cursor size in simulator scene.

## [1.0.8]
- Fix:
>- 2D Clock missing BackGround Image.
>- Can't export custom scripts by provided manifest.json.

## [1.0.7]
- Fix error when your try to open simulator scene from none embed package.
- 
## [1.0.6]
- Update EventPlayer samples.

## [1.0.5]
- Add visualscripting&visualeffectgraph to package.json

## [1.0.4]
- Fix PD_Enum will cause UMod failed to reflection (caused by Dictionary field).

## [1.0.3]
- Remove codes that costs warning.

## [1.0.2]
- Add Samples

## [1.0.1]
- Fix README

## [1.0.0]
- New release
