Shader "Custom/URP/LatLongGridSphere"
{
    Properties
    {
        _Color ("Line Color", Color) = (0.3, 0.8, 1, 1)
        _Background ("Background Color", Color) = (0, 0, 0, 0.0)
        _LongitudeCount ("Longitude Lines", Float) = 12
        _LatitudeCount ("Latitude Lines", Float) = 8
        _LineWidth ("Line Width", Range(0.001,0.05)) = 0.01
        _Glow ("Glow", Range(0,5)) = 1
        _AxisColor("Axis Color", Color) = (1,0,0,1)
        _AxisWidth("Axis Width", Range(0.001,0.05)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color;
            float4 _Background;
            float _LongitudeCount;
            float _LatitudeCount;
            float _LineWidth;
            float _Glow;
            float4 _AxisColor;
            float _AxisWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.normal = normalize(worldNormal);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float LineMask(float value, float lineWidth)
            {
                return 1.0 - smoothstep(lineWidth, lineWidth * 1.5, abs(value));
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.normal);

                // Longitude angle (-PI ~ PI)
                float longitude = atan2(n.z, n.x); // horizontal axis
                // Latitude angle (-PI/2 ~ PI/2)
                float latitude = asin(n.y);

                // Normalize to 0~1
                float lon01 = (longitude + UNITY_PI) / (2 * UNITY_PI);
                float lat01 = (latitude + UNITY_PI * 0.5) / UNITY_PI;

                // Compute line interval
                float lonLine = frac(lon01 * _LongitudeCount);
                float latLine = frac(lat01 * _LatitudeCount);

                float lonMask = LineMask(lonLine - 0.5, _LineWidth);
                float latMask = LineMask(latLine - 0.5, _LineWidth);

                float grid = max(lonMask, latMask);

                // Glow
                float glow = pow(grid, _Glow);

                float4 lineColor = _Color * glow;

                // --- 新增南北极轴线 ---
                float axisDist = length(float2(n.x, n.z)); // 距离 Y 轴
                float axisMask = 1.0 - smoothstep(_AxisWidth, _AxisWidth * 1.5, axisDist);
                float4 axisColor = _AxisColor * axisMask;

                // 叠加：轴线优先显示
                float4 finalColor = max(lineColor, axisColor);

                return lerp(_Background, finalColor, max(grid, axisMask));
            }
            ENDCG
        }
    }
}