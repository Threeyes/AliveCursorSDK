<h1 align="center">AliveCursorSDK</h1>

<p align="center">
    <img src="https://github.com/Threeyes/AliveCursorSDK/blob/main/Threeyes/SDK/Textures/ACSDK%20Icon.png?raw=true" alt="Logo" width="200px" height="200px" />
    <br />
	<a href="https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe"><img src="https://img.shields.io/badge/%20Unity-2021.3.5f1%20-blue" /></a>
	<a href="https://openupm.com/packages/com.threeyes.alivecursor.sdk/"><img src="https://img.shields.io/npm/v/com.threeyes.alivecursor.sdk?label=openupm&amp;registry_uri=https://package.openupm.com" /></a>
	<a href="https://github.com/Threeyes/AliveCursorSDK/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-MIT-brightgreen.svg" /></a>
    <img src="https://user-images.githubusercontent.com/13210990/195757514-014d8d7d-b0bf-438c-9e53-40300185e1a2.gif" alt="Item图片墙" width="600px" height="450px" />
    <br />
</p>

## Installation via [OpenUPM](https://openupm.com/packages/com.threeyes.alivecursor.sdk/)

### 

+ Install [Git](https://git-scm.com/).
+ Create an empty Win project with [Unity2021.3.5f1](https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe) (You can download it from [here](https://unity3d.com/get-unity/download/archive)).
+ Merge the following snippet to `Packages/manifest.json` file in your project, or just download the latest [manifest.json](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/manifest.json) file and replace it. (Make sure the version <u>X.X.X</u> has set to the latest version):
```json
{
    "scopedRegistries": [
        {
            "name": "package.openupm.com",
            "url": "https://package.openupm.com",
            "scopes": [
                "com.openupm",
                "com.beans.deform",
                "com.coffee.ui-effect",
                "com.zibra.liquids-free",
                "com.threeyes.alivecursor.sdk",
                "com.yasirkula.beziersolution",
                "com.dbrizov.naughtyattributes",
                "jillejr.newtonsoft.json-for-unity.converters"
            ]
        }
    ],
    "dependencies": {
        "com.beans.deform": "1.2.1",
        "com.coffee.ui-effect": "4.0.0-preview.9",
        "com.zibra.liquids-free": "1.4.5",
        "com.threeyes.alivecursor.sdk": "X.X.X",
        "com.yasirkula.beziersolution": "2.3.2",
        "com.dbrizov.naughtyattributes": "2.1.4",
        "jillejr.newtonsoft.json-for-unity.converters": "1.4.0"
    }
}
```
+ Make sure `Packages/manifest.json` contain one and only one External Script Editor that you are using in `Preferences` Window/External Tools (eg:VisualStudio):

![image](https://user-images.githubusercontent.com/13210990/180822147-5a917199-279f-4cbb-a073-32e5078e2709.png)

+ Download [ProjectSetting](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/ProjectSettings.zip) zip file, extract it and replace project's origin ProjectSettings folder.
+ Open the project.

## Possible Errors
+ If some error appear on first import, try cleanup or restart the project.
+ If some subfolder in Package/AliveCursorSDK become empty, try reimport the package.

## Documentation
Check out [wiki](https://github.com/Threeyes/AliveCursorSDK/wiki).

## Samples
To find out the possibility of AliveCursor, I also upload different kinds of mods to [workshop](https://steamcommunity.com/profiles/76561199378980403/myworkshopfiles/?appid=1606490), you can find the source project [here](https://github.com/Threeyes/AliveCursor_ModUploader);
