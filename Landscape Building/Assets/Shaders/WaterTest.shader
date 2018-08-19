Shader "Unlit/WaterTest"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Smooth("Smoothening", float) = 1
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			float _Smooth;

			v2f vert (appdata v)
			{
				v2f o;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				float noise = tex2Dlod(_NoiseTex, float4(v.uv,0,_Smooth));

				o.vertex = UnityObjectToClipPos(v.vertex);
				// Manipulate the y coords to move in a wave form for both x and z
				o.vertex.y += sin(worldPos.z+(_Time.w*noise))/2 - cos(worldPos.x+(_Time.w*noise))/2;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) + _TintColor;

				return col;
			}

			ENDCG
		}
	}
}
