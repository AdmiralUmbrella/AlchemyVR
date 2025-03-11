Shader "Custom/AlchemyLiquid"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PotionColor("Potion Color", Color) = (1,1,1,1)
        _ColorBlend("Color Blend", Range(0,1)) = 0
        _Emission("Emission", Range(0,5)) = 1
        _Distortion("Distortion", Range(0,0.2)) = 0.1
        _RimPower("Rim Power", Range(0,5)) = 2
        _WaveSpeed("Wave Speed", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Unlit alpha:fade vertex:vert

        #include "UnityCG.cginc"

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
        };

        fixed4 _BaseColor;
        fixed4 _PotionColor;
        float _ColorBlend;
        float _Emission;
        float _Distortion;
        float _RimPower;
        float _WaveSpeed;

        void vert(inout appdata_full v)
        {
            // Animación de onda suave
            float wave = sin(_Time.y * _WaveSpeed + v.vertex.x * 3) * 0.1;
            v.vertex.y += wave * _Distortion;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Mezcla entre color base y color de poción
            fixed4 finalColor = lerp(_BaseColor, _PotionColor, _ColorBlend);

            // Efecto Fresnel para brillo en bordes
            float rim = 1 - saturate(dot(IN.viewDir, IN.worldNormal));
            rim = pow(rim, _RimPower) * _Emission;

            o.Albedo = finalColor.rgb;
            o.Emission = finalColor.rgb * rim;
            o.Alpha = finalColor.a;
        }

        half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo + s.Emission, s.Alpha);
        }
        ENDCG
    }
    FallBack "Diffuse"
}