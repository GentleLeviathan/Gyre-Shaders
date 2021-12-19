/*

MIT License

Copyright (c) 2021 GentleLeviathan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

Shader "GentleShaders/GyreEleven"
{
    Properties
    {
		//Primary Textures
        _MainTex ("ASG Control (RGBA)", 2D) = "white" {}
		_CC ("CC (RGBA)", 2D) = "red" {}
		_WearMask ("Wear Mask (RGBA)", 2D) = "black" {}
		[Normal]_BumpMap ("Normal Map (DX)", 2D) = "bump" {}

		//Main Colors
		_Color ("Primary Color", Color) = (1,1,1,1)
		_SecondaryColor("Secondary Color", Color) = (0,1,0,1)
		_UPrimaryColor("Undersuit Primary", Color) = (0,0,0,1)
		_USecondaryColor("Undersuit Secondary", Color) = (0,0,0,1)
		_UTertiaryColor("Undersuit Tertiary", Color) = (0,0,0,1)

		//Wear Colors
		[HDR]_IllumColor("Illumination Color", Color) = (0.643,1.0744,1.6862,0)
		_EdgeWearColor ("Edge Wear Color", Color) = (1,1,1,1)
		_DirtColor ("Dirt Color", Color) = (0.349,0.324,0.248,1)
		_GrungeColor ("Grunge Color", Color) = (0.462,0.423,0.377,1)

		//Main Sliders
        _Roughness ("Roughness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 1.0

		//Wear Sliders
		_EdgeWear ("Edge Wear Strength", Range(0,1)) = 0.5
		_Dirt ("Dirt Strength", Range(0,1)) = 1
		_Grunge ("Grunge Wear Strength", Range(0,1)) = 1
		_GrungeMagnitude ("Grunge Magnitude", Range(0.1, 3)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#include "../CgInc/GentleUtilities.cginc"
		#pragma target 4.0
		#pragma only_renderers d3d11 glcore gles
		#pragma surface surface GyreBRDF addshadow fullforwardshadows

		#pragma shader_feature_local _WEARMASK

		half4 LightingGyreBRDF(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize (lightDir + viewDir);
			half diff = max (0, dot (s.Normal, lightDir));
			half4 NdotL = dot(s.Normal, lightDir);
			half4 NdotH = dot(s.Normal, h);
			half4 NdotV = dot(s.Normal, viewDir);
			half4 VdotH = dot(viewDir, h);
			half4 LdotH = dot(lightDir, h);

			float specDist = BeckmannNDF(s.Gloss, NdotH) * 0.5;
			specDist += GGXNDF(s.Gloss, NdotH);

			float shadowDist = WalterEtAlGSF(NdotL, NdotV, s.Gloss);
			shadowDist = pow(shadowDist, 3);

			float fresnel = SphericalGFF(LdotH, s.Specular);

			half4 spec = (specDist * fresnel * shadowDist) / (1.0 * (  NdotL * NdotV));

			half3 hsvAlbedo = rgb2hsv(s.Albedo.rgb);
			half3 specMetalColor = hsv2rgb(half3(hsvAlbedo.r, hsvAlbedo.g, 1));
			spec = lerp(spec, spec * half4(specMetalColor, 1), s.Alpha);
			spec = saturate(spec);

			half4 c;
			c.rgb = (((s.Albedo + spec) * diff) * atten) * _LightColor0;
			c.a = 1;
			return c;
		}

        uniform sampler2D _MainTex;
		uniform sampler2D _CC;
		uniform sampler2D _WearMask;
		uniform sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
			half3 worldPos;
			half3 worldRefl; INTERNAL_DATA
        };

        UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _SecondaryColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _UPrimaryColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _USecondaryColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _UTertiaryColor)
			UNITY_DEFINE_INSTANCED_PROP(half4, _IllumColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _DirtColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _EdgeWearColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _GrungeColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _Roughness)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _Metallic)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _EdgeWear)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _Dirt)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _Grunge)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _GrungeMagnitude)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surface (Input i, inout SurfaceOutput o)
        {
            fixed4 control = tex2D (_MainTex, i.uv_MainTex);
			fixed4 cc = tex2D(_CC, i.uv_MainTex.xy);
			half4 normal = tex2D(_BumpMap, i.uv_MainTex);
			fixed edgeWear = control.g * UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeWear);
			fixed dirt = control.b * UNITY_ACCESS_INSTANCED_PROP(Props, _Dirt);

			half3 finalNormal = normalize(UnpackNormal(half4(normal.r, 1.0 - normal.g, normal.b, normal.a)));
			fixed diffuse = control.r;

			fixed roughness = (0.99 - (edgeWear * (1.0 - UNITY_ACCESS_INSTANCED_PROP(Props, _Roughness))));
			roughness += dirt * 2;

			//CC Masking
			fixed4 black = half4(0,0,0,0);
			fixed4 white = half4(1,1,1,0);

			fixed4 primary = lerp(black, diffuse * max(0.075, UNITY_ACCESS_INSTANCED_PROP(Props, _Color)), 1.0 - (cc.r + cc.g + cc.b));
			fixed4 secondary = lerp(black, diffuse * max(0.075, UNITY_ACCESS_INSTANCED_PROP(Props, _SecondaryColor)), cc.r);
			fixed4 uPrimary = lerp(black, diffuse * max(0.075, UNITY_ACCESS_INSTANCED_PROP(Props, _UPrimaryColor)), cc.g);
			fixed4 uSecondary = lerp(black, diffuse * max(0.075, UNITY_ACCESS_INSTANCED_PROP(Props, _USecondaryColor)), cc.b);
			
			fixed4 finalColor = primary + secondary + uPrimary + uSecondary;

			//Wear Application
			finalColor = lerp(finalColor, UNITY_ACCESS_INSTANCED_PROP(Props, _DirtColor), dirt * 0.99);
			finalColor = lerp(finalColor, UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeWearColor), edgeWear * 0.99);

			fixed metallic = (1.0 - (cc.r + cc.g + cc.b)) * UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic) * 0.99;
			metallic -= dirt;
			metallic += edgeWear;

			#ifdef _WEARMASK
				fixed4 wearMask = tex2D(_WearMask, i.uv_MainTex);
				fixed illum = wearMask.g;
				fixed grunge = pow(wearMask.b * UNITY_ACCESS_INSTANCED_PROP(Props, _Grunge), UNITY_ACCESS_INSTANCED_PROP(Props, _GrungeMagnitude));
				roughness += grunge;
				metallic -= grunge;

				finalColor = lerp(finalColor, UNITY_ACCESS_INSTANCED_PROP(Props, _GrungeColor), grunge * 0.99);
				finalColor = lerp(finalColor, diffuse * max(0.075, UNITY_ACCESS_INSTANCED_PROP(Props, _UTertiaryColor)), wearMask.r);
			#endif

			metallic = saturate(metallic);
			roughness *= UNITY_ACCESS_INSTANCED_PROP(Props, _Roughness);
			roughness = saturate(roughness);

			//Reflections
				//setup
			half3 reflectDir = WorldReflectionVector(i, finalNormal);
			half3 boxProjectionDir = BoxProjectedCubemapDirection(reflectDir + 0.001, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
			Unity_GlossyEnvironmentData envData; envData.roughness = roughness; envData.reflUVW = boxProjectionDir;
				//probe blending
			half3 skyColor = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
			half3 skyColor2 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
			skyColor = lerp(skyColor2, skyColor, unity_SpecCube0_BoxMin.w);
				//aesthetic
			skyColor *= diffuse;
			skyColor = max(half3(0,0,0), skyColor * 1);
			skyColor *= lerp(Desaturate(finalColor.xyz), finalColor.xyz, metallic);

			//Output
            o.Albedo = finalColor.rgb * (1.0 - metallic);
			o.Normal = finalNormal;
            o.Specular = finalColor;
			o.Emission = skyColor;
            o.Gloss = roughness;
            o.Alpha = metallic;
			#ifdef _WEARMASK
				o.Emission += illum * UNITY_ACCESS_INSTANCED_PROP(Props, _IllumColor);
			#endif
        }
        ENDCG
    }
    FallBack "Diffuse"
	CustomEditor "GentleShaders.Gyre.GyreElevenEditor"
}
