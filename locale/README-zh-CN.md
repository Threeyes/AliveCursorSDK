<h1 align="center">AliveCursorSDK</h1>

<p align="center">
    <img src="https://github.com/Threeyes/AliveCursorSDK/blob/main/Threeyes/SDK/Textures/ACSDK%20Icon.png?raw=true" alt="Logo" width="200px" height="200px" />
    <br />
	<a href="https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe"><img src="https://img.shields.io/badge/%20Unity-2021.3.5f1%20-blue" /></a>
	<a href="https://openupm.com/packages/com.threeyes.alivecursor.sdk/"><img src="https://img.shields.io/npm/v/com.threeyes.alivecursor.sdk?label=openupm&amp;registry_uri=https://package.openupm.com" /></a>
	<a href="https://github.com/Threeyes/AliveCursorSDK/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-MIT-brightgreen.svg" /></a>
    <br />
</p>

## 语言
<p float="left">
  <a href="https://github.com/Threeyes/AliveCursorSDK/blob/main/locale/README-zh-CN.md"><img src="https://raw.githubusercontent.com/hampusborgos/country-flags/main/png100px/cn.png"/></a>
    <a href="https://github.com/Threeyes/AliveCursorSDK"><img src="https://raw.githubusercontent.com/hampusborgos/country-flags/main/png100px/us.png"/></a>
</p>

## 通过[OpenUPM](https://openupm.com/packages/com.threeyes.alivecursor.sdk/)安装
+ 安装[Git](https://git-scm.com/).
+ 使用[Unity2021.3.5f1](https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe)创建一个Windows空项目.
+ 将以下内容合并到项目中的`Packages/manifest.json`文件中，或者直接下载最新的[manifest.json](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/manifest.json)文件并直接替换（需要确保<u>X.X.X</u>已经设置为最新的版本）:
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
+ 确保`PackagesManager`窗口包含**唯一**的脚本编辑器，并且与`Preferences` 窗口-External Tools中的设置一致（如VisualStudio）：
![image](https://user-images.githubusercontent.com/13210990/180822147-5a917199-279f-4cbb-a073-32e5078e2709.png)

+ 下载 [ProjectSetting](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/ProjectSettings.zip)压缩文件，解压后覆盖项目根目录的ProjectSettings文件夹。
+ 打开该项目。

## 可能出现的错误
+ 如果第一次打开时出现错误，请尝试关闭程序，删掉`Library`文件夹然后重启项目。
+ 如果`Package/AliveCursorSDK`中的某个子文件夹为空，请在`Package Manager`窗口重新导入该包。

## 文档
请查阅[wiki](https://github.com/Threeyes/AliveCursorSDK/wiki)。

## 联系方式
+ [QQ](https://im.qq.com/index/)群: 673880395

## 案例
为了发挥《AliveCursor》的潜力，我上传了不同类型的MOD到[创意工坊](https://steamcommunity.com/profiles/76561199378980403/myworkshopfiles/?appid=1606490)，你可以下载[AliveCursor_ModUploader](https://github.com/Threeyes/AliveCursor_ModUploader)以便参考。
<p align="center">
    <img src="https://user-images.githubusercontent.com/13210990/195757514-014d8d7d-b0bf-438c-9e53-40300185e1a2.gif" alt="Item图片墙" width="600px" height="450px" />
    <br />
</p>    