Shader "Custom/SeenBehindShader" {
	Properties
	{
		_Color("RGBA", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Brightness("Brightness override", range(0,25.0)) = 0.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Ztest NotEqual

		CGPROGRAM
		#pragma surface surf Lambert alpha

		fixed4 _Color;
		sampler2D _MainTex;
		float _Brightness;

		struct Input
		{
			float2 uv_MainTex : TEXCOORD0;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 col = _Color * tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = col.rgb * _Brightness;
			o.Alpha = col.a;
		}
		ENDCG
	}
	Fallback "Diffuse" //if all the shader before cannot be run
}