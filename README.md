<h1 align="center">AliveCursorSDK</h1>

<p align="center">
    <img src="https://github.com/Threeyes/AliveCursorSDK/blob/main/Threeyes/SDK/Textures/ACSDK%20Icon.png?raw=true" alt="Logo" width="200px" height="200px" />
    <br />
    <i>SDK for AliveCursor.</i>
</p>


<p align="center">
	<a href="https://openupm.com/packages/com.threeyes.alivecursor.sdk/"><img src="https://img.shields.io/npm/v/com.threeyes.alivecursor.sdk?label=openupm&amp;registry_uri=https://package.openupm.com" /></a>
</p>

## Installation via [OpenUPM](https://openupm.com/packages/com.threeyes.alivecursor.sdk/)

### 

+ Install [Git](https://git-scm.com/).
+ Create an empty Win project with [Unity2021.3.5f1](https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe) (You can download it from [here](https://unity3d.com/get-unity/download/archive)).
+ Merge the following snippet to `Packages/manifest.json` file in your project, or just download the latest [manifest.json](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/manifest.json) file and replace it. (Make sure "com.threeyes.alivecursor.sdk": "*<u>X.X.X</u>" set to the latest version):
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
        "com.threeyes.alivecursor.sdk": "1.0.4",
        "com.yasirkula.beziersolution": "2.3.1",
        "com.dbrizov.naughtyattributes": "2.1.4",
        "jillejr.newtonsoft.json-for-unity.converters": "1.4.0"
    }
}
```
+ Download [ProjectSetting](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/ProjectSettings.zip) zip file, extract it and replace project's origin ProjectSettings folder.
+ Open the Project.

## Problem
+ If some error appear on first import, try cleanup or restart the project.
+ If some AliveCursorSDK's folder become empty, try reimport the package.
## Documentation
Please hold...