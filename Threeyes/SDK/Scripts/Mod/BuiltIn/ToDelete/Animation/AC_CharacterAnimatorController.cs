using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using Newtonsoft.Json;
using Threeyes.Config;
using Threeyes.Steamworks;

/// <summary>
/// Function：
/// -Play Character Animation base on movement info
/// -Random Animations using BlendTree
/// 
/// 实现原理：
/// -主BlendTree通过MoveSpeed参数控制Idle/Move的动画切换（中间可以按需求细化成Walk和Run），sub BlendTree通过RandomParamInfo（随机值参数）随机调用对应Bored的动画
/// Howto：
/// -为每个sub BlendTree设置对应的动画权重：在BlendTree设置界面中，先将第一个设置为0，最后一个设置为1，然后勾选[Automate Thresholds]
/// </summary>
[System.Obsolete("Use the class without AC_ prefix instead!", true)]
public class AC_CharacterAnimatorController : CharacterAnimatorController { }