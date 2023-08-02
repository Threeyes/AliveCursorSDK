using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_VFX
using UnityEngine.VFX;
#endif

public class VisualEffectHelper :
#if USE_VFX
	ComponentHelperBase<VisualEffect>
#else
    MonoBehaviour
#endif
{
	public string propertyName;

	////ToAdd:通过属性暴露对应的值，方便PD等回调
	//public int IntPropertyValue { get { return Comp.GetInt(propertyName); } set { Comp.SetInt(propertyName, value); } }

#if USE_VFX
	//——Set Property——
	public void SetInt(int value)
	{
		Comp.SetInt(propertyName, value);
	}
	public void SetUInt(uint value)
	{
		Comp.SetUInt(propertyName, value);
	}
	public void SetFloat(float value)
	{
		Comp.SetFloat(propertyName, value);
	}
	public void SetVector2(Vector2 value)
	{
		Comp.SetVector2(propertyName, value);
	}
	public void SetVector3(Vector3 value)
	{
		Comp.SetVector3(propertyName, value);
	}
	public void SetVector4(Vector4 value)
	{
		Comp.SetVector4(propertyName, value);
	}
	public void SetColor(Color value)//PS:会将Color转为Vector4
	{
		Comp.SetVector4(propertyName, value);
	}
	public void SetMatrix4x4(Matrix4x4 value)
	{
		Comp.SetMatrix4x4(propertyName, value);
	}
	public void SetTexture(Texture value)
	{
		Comp.SetTexture(propertyName, value);
	}
	public void SetAnimationCurve(AnimationCurve value)
	{
		Comp.SetAnimationCurve(propertyName, value);
	}
	public void SetGradient(Gradient value)
	{
		Comp.SetGradient(propertyName, value);
	}
	public void SetMesh(Mesh value)
	{
		Comp.SetMesh(propertyName, value);
	}
	public void SetSkinnedMeshRenderer(SkinnedMeshRenderer value)
	{
		Comp.SetSkinnedMeshRenderer(propertyName, value);
	}
	public void SetGraphicsBuffer(GraphicsBuffer value)
	{
		Comp.SetGraphicsBuffer(propertyName, value);
	}
	public void SetBool(bool value)
	{
		Comp.SetBool(propertyName, value);
	}

	public void Play(bool isPlay)
	{
		if (isPlay)
			Comp.Play();
		else
			Comp.Stop();
	}
#endif
}
