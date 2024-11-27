Shader "Custom/HealthBar"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BgColor("Bg Color", Color) = (1, 1, 1, 1)
        _FillColor("Fill Color", Color) = (1, 1, 1, 1)
        _MarginX("Margin X", Range(0, 1)) = 0.1
        _MarginY("Margin Y", Range(0, 1)) = 0.1
        _RoundedRadius("Rounded Radius", Range(0, 1)) = 0.1
        _Alpha("Alpha", Range(0, 1)) = 1
        _Value("Value", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float fillVal : FILL_VALUE;
                float alpha : ALPHA_BLEND;
                fixed4 fillCol : FILL_COLOR;
                // If you need instance data in the fragment shader, uncomment next line
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BgColor;
            float _MarginX, _MarginY;
            float _RoundedRadius;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Value)
            UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _FillColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            void Unity_RoundedRectangle_float(float2 UV, float Width, float Height, float Radius, out float Out)
            {
                Radius = max(min(min(abs(Radius * 2), abs(Width)), abs(Height)), 1e-5);
                float2 uv = abs(UV * 2 - 1) - float2(Width, Height) + Radius;
                float d = length(max(0, uv)) / Radius;
                Out = saturate((1 - d) / fwidth(d));
            }

            v2f vert(appdata v)
            {
                v2f o; // Declare instance of v2f structure
                UNITY_SETUP_INSTANCE_ID(v);

                // If you need instance data in the fragment shader, uncomment next line
                //UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x, v.vertex.y, 0, 0) * float4(1.7, 0.2, 1.0, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.fillVal = UNITY_ACCESS_INSTANCED_PROP(Props, _Value);
                o.alpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
                o.fillCol = UNITY_ACCESS_INSTANCED_PROP(Props, _FillColor);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Could access instanced data here too like:
                // UNITY_SETUP_INSTANCE_ID(i);
                // UNITY_ACCESS_INSTANCED_PROP(Props, _Foo);
                // But, remember to uncomment lines flagged above

                float alpha = 1;
                bool roundedRectangle = (_RoundedRadius > 0);

                if (roundedRectangle)
                    Unity_RoundedRectangle_float(i.uv, 1, 1, _RoundedRadius, alpha);

                if (alpha <= 0)
                    discard;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _BgColor;

                if (i.uv.x > _MarginX && i.uv.x < 1 - _MarginX &&
                    i.uv.y > _MarginY && i.uv.y < 1 - _MarginY &&
                    i.uv.x / (1 - _MarginX * 2) - _MarginX < i.fillVal) {

                    if (roundedRectangle)
                        Unity_RoundedRectangle_float(i.uv, 1 - _MarginX * 2, 1 - _MarginY * 2, _RoundedRadius, alpha);
                    else
                        alpha = 1;

                    if (alpha > 0)
                        return lerp(col, i.fillCol, i.fillCol.a) * i.alpha;
                }

                return col * i.alpha;
            }
            ENDCG
        }
    }
}
