<h1 align="center">AliveCursorSDK</h1>

<p align="center">
    <a href="https://store.steampowered.com/app/1606490/Alive_Cursor/"><img src="https://github.com/Threeyes/AliveCursorSDK/blob/main/Threeyes/SDK/Textures/ACSDK%20Icon.png?raw=true" alt="Logo" width="200px" height="200px" />
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

## 简介
《[躁动光标](https://store.steampowered.com/app/1606490/Alive_Cursor/)》是一款发布在Steam的软件，它支持将系统光标替换成任意Mod。他有以下特点：
+ 提供完善的可视化功能模块，不懂编程也能快速实现常用功能；
+ 集成数十个稳定的[第三方开源库](https://github.com/Threeyes/AliveCursorSDK/wiki/Third-party-zh-CN)，方便实现更优秀的效果；
+ 支持热更新，可使用C#或VisualScripting编写逻辑；
+ 支持将各种参数（如基础数值、模型贴图、材质颜色等）暴露给用户，方便用户自定义Mod效果。

## 通过[OpenUPM](https://openupm.com/packages/com.threeyes.alivecursor.sdk/)安装
1. 安装[Git](https://git-scm.com/).
2. 使用[Unity2021.3.5f1](https://download.unity3d.com/download_unity/40eb3a945986/UnityDownloadAssistant-2021.3.5f1.exe)创建一个Windows版的空项目.
3. 下载最新的[manifest.json](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/manifest.json)文件并替换`[项目根目录]/Packages`路径下的同名文件。
4. 下载 [ProjectSetting](https://raw.githubusercontent.com/Threeyes/AliveCursorSDK/main/ProjectConfig~/ProjectSettings.zip)压缩文件，解压后覆盖`[项目根目录]`路径下的同名文件夹。
5. 打开该项目，确保`PackagesManager`窗口包含**唯一**的脚本编辑器，并且与`Preferences` 窗口-External Tools中的设置一致（如VisualStudio）：
![image](https://user-images.githubusercontent.com/13210990/180822147-5a917199-279f-4cbb-a073-32e5078e2709.png)


## 可能出现的错误
+ 如果第一次打开时出现错误，请尝试关闭程序，删掉`Library`文件夹然后重启项目。
+ 如果`Package/AliveCursorSDK`中的某个子文件夹为空，请在`Package Manager`窗口重新导入该包。

## 文档
请查阅[wiki](https://github.com/Threeyes/AliveCursorSDK/wiki/Home-zh-CN)。

## 联系方式
+ [QQ](https://im.qq.com/index/)群: 673880395

## 案例
为了发挥《AliveCursor》的潜力，我上传了不同类型的MOD到[创意工坊](https://steamcommunity.com/profiles/76561199378980403/myworkshopfiles/?appid=1606490)，你可以下载[AliveCursor_ModUploader](https://github.com/Threeyes/AliveCursor_ModUploader)项目用作参考。
<p align="center">
    <a href="https://github.com/Threeyes/AliveCursor_ModUploader"><img src="https://user-images.githubusercontent.com/13210990/195757514-014d8d7d-b0bf-438c-9e53-40300185e1a2.gif" alt="Item图片墙" width="600px" height="450px" />
    <br />
</p>    