Shader "Custom/Mask"
{

	Properties {
		_StencilMask("Stencil mask", Int) = 0
	}


    SubShader {
        Tags { "RenderType"="Transparent" }
		ColorMask 0
		ZWrite off

		Stencil {
			Ref[_StencilMask]
			Comp always
			Pass replace
		}
 
        CGPROGRAM
        #pragma surface surf Lambert alpha
 
        struct Input {
            fixed3 Albedo;
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = fixed3(1, 1, 1);
            o.Alpha = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
