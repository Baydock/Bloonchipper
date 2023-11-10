Shader "Outlined/Unlit" {
	Properties {
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_SelectColor("Select Color", Color) = (1,1,1,1)
		_Outline("Outline width", Range(0.0, 3)) = 10
		[Toggle] _Selected("Selected", Float) = 1
		_MainTex("Base (RGB)", 2D) = "white" { }
		[HideInInspector] _Bloonchipper("Identifier", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
	}

	SubShader {
		Tags { "QUEUE" = "Geometry" "RenderType" = "Outline" }

		// Outline
		Pass {
			Tags { "QUEUE" = "Geometry" "RenderType" = "Outline" }
			Cull Off
			ZWrite Off
			ZTest [_ZTest]

			ColorMask RGB // alpha not used

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 pos : POSITION;
					float4 color : COLOR;
				};

				uniform float _Outline;
				uniform float _Selected;
				uniform float4 _OutlineColor;
				uniform float4 _SelectColor;

				v2f vert(appdata v) {
					// just make a copy of incoming vertex data but scaled according to normal direction
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);

					float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					float2 offset = TransformViewToProjection(norm.xy);

					float w = _Outline;
					float4 c = _OutlineColor;
					if (_Selected) {
						w = _Outline * (4.0 / 3.0);
						c = _SelectColor;
					} else {
						w = _Outline;
						c = _OutlineColor;
					}
					o.pos.xy += offset * o.pos.z * w;
					o.color = c;
					return o;
				}

				half4 frag(v2f i) :COLOR {
					return i.color;
				}
			ENDCG
		}

		// Unlit
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			ZWrite On
			ZTest LEqual
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
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv);
					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}

			ENDCG

			Lighting On

			SetTexture[_MainTex]{
				Combine previous * primary DOUBLE
			}
		}
	}
	Fallback "Outline/Standard"
}