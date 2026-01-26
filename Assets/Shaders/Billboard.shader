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
        ZWrite On
        Cull Off
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
                
                // Get the object's position in world space
                float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Get camera basis vectors (right, up, forward) in world space
                float3 camRight = UNITY_MATRIX_V[0].xyz;  // Camera right
                float3 camUp = UNITY_MATRIX_V[1].xyz;     // Camera up
                float3 camForward = UNITY_MATRIX_V[2].xyz; // Camera forward
                
                // Extract scale from object's world matrix
                // The scale is in the length of the basis vectors
                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                // Apply the object's scale to the vertex position
                // v.vertex.xy contains the quad's local coordinates (-0.5 to 0.5 for a unit quad)
                float3 scaledVertex = float3(v.vertex.x * objectScale.x, v.vertex.y * objectScale.y, 0);
                
                // Rotate the scaled vertex to face the camera
                // Use camera right/up vectors to build the billboard
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex += scaledVertex.y * camUp;
                
                // Transform to clip space
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.01);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
