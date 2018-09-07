Shader "Custom/PhongShader"
{
	Properties
	{
		_PointLightColor("Point Light Color", Color) = (0, 0, 0)
		_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)
		_fAtt("Attenuation", float) = 1
		_Ka("Ka", float) = 1
		_Ks("Ks", float) = 1
		_Shine("Shinyness", float) = 0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float3 _PointLightColor;
			uniform float3 _PointLightPosition;
			uniform float _fAtt;
			uniform float _Ka;
			uniform float _Ks;
			uniform float _Shine;

			struct vertIn
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};

			struct vertOut
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				float3 worldVertex : TEXCOORD0;
			};

			// Implementation of the vertex shader
			vertOut vert(vertIn v)
			{
				vertOut o;

				o.worldVertex = mul(unity_ObjectToWorld, v.vertex);

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = v.color;
				o.normal = v.normal;

				return o;
			}

			// Implementation of the fragment shader
			fixed4 frag(vertOut v) : SV_Target
			{

				float3 amb = UNITY_LIGHTMODEL_AMBIENT.rgb * v.color.rgb * _Ka;

				float3 norm = normalize(v.normal);
				float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
				
				float3 L = normalize(_PointLightPosition.xyz - v.worldVertex.xyz);

				float3 diff = _fAtt * _PointLightColor.rgb * v.color.rgb * max(0.0, dot(norm, L));

				float3 specular;
				float3 R = reflect(-L, norm);

				if (dot(v.normal, L) < 0.0) {
					specular = float3(0, 0, 0);
				}
				else {
					specular = _fAtt * _PointLightColor.rgb * _Ks * pow(max(0.0, dot(R, L)), _Shine);
				}

				fixed3 col = amb + diff + specular;
				return fixed4(col,1.0);
			}
			ENDCG
		}
	}
}
