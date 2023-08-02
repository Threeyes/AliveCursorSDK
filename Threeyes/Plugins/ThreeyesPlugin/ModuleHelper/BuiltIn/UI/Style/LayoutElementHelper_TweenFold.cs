using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 通过Tween折叠元素
/// </summary>
public class LayoutElementHelper_TweenFold : MonoBehaviour
{
	//#Config
	public Selectable selectable;//eg: Toggle
	public LayoutElement layoutElement;
	public float tweenEndValue = 300;
	public float tweenDuration = 0.5f;
	public Ease easeExpand = Ease.OutBack;
	public Ease easeFold = Ease.OutSine;

	public bool hideOnFold = true;

	//#Runtime
	Tween tween;
	private void Awake()
	{
		if (selectable is Toggle toggle)
		{
			toggle.onValueChanged.AddListener((b) => OnUpdateVisuals(b));
		}
	}
	public void OnUpdateVisuals(bool isExpand)
	{

		//ToUpdate:增加Flag枚举，决定需要进行变换的字段
		if (tween != null)
		{
			tween.Kill(false);
		}

		if (isExpand)
			layoutElement.gameObject.SetActive(true);
		tween = DOTween.To(
			() => layoutElement.preferredWidth,
			(f) => layoutElement.preferredWidth = f, isExpand ? tweenEndValue : 0, tweenDuration).SetEase(isExpand ? easeExpand : easeFold);
		tween.onComplete += () =>
		  {
			  if (!isExpand)
				  layoutElement.gameObject.SetActive(false);
		  };
	}
}
