Shader "SupGames/Mobile/BloomURP"
{
	Properties
	{
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
	}
	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	TEXTURE2D_X(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D_X(_BlurTex);
	SAMPLER(sampler_BlurTex);

	half4 _MainTex_TexelSize;
	half _BlurAmount;
	half4 _BloomColor;
	half _BloomAmount;
	half _Threshold;


	struct appdata {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2fb {
		half4 pos : SV_POSITION;
		half4 uv : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	struct v2f {
		half4 pos : SV_POSITION;
		half2 uv  : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata i)
	{
		v2f o = (v2f)0;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, half4(i.pos.xyz, 1.0h)));
		o.uv = i.uv;
		return o;
	}

	v2fb vertBlur(appdata i)
	{
		v2fb o = (v2fb)0;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, half4(i.pos.xyz, 1.0h)));
		half2 offset = _MainTex_TexelSize.xy * _BlurAmount;
		o.uv = half4(i.uv - offset, i.uv + offset);
		return o;
	}

	half4 fragBloom(v2fb i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.xy));
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.xw));
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.zy));
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.zw));
		c *= 0.25h;
		half br = max(c.r, max(c.g, c.b));
		half a = max(0.0h, br - _Threshold) / max(br, 0.00001h);
		return c * a;
	}

	half4 fragBlur(v2fb i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 result = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.xy));
		result += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.xw));
		result += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.zy));
		result += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.zw));
		return result * 0.25h;
	}

	half4 frag(v2f i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
		half4 b = SAMPLE_TEXTURE2D_X(_BlurTex, sampler_BlurTex, UnityStereoTransformScreenSpaceTex(i.uv)) * _BloomColor * _BloomAmount;
		return c + b;
	}

	ENDHLSL

	Subshader
	{
		Pass //0
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBloom
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDHLSL
		}

		Pass //1
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBlur
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDHLSL
		}

		Pass //2
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDHLSL
		}
	}
	Fallback off
}