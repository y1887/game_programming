Shader "Custom/CommonImageProcessing"
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
		_Hue("Hue", Range(0, 1.0)) = 0
		_Saturation("Saturation", Range(0, 1.0)) = 0.5
		_Brightness("Brightness", Range(0, 1.0)) = 0.5
		_Contrast("Contrast", Range(0, 1.0)) = 0.5
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

			fixed _Hue, _Saturation, _Brightness, _Contrast;

			fixed4 MySampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
			#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2D(_AlphaTex, uv);
				color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
			#endif
				return color;
			}

			inline float3 ApplyHue(float3 c, float h)  // See HSV color space
			{
				float angle = radians(h);
				float3 k = float3(0.57735, 0.57735, 0.57735);
				float cosAngle = cos(angle);
				return c * cosAngle + cross(k, c) * sin(angle) + k * dot(k, c) * (1 - cosAngle);
			}

			inline fixed4 ApplyHSBC(float4 c) 
			{
				float hue = 360 * _Hue;
				float saturation = _Saturation * 2;
				float brightness = _Brightness * 2 - 1;
				float contrast = _Contrast * 2;
				c.rgb = ApplyHue(c.rgb, hue);
				c.rgb = (c.rgb - 0.5f) * contrast + 0.5f;
				c.rgb = c.rgb + brightness;
				float3 intensity = dot(c.rgb, float3(0.2999, 0.587, 0.114));  // RGB to intensity (YCbCr)
				c.rgb = lerp(intensity, c.rgb, saturation);

				return c;
			}

			fixed4 MySpriteFrag(v2f IN) : SV_Target
			{
				fixed4 c = MySampleSpriteTexture(IN.texcoord) * IN.color;
				c = ApplyHSBC(c);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
}
