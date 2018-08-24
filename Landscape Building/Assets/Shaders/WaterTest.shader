// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Water"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Transparency("Transparency", Range(0.0,0.8)) = 0.25
		_WaveAmp("Wave Amplitude", float) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct vertexIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct vertexOut
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			float _Transparency;
			float _WaveAmp;

			vertexOut vert (vertexIn v)
			{
				vertexOut o;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.vertex = UnityObjectToClipPos(v.vertex);
				// Manipulate the y coords to move in a wave form for both x and z
				o.vertex.y += (sin(worldPos.z+_Time.y) + cos(worldPos.x+_Time.y)) * _WaveAmp;
				// texture scale and offset are applied correctly to the uv
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (vertexOut i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) + _TintColor;
				// Add abit of transparency of the material
				col.a = _Transparency;
				return col;
			}

			ENDCG
		}
	}
}
