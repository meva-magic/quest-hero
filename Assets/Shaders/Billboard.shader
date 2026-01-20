Shader "Unlit/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On  // Important for proper depth sorting
        Cull Off   // We'll see both sides
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            v2f vert (appdata v)
            {
                v2f o;
                
                // Get camera position in object space
                float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Get camera forward and up vectors
                float3 forward = UNITY_MATRIX_V[2].xyz;  // Camera forward in world space
                float3 up = UNITY_MATRIX_V[1].xyz;       // Camera up in world space
                float3 right = UNITY_MATRIX_V[0].xyz;    // Camera right in world space
                
                // Create billboard rotation
                float3 worldVertex = worldPos;
                
                // Add the vertex position relative to the billboard center
                // v.vertex.x uses right vector, v.vertex.y uses up vector
                worldVertex += v.vertex.x * right * _MainTex_ST.x;
                worldVertex += v.vertex.y * up * _MainTex_ST.y;
                
                // Transform to clip space
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Discard fully transparent pixels for better depth sorting
                clip(col.a - 0.01);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
