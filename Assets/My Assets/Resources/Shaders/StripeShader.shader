Shader "Custom/StripeShader" {
	Properties {
		_StripeColor ("StripeColor", Color) = (1,1,1,1)
		_NonStripeColor ("NonStripeColor", Color) = (1,1,1,1)
		_StripeTex ("Stripe texture (Greyscale)", 2D) = "white" {}
	}
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			#include "UnityCG.cginc"

			sampler2D _StripeTex;

			fixed4 _StripeColor;
			fixed4 _NonStripeColor;

			appdata_img vert(appdata_img input) {
				input.vertex = mul (UNITY_MATRIX_MVP, input.vertex);
				return input;
			}

			float4 frag (appdata_img input) : COLOR {
				float4 result = tex2D(_StripeTex, input.texcoord);
				fixed4 color = _StripeColor * result.r + _NonStripeColor * (1 - result.r);

				return color;
			}
			ENDCG
		}
	}
}
