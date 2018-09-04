// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Water"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Transparency("Transparency", Range(0.0,0.8)) = 0.25
		_WaveAmp("Wave Amplitude", float) = 1

		_PointLightColor("Point Light Color", Color) = (0, 0, 0)
		_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)
		_fAtt("Attenuation", float) = 1
		_Ka("Ka", float) = 1
		_Ks("Ks", float) = 1
		_Shine("Shinyness", float) = 1
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
				float3 normal : NORMAL;
			};

			struct vertexOut
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertex : SV_POSITION;
				float3 worldVertex : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			float _Transparency;
			float _WaveAmp;

			float3 _PointLightColor;
			float3 _PointLightPosition;
			float _fAtt;
			float _Ka;
			float _Ks;
			float _Shine;

			vertexOut vert (vertexIn v)
			{
				vertexOut o;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.worldVertex = worldPos;
				o.normal = v.normal;

				o.vertex = UnityObjectToClipPos(v.vertex);
				// Manipulate the y coords to move in a wave form for both x and z
				o.vertex.y += (sin(worldPos.z+_Time.y) + cos(worldPos.x+_Time.y)) * _WaveAmp;
				// texture scale and offset are applied correctly to the uv
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}

			// Implementation of the fragment shader
			fixed4 frag(vertexOut v) : COLOR
			{
				float3 norm = normalize(v.normal);
				float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);


				float3 amb = UNITY_LIGHTMODEL_AMBIENT.rgb * _TintColor * _Ka;

				float3 L = normalize(_PointLightPosition.xyz - v.worldVertex.xyz);

				float3 diff = _fAtt * _PointLightColor.rgb * _TintColor * max(0.0, dot(norm, L));

				float3 specular;
				float3 R = reflect(-L, norm);

				if (dot(v.normal, L) < 0.0) {
					specular = float3(0, 0, 0);
				}
				else {
					specular = _fAtt * _PointLightColor.rgb * _Ks * pow(max(0.0, dot(R, L)), _Shine);
				}

				fixed3 col = (amb + diff) * tex2D(_MainTex, v.uv) + specular;

				return fixed4(col,_Transparency);
			}

			ENDCG
		}
	}
}
