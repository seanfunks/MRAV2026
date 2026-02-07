// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MtShader"
{
	Properties
	{
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 17.3
		_Color0("Color 0", Color) = (0,0,0,0)
		_ScorllingSpeed("ScorllingSpeed", Float) = 6.22
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		[HDR]_Emission("_Emission", Color) = (34.29675,34.29675,34.29675,0)
		_BumpVal("BumpVal", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		Stencil
		{
			Ref 3
			Comp Greater
			Pass Keep
			Fail Zero
		}
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample2;
		uniform float _ScorllingSpeed;
		uniform float _BumpVal;
		uniform float4 _Color0;
		uniform float4 _Emission;
		uniform float _TessValue;

		float4 tessFunction( )
		{
			return _TessValue;
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 _uv = float2(1,1);
			float4 appendResult22 = (float4(_uv.x , ( _Time.y * _uv.y * _ScorllingSpeed ) , 0.0 , 0.0));
			float2 uv_TexCoord15 = v.texcoord.xy + appendResult22.xy;
			float4 temp_output_4_0 = ( saturate( tex2Dlod( _TextureSample2, float4( uv_TexCoord15, 0, 0.0) ) ) * _BumpVal );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( temp_output_4_0 * float4( ase_vertexNormal , 0.0 ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 _uv = float2(1,1);
			float4 appendResult22 = (float4(_uv.x , ( _Time.y * _uv.y * _ScorllingSpeed ) , 0.0 , 0.0));
			float2 uv_TexCoord15 = i.uv_texcoord + appendResult22.xy;
			float4 temp_output_4_0 = ( saturate( tex2D( _TextureSample2, uv_TexCoord15 ) ) * _BumpVal );
			o.Albedo = ( ( 1.0 - temp_output_4_0 ) * _Color0 ).rgb;
			o.Emission = _Emission.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.Vector2Node;17;-1434.913,-231.5293;Inherit;False;Constant;_uv;uv;5;0;Create;True;0;0;0;False;0;False;1,1;4.32,6.07;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;23;-1376.589,351.8365;Inherit;False;Property;_ScorllingSpeed;ScorllingSpeed;7;0;Create;True;0;0;0;False;0;False;6.22;-0.28;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;31;-1512.403,66.6492;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1301.537,144.6907;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;-1117.537,10.69073;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-1054.913,-191.5293;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;1.56,2.46;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;34;-865.6345,-387.1248;Inherit;True;Property;_TextureSample2;Texture Sample 2;8;0;Create;True;0;0;0;False;0;False;-1;None;e16f8e2dd5ea82044bade391afc45676;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;8;-578.3019,-244.472;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-654.233,138.4572;Inherit;False;Property;_BumpVal;BumpVal;11;0;Create;True;0;0;0;False;0;False;0;2.78;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-397.3644,-183.6877;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;7;-551.0023,311.9278;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;32;-270.489,-739.5084;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;25;-81.53363,-316.2616;Inherit;False;Property;_Color0;Color 0;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.8301887,0.8301887,0.8301887,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DeltaTime;30;-1645.004,108.2492;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosTime;24;-1644.843,302.1823;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-302.7026,132.528;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;9;-155.8021,-22.17203;Inherit;False;Property;_Emission;_Emission;10;1;[HDR];Create;True;0;0;0;False;0;False;34.29675,34.29675,34.29675,0;0.06603771,0.06603771,0.06603771,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;18;-105.6734,-604.8212;Inherit;True;Property;_MainTex;MainTex;9;0;Create;True;0;0;0;False;0;False;-1;None;0c2641f16f0c74dc8bd65cc0f24b03df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;405.1272,-720.5262;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;912fe0350b83a4934aa3799ef6fced00;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;209.5192,-356.8932;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VoronoiNode;1;-1169.302,-588.2323;Inherit;True;2;0;1;0;8;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;12.39;False;2;FLOAT;6.15;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;503.6001,-198.3;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;MtShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;True;3;False;;255;False;;255;False;;1;False;;1;False;;2;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;1;17.3;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;0;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;31;0
WireConnection;20;1;17;2
WireConnection;20;2;23;0
WireConnection;22;0;17;1
WireConnection;22;1;20;0
WireConnection;15;1;22;0
WireConnection;34;1;15;0
WireConnection;8;0;34;0
WireConnection;4;0;8;0
WireConnection;4;1;5;0
WireConnection;32;0;4;0
WireConnection;6;0;4;0
WireConnection;6;1;7;0
WireConnection;33;0;32;0
WireConnection;33;1;25;0
WireConnection;0;0;33;0
WireConnection;0;2;9;0
WireConnection;0;11;6;0
ASEEND*/
//CHKSM=A0CA6FDFE20E320B7E15E8FF8C744AE563099192