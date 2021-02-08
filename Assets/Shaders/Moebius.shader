//Moebius transformations in 3d, by reverse stereographic projection to the 3-sphere,
//rotation in 4d space, and projection back.
//by Daniel Piker 09/08/20
//Feel free to use, adapt and reshare. I'd appreciate a mention if you post something using this.
//You can also now find this transformation as a component in Grasshopper/Rhino
//I first wrote about these transformations here:
//https://spacesymmetrystructure.wordpress.com/2008/12/11/4-dimensional-rotations/

Shader "Custom/Moebius"
{
    Properties
    {

        _Amount ("Amount", Range(-6.283,6.283)) = 0
        _p ("p", Range(-6.283,6.283)) = 0
        _q ("q", Range(-6.283,6.283)) = 1.0

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Saturation ("Saturation", Range(0,4)) = 1.0


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        float _Amount;
        float _p;
        float _q;

        struct Input
        {
            float2 uv_MainTex;
            float3 vertexColor;
        };


        // Surface shader vertex function
        // appdata_full contains also vertices' color
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 Pt = v.vertex.xyz;

            float xa = Pt.x;
            float ya = Pt.y;
            float za = Pt.z;

            //reverse stereographic projection to hypersphere
            float xb = 2 * xa / (1 + xa * xa + ya * ya + za * za);
            float yb = 2 * ya / (1 + xa * xa + ya * ya + za * za);
            float zb = 2 * za / (1 + xa * xa + ya * ya + za * za);
            float wb = (-1 + xa * xa + ya * ya + za * za) / (1 + xa * xa + ya * ya + za * za);

            //rotate hypersphere by amount t

            float xc = xb * cos(_p*_Amount) + yb * sin(_p*_Amount);
            float yc = (-1.0 * xb) * sin(_p*_Amount) + yb * cos(_p*_Amount);
            float zc = zb * cos(_q*_Amount) - wb * sin(_q*_Amount);
            float wc = zb * sin(_q*_Amount) + wb * cos(_q*_Amount);

            //project stereographically back to flat 3D
            float xd = xc / (1 - wc);
            float yd = yc / (1 - wc);
            float zd = zc / (1 - wc);

            //the transformed point
            v.vertex.xyz = float3(xd, yd, zd);



            o.vertexColor = v.color;
        }


        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Saturation;


        // Convert image to grayscale, according to FCC standards.
        float Bw(float3 col)
        {
            return float(col.r * 0.299 + col.g * 0.587 + col.b * 0.114);
        }


        float3 Saturation(float3 col)
        {
            return lerp(Bw(col.rgb), col.rgb, _Saturation);
        }


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        // Surface program
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            float3 vertexColor = Saturation(IN.vertexColor);
            c.rgb *= vertexColor;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}