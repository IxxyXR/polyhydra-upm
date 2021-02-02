Shader "Unlit/Prismatic"
{
    Properties
    {
        _Color ("_Color", color) = (1,1,1,1)
        _BumpTex ("_BumpTex", 2D) = "white" {}
        [NoScaleOffset] _RampTex ("_RampTex", 2D) = "white" {}
        _Distance("_Distance", Range(-3, 3)) = 1
        _Strength("_Strength", Range(0, 5)) = 1
        _Edge1("_Edge1", Range(0, 1)) = .1
        _Edge2("_Edge2", Range(0, 1)) = .9
        _FEdge1("_FEdge1", Range(0, 1)) = .1
        _FEdge2("_FEdge2", Range(0, 1)) = .9
        _Falloff1("_Falloff1", Range(0, 50)) = 1
        _Falloff2("_Falloff2", Range(-10, 10)) = .1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                //float3 nor : NORMAL;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
            };

            struct v2f
            {
                //float3 wnor : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD4;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            sampler2D _RampTex;
            sampler2D _BumpTex;
            float4 _BumpTex_ST;

            float4 _Color;
            float _Distance;
            float _Strength;

            float _Edge1;
            float _Edge2;
            float _FEdge1;
            float _FEdge2;
            float _Falloff1;
            float _Falloff2;

            float map(float value, float min1, float max1, float min2, float max2) {
                return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            float calcTriEdge(float input)
            {
                return 1 - smoothstep(_Edge1, _Edge2, input);
            }

            float4 calcEdges(float4 uv1, float4 uv2)
            {
                float4 triEdge = max(max(calcTriEdge(uv2.x), calcTriEdge(uv2.y)), calcTriEdge(uv2.z));
                float4 polyEdge = (1 - smoothstep(1 - _Edge1, 1 - _Edge2, uv1).r);
                return 1 - min(polyEdge, triEdge);
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv1 = v.uv1;
                o.uv2 = v.uv2;
                o.uv3 = v.uv3;
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(wpos));
                //o.wnor = mul(unity_ObjectToWorld, v.nor).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                //Normal map
                float2 uvBump = TRANSFORM_TEX(i.uv, _BumpTex);
                float3 bump = UnpackNormal(tex2D(_BumpTex, uvBump));
                float3 wbump = normalize(mul(unity_ObjectToWorld, float4(bump, 0)));

                //Diffraction Grating from https://www.alanzucconi.com/2017/07/15/cd-rom-shader-2/
                // float3 L = dot(i.wnor, _WorldSpaceLightPos0.xyz);
                // float3 V = i.viewDir;
                // float3 T = wbump;
                // float cos_ThetaL = dot(L, T);
                // float cos_ThetaV = dot(V, T);
                // float u = abs(cos_ThetaL - cos_ThetaV);
                // float w = u * _Distance;

                float3 L = wbump;
                float3 V = i.viewDir;
                float Theta = dot(L, V);
                float u = abs(Theta);
                float w = u * ((pow(map(i.uv1.x, _FEdge1, _FEdge2, 0, 1), _Falloff1) * _Falloff2) + _Distance);

                //Rainbow color
                float3 prismastic =  tex2D(_RampTex, float2(w,0.5f));
                float4 result = _Color;
                result.rgb += prismastic * _Strength;
                float4 edges = calcEdges(i.uv1, i.uv2);
                return result * edges;
            }
            
            ENDCG
        }
    }
}
