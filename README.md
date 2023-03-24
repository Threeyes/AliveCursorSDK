<h1 align="center">AliveCursorSDK</h1>

<p align="center">
    <a href="https://store.steampowered.com/app/1606490/Alive_Cursor/"><img src="https://github.com/Threeyes/AliveCursorSDK/blob/main/Threeyes/SDK/Textures/ACSDK%20Icon.png?raw=true" alt="Logo" width="200px" height="200px" />
    <br />
	<a href="https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe"><img src="https://img.shields.io/badge/%20Unity-2021.3.5f1%20-blue" /></a>
	<a href="https://openupm.com/packages/com.threeyes.alivecursor.sdk/"><img src="https://img.shields.io/npm/v/com.threeyes.alivecursor.sdk?label=openupm&amp;registry_uri=https://package.openupm.com" /></a>
	<a href="https://github.com/Threeyes/AliveCursorSDK/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-MIT-brightgreen.svg" /></a>
    <br />
</p>

## Language
<p float="left">
  <a href="https://github.com/Threeyes/AliveCursorSDK/blob/main/locale/README-zh-CN.md"><img src="https://raw.githubusercontent.com/hampusborgos/country-flags/main/png100px/cn.png"/></a>
    <a href="https://github.com/Threeyes/AliveCursorSDK"><img src="https://raw.githubusercontent.com/hampusborgos/country-flags/main/png100px/us.png"/></a>
</p>

## Description
[AliveCursor](https://store.steampowered.com/app/1606490/Alive_Cursor/) is a software released in Steam, it supports replacing the system cursor with any Mods. It has the following characteristics:
+ Provide a complete visual function module, which can quickly implement common functions without understanding programming.
+ Support dozens of stable [third-party open source libraries](https://github.com/Threeyes/AliveCursorSDK/wiki/Third-party) To facilitate achieving better results;
+ Support hot updates, you can write logic using C # or Visual Scripting;
+ Support for exposing various parameters (eg: base values, textures or material colors) to users to facilitate customization of the final effect.

## Installation via [OpenUPM](https://openupm.com/packages/com.threeyes.alivecursor.sdk/)
1. Install [Git](https://git-scm.com/).
2. Create an empty Win project with [Unity2021.3.5f1](https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe).
3. Download the latest [manifest.json](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/manifest.json) file and replace the file with the same name in the `[ProjectRootPath]/Packages` directory. 
4. Download [ProjectSetting](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/ProjectSettings.zip) zip file, extract it and replace the folder with the same name in the `[ProjectRootPath]` directory. 
5. Open the project, Make sure `Packages/manifest.json` contain one and only one External Script Editor that you are using in `Preferences/External Tools` (eg, VisualStudio):

![image](https://user-images.githubusercontent.com/13210990/180822147-5a917199-279f-4cbb-a073-32e5078e2709.png)

## Possible Errors
+ If some error appear on first import, try close the project, delete `Library` folder then reopen.
+ If some subfolder in `Package/AliveCursorSDK` become empty, try reimport the package on `Package Manager` window.

## Documentation
Check out [wiki](https://github.com/Threeyes/AliveCursorSDK/wiki).

## Contact
+ [QQ](https://im.qq.com/index/) Group: 673880395

## Samples
To find out the possibility of AliveCursor, I also upload different kinds of mods to [workshop](https://steamcommunity.com/profiles/76561199378980403/myworkshopfiles/?appid=1606490), feel free to check out the [AliveCursor_ModUploader](https://github.com/Threeyes/AliveCursor_ModUploader) project.
<p align="center">
    <a href="https://github.com/Threeyes/AliveCursor_ModUploader"><img src="https://user-images.githubusercontent.com/13210990/195757514-014d8d7d-b0bf-438c-9e53-40300185e1a2.gif" alt="Item图片墙" width="600px" height="450px" />
    <br />
</p>    