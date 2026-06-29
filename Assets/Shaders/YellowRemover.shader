Shader "UI/YellowRemover_KeepVibrant"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _YellowCenter ("Yellow Center (Hue)", Range(0.0, 1.0)) = 0.166
        _Sensitivity ("Yellow Sensitivity", Range(0.0, 0.5)) = 0.08
        _Smoothness ("Smoothness", Range(0.0, 0.5)) = 0.04
        _DesatAmount ("Desaturation Amount", Range(0.0, 1.0)) = 1.0
        
        // НОВЫЙ ПАРАМЕТР: Максимальная насыщенность для удаления
        _MaxSaturation ("Max Saturation to Remove", Range(0.0, 1.0)) = 0.5
        _SatSmoothness ("Saturation Smoothness", Range(0.0, 0.3)) = 0.1
        
        // Обязательные параметры для поддержки UI Масок
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _YellowCenter;
            float _Sensitivity;
            float _Smoothness;
            float _DesatAmount;
            float _MaxSaturation;
            float _SatSmoothness;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o); 
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                float3 rgb = col.rgb;
                float maxVal = max(rgb.r, max(rgb.g, rgb.b));
                float minVal = min(rgb.r, min(rgb.g, rgb.b));
                float delta = maxVal - minVal;

                float hue = -1.0;
                float saturation = 0.0;

                if (delta > 0.0001) 
                {
                    saturation = maxVal > 0.0 ? delta / maxVal : 0.0;

                    if (maxVal == rgb.r)
                        hue = (rgb.g - rgb.b) / delta + (rgb.g < rgb.b ? 6.0 : 0.0);
                    else if (maxVal == rgb.g)
                        hue = (rgb.b - rgb.r) / delta + 2.0;
                    else
                        hue = (rgb.r - rgb.g) / delta + 4.0;
                    
                    hue /= 6.0;
                }

                float yellowMask = 0.0;
                if (hue >= 0.0)
                {
                    // 1. Маска по цветовому тону (попадаем ли в желтый)
                    float hueDiff = abs(hue - _YellowCenter);
                    if (hueDiff > 0.5) 
                        hueDiff = 1.0 - hueDiff;

                    yellowMask = smoothstep(_Sensitivity + _Smoothness, _Sensitivity, hueDiff);
                    
                    // 2. Игнорируем чистый белый/серый шум (низкий порог)
                    yellowMask *= smoothstep(0.03, 0.08, saturation);

                    // 3. ИЗМЕНЕНИЕ: Игнорируем слишком сочные цвета (верхний порог)
                    // Если saturation больше _MaxSaturation, этот шаг плавно сбросит маску в 0
                    float satMask = smoothstep(_MaxSaturation + _SatSmoothness, _MaxSaturation, saturation);
                    yellowMask *= satMask;
                }

                // Вычисляем черно-белую яркость
                half luminance = dot(col.rgb, half3(0.2126, 0.7152, 0.0722));
                half3 targetColor = half3(luminance, luminance, luminance);
                
                // Смешиваем исходный цвет и обесцвеченный
                float finalBlend = yellowMask * _DesatAmount;
                col.rgb = lerp(col.rgb, targetColor, finalBlend);
                
                return col;
            }
            ENDCG
        }
    }
}
