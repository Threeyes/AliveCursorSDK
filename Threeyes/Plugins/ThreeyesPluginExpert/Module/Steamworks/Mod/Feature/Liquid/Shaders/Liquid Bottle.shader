//Ref from:
//https://www.patreon.com/posts/quick-game-art-18245226
//https://pastebin.com/6EXJHAgA
//https://pastebin.com/ppbzx7mn
Shader "Threeyes/SpecialFX/Liquid Bottle"
{
	Properties
	{
		////——Model (Setup by Script)——
		//_PosOffset("Pos Offset", Float) = 0.0//修复物体坐标的位移（从[最低值，最高值]变为[-X,X]）
		//_PosScale("Pos Scale", Float) = 1.0//修复物体坐标的缩放(基于上值，结果=0.5/X)
		//_GlobalScale("Global Scale", Float) = 1.0//物体的GlobalScale（需要乘以物体导入时的缩放）	

		////——Motion——
		//_FillAmount("Fill Amount", Range(0, 1)) = 0.5//以物体的锚点为中心，数值是世界坐标的位移，那Fill Amount就刚好一一匹配，否则需要_PosOffset进行偏移
		//_WobbleX("WobbleX", Range(-1,1)) = 0.0
		//_WobbleZ("WobbleZ", Range(-1,1)) = 0.0

		//_MainTex("Texture", 2D) = "white" {}
		//[HDR]_Color ("Color", Color) = (1,1,1,1)//Body Color
		//[HDR]_RimColor("Rim Color", Color) = (1,1,1,1)//Border Color
		_RimPower("Rim Power", Range(0,10)) = 0.0
		//[HDR]_TopColor("Top Color", Color) = (1,1,1,1)
		//[HDR]_FoamColor("Foam Line Color", Color) = (1,1,1,1)
		//_FoamLineWidth("Foam Line Width", Range(0,1)) = 0.0

		_BottleWidth("BottleWidth",Range(0,1)) = 0.2
		_BottleColor("BottleColor",Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags {"Queue" = "Geometry"  "DisableBatching" = "True" }

		//BottleEffect
		Pass
		{
			//Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 viewDir : COLOR;
				float3 normal :NORMAL;
			};

			float4 _BottleColor;
			float _BottleWidth;

			//float4 _RimColor;
			float _RimPower;

			v2f vert(appdata v)
			{
				v2f o;
				v.vertex.xyz += v.normal.xyz * _BottleWidth;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.normal = v.normal.xyz;
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));

				return o;
			}

			fixed4 frag(v2f i,fixed facing : VFace) : SV_Target
			{
				// sample the texture
				fixed4 col = _BottleColor;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				float dotProduct = 1 - pow(dot(i.normal, i.viewDir),_RimPower);//1-pow(dot(i.normal, i.viewDir),_RimPower/10);
				float4 RimResult = _BottleColor * smoothstep(0.2,1,dotProduct);

				return RimResult;
			}
			ENDCG
		}

		////WaterEffect
		//Pass
		//{
		//	Zwrite On
		//	Cull Off // we want the front and back faces
		//	AlphaToMask On // transparency

		//	CGPROGRAM

		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// make fog work
		//	#pragma multi_compile_fog

		//	#include "UnityCG.cginc"

		//	struct appdata
		//	{
		//		float4 vertex : POSITION;
		//		float2 uv : TEXCOORD0;
		//		float3 normal : NORMAL;
		//	};

		//	struct v2f
		//	{
		//		float2 uv : TEXCOORD0;
		//		UNITY_FOG_COORDS(1)
		//		float4 vertex : SV_POSITION;
		//		float3 viewDir : COLOR;
		//		float3 normal : COLOR2;
		//		float fillEdge : TEXCOORD2;//Range: [-0.5f, 0.5f]
		//	};

		//	sampler2D _MainTex;
		//	float4 _MainTex_ST;
		//	float _GlobalScale, _FillAmount, _PosOffset, _PosScale,  _WobbleX, _WobbleZ;
		//	float4  _Color, _RimColor, _TopColor, _FoamColor;
		//	float _FoamLineWidth, _RimPower;

		//	float4 RotateAroundYInDegrees(float4 vertex, float degrees)
		//	{
		//		float alpha = degrees * UNITY_PI / 180;
		//		float sina, cosa;
		//		sincos(alpha, sina, cosa);
		//		float2x2 m = float2x2(cosa, sina, -sina, cosa);
		//		return float4(vertex.yz , mul(m, vertex.xz)).xzyw;
		//	}


		//	v2f vert(appdata v)
		//	{
		//		v2f o;

		//		o.vertex = UnityObjectToClipPos(v.vertex);
		//		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		//		UNITY_TRANSFER_FOG(o,o.vertex);
		//		// get world position of the vertex
		//		float3 worldPos = mul(unity_ObjectToWorld, v.vertex.xyz);
		//		// rotate it around XY
		//		float3 worldPosX = RotateAroundYInDegrees(float4(worldPos,0),360);
		//		// rotate around XZ
		//		float3 worldPosZ = float3 (worldPosX.y, worldPosX.z, worldPosX.x);
		//		// combine rotations with worldPos, based on sine wave from script
		//		float3 worldPosAdjusted = worldPos + (worldPosX * _WobbleX) + (worldPosZ * _WobbleZ);
		//		// how high up the liquid is
		//		//PS：该值范围 [-0.5f, 0.5f]，为液体显示区域)，因此所有物体的坐标都应该映射到该值：先将局部顶点坐标乘以缩放转回物体世界坐标，然后将其重映射到[-0.5f, 0.5f]
		//		o.fillEdge = (worldPosAdjusted.y / _GlobalScale + _PosOffset) * _PosScale + (1 - _FillAmount);

		//		o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
		//		o.normal = v.normal;
		//		return o;
		//	}

		//	fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
		//	{
		//		// sample the texture
		//		fixed4 col = tex2D(_MainTex, i.uv) * _Color;
		//		// apply fog
		//		UNITY_APPLY_FOG(i.fogCoord, col);

		//		// rim light
		//		float dotProduct = 1 - pow(dot(i.normal, i.viewDir), _RimPower);
		//		float4 RimResult = smoothstep(0.5, 1.0, dotProduct);
		//		RimResult *= _RimColor;

		//		// foam edge（泡沫边缘）
		//		float4 foam = (step(i.fillEdge, 0.5) - step(i.fillEdge, (0.5 - _FoamLineWidth)));
		//		float4 foamColored = foam * (_FoamColor * 0.9);
		//		// rest of the liquid
		//		float4 result = step(i.fillEdge, 0.5) - foam;
		//		float4 resultColored = result * col;
		//		// both together, with the texture
		//		float4 finalResult = resultColored + foamColored;
		//		finalResult.rgb += RimResult;

		//		// color of backfaces/ top
		//		float4 topColor = _TopColor * (foam + result);
		//		//VFACE returns positive for front facing, negative for backfacing
		//		return facing > 0 ? finalResult : topColor;
		//	}
		//	ENDCG
		//}
	}
}