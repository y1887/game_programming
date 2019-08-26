Shader "Custom/ChangePure"
{
	Properties
	{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		_FillColor("Fill Color", Color) = (1,1,1,0)
		_PickColor("Pick Color", Color) = (1,1,1,0)
		_ToColor("To Color", Color) = (1,1,1,0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment MySpriteFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			#include "UnitySprites.cginc"

			fixed4 _FillColor;
			fixed4 MySampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				color.a *= _FillColor.a;
				return color;
			}

			fixed4 _PickColor;
			fixed4 _ToColor;
			fixed4 MySpriteFrag(v2f IN) : SV_Target
			{
				fixed4 c = MySampleSpriteTexture(IN.texcoord) * IN.color;
				float eps = length(c.rgb - _PickColor.rgb);
				c.rgb = (step(0.01f, eps) * (_FillColor.rgb - _ToColor.rgb) + _ToColor.rgb) * (step(0.01f, eps) * (_FillColor.a - _ToColor.a) + _ToColor.a);
				return c;
			}
			ENDCG
		}
	}
}
