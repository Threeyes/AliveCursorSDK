//功能：自身隐藏，但可以遮挡后面的物体，适用于全景图游览场景中，用隐形墙遮挡后面的DP
//参考：https://forum.unity.com/threads/blocking-invisible-object.466313/
//类似效果：VR/SpatialMapping/Occlusion.shader
Shader "Colyu/InvisibleOccluder" {
	SubShader{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" }
		Pass {
			Blend Zero One
		}
	}
}