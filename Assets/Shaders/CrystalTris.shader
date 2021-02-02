Shader "Unlit/CrystalTris"
{
    Properties
    {
        _VertexColorIntensity ("_VertexColorIntensity", Range(0, 5)) = 1.0
        [NoScaleOffset] _RampTex ("_RampTex", 2D) = "white" {}
        _RampIntensity ("_RampIntensity", Range(0, 5)) = 1.0
        _RampOffset ("_RampOffset", Range(-2, 2)) = .2
        _RampScale ("_RampScale", Range(0, 5)) = .2
        _RampRadialWeight ("_RampRadialWeight", Range(0, 2)) = 1
        _RampLinearWeight ("_RampLinearWeight", Range(0, 2)) = 1
        _RampViewDirWeight("_RampViewDirWeight", Range(0, 2)) = 1 
        
        _BaseColor ("BaseColor", Color) = (1,1,1,1)
        _DensityColor ("DensityColor", Color) = (1,1,1,1)
        _NoiseColor ("NoiseColor", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _NumSteps("numSteps",int) = 10
        _DeltaSize("DeltaSize",float) = .01
        _NoiseSize("NoiseSize", float) = 1
        _DensityFalloff("DensityFalloff", float) = 1
        _DensityRadius("DensityRadius", float) = 1
        _NoiseImportance("NoiseImportance", float) = 1
        _DensityImportance("DensityImportance", float) = 1
        _DensityRefractionMultiplier("DensityRefractionMultiplier", float) = 1
        _NoiseSharpness("NoiseSharpness",float) = 1
        _Opaqueness("_Opaqueness",float) = 1
        _EdgeDistance("_EdgeDistance", Range(0, 1)) = .1
        _EdgeThickness("_EdgeThickness", Range(0, .5)) = .1
    }

    SubShader
    {
        // Draw ourselves after all opaque geometry
        Tags {"Queue" = "Transparent"}

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass {"_BackgroundTexture"}

        Cull Off
        Pass
        {
            
            Blend SrcAlpha OneMinusSrcAlpha
            //Blend One One
            CGPROGRAM
          
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
         
            struct Vert{
                float3 pos;
                float debug;
            };

            uniform int _Count;
            uniform int _WhichFace;
            uniform float _Size;
            float _VertexColorIntensity;
            sampler2D _RampTex;
            float _RampIntensity;
            float _RampOffset;
            float _RampScale;
            float _RampLinearWeight;
            float _RampRadialWeight;
            float _RampViewDirWeight;
            float4 _BaseColor;
            float4 _DensityColor;
            float4 _NoiseColor;
            int _NumSteps;
            float _DeltaSize;
            float _NoiseSize;
            float _DensityFalloff;
            float _DensityRadius;
            float _NoiseImportance;
            float _DensityImportance;
            float _DensityRefractionMultiplier;
            float _NoiseSharpness;
            float _Opaqueness;

            float _EdgeDistance;
            float _EdgeThickness;

            StructuredBuffer<float4> _TriBuffer;
            StructuredBuffer<float3> _NormBuffer;

            // uniform float4x4 worldMat;

            // A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos : SV_POSITION;
                float uv : TEXCOORD0;
                float face : TEXCOORD40;
                float triID : TEXCOORD11;
                float3 nor : NORMAL;
                float3 ro : TEXCOORD12;
                float3 rd : TEXCOORD13;
                float3 eye : TEXCOORD4;
                float3 localPos : TEXCOORD10;
                float3 worldNor : TEXCOORD5;
                float3 lightDir : TEXCOORD6;
                float4 grabPos : TEXCOORD7;
                float uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
				float4 color : COLOR;
            };

            float4 _Color;
            uniform float4x4 _Transform;

            sampler2D _BackgroundTexture;

            struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
                float uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
				float4 color : COLOR;
            };

            // Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            // which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert ( appdata vertex )
            {

                varyings o;
                float4 p = vertex.position;
                float3 n =  vertex.normal;//_NormBuffer[id/3];

                float3 worldPos = mul (unity_ObjectToWorld, float4(p.xyz,1.0f)).xyz;
                o.pos = UnityObjectToClipPos (float4(p.xyz,1.0f));
                o.nor = n;//normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f)));; 
                o.face = p.w;
                o.ro = p;//worldPos.xyz;

                o.uv = vertex.uv;
                o.uv1 = vertex.uv1;
                o.uv2 = vertex.uv2;
                o.uv3 = vertex.uv3;
                o.color = vertex.color;
                
                o.localPos = p.xyz;


                float3 localP = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
                float3 eye = normalize(localP - p.xyz);

                o.rd = refract( eye , -n , .8);
                o.eye = refract( -normalize(_WorldSpaceCameraPos - worldPos) , normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f))), .8);
                //o.worldNor = mul (unity_ObjectToWorld, float4(n.xyz,0.0f)).xyz;
                o.worldNor = normalize(mul (unity_ObjectToWorld, float4(-n,0.0f)).xyz);
                o.lightDir = normalize(mul( unity_ObjectToWorld , float4(1,-1,0,0)).xyz);

                float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
                o.grabPos = ComputeGrabScreenPos(refractedPos);
                //o.triID = float(id)%3;
               return o;

            }

            float3 hsv(float h, float s, float v)
            {
                return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
                h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
            }  

            #ifndef __noise_hlsl_
            #define __noise_hlsl_
             
            // hash based 3d value noise
            // function taken from [url]https://www.shadertoy.com/view/XslGRr[/url]
            // Created by inigo quilez - iq/2013
            // License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
             
            // ported from GLSL to HLSL
             
            float hash( float n )
            {
                return frac(sin(n)*43758.5453);
            }
     
            float noise( float3 x )
            {
                // The noise function returns a value in the range -1.0f -> 1.0f

                float3 p = floor(x);
                float3 f = frac(x);

                f = f*f*(3.0-2.0*f);
                float n = p.x + p.y*57.0 + 113.0*p.z;

                return  lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
                        lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
                        lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                        lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
            }

            #endif 

            // Taken from https://www.shadertoy.com/view/4ts3z2
            float tri(in float x){return abs(frac(x)-.5);}
            float3 tri3(in float3 p){return float3( tri(p.z+tri(p.y*1.)), tri(p.z+tri(p.x*1.)), tri(p.y+tri(p.x*1.)));}
                    
            // Taken from https://www.shadertoy.com/view/4ts3z2
            float triNoise3D(in float3 p, in float spd )
            {
                float z=1.4;
	            float rz = 0.;
                float3 bp = p;
	            for (float i=0.; i<=1.; i++ )
	            {
                    float3 dg = tri3(bp*2.);
                    p += (dg+(_Time.y%100)*.1*spd);

                    bp *= 1.8;
		            z *= 1.5;
		            p *= 1.2;
                    //p.xz*= m2;
                    
                    rz+= (tri(p.z+tri(p.x+tri(p.y))))/z;
                    bp += 0.14;
	            }
	            return rz;
            }

            float t3D( float3 pos )
            {
                float3 fPos = pos * .05 ;

                // Adds Randomness to noise for each crystal
                // fPos += 100 * mul(unity_ObjectToWorld,float4(0,0,0,1)).xyz;
                return triNoise3D( fPos,0);
            }

            float dT3D( float3 pos , float3 lightDir )
            {
                float eps = .0001;
                return ((t3D(pos) - t3D(pos+ lightDir * eps))/eps+.5);
            }

            float3 nT3D( float3 pos )
            {
                float3 eps = float3(.0001,0,0);

                return t3D(pos) * normalize(
                     float3(  t3D(pos + eps.xyy) - t3D(pos - eps.xyy), 
                              t3D(pos + eps.yxy) - t3D(pos - eps.yxy),
                              t3D(pos + eps.yyx) - t3D(pos - eps.yyx) ));
            }

            float map(float value, float min1, float max1, float min2, float max2) {
                return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            float calcTriEdge(float input)
            {
                return 1 - smoothstep(_EdgeDistance, _EdgeDistance + _EdgeThickness, input);
            }

            float3 calcEdges(float4 uv1, float4 uv2)
            {
                float3 triEdge = max(max(calcTriEdge(uv2.x), calcTriEdge(uv2.y)), calcTriEdge(uv2.z));
                float3 polyEdge = (1 - smoothstep(1 - _EdgeDistance, 1 - (_EdgeDistance + _EdgeThickness), uv1).r);
                return 1 - min(polyEdge, triEdge);
            }

            //Pixel function returns a solid color for each point.
            float4 frag (varyings v) : COLOR
            {
                // float densityFalloff = _DensityFalloff * (noise(cos(_Time * 1.1) * .5) + .2);
                float densityFalloff = _DensityFalloff * ((cos(_Time * 10.0) * 1.5) + 1.8);
                // densityFalloff = densityFalloff + cos(_Time * 22) * 6;
                
                float rampIndex =
                (
                    ((v.eye + 1.0) * _RampViewDirWeight) +
                    (v.uv * _RampLinearWeight) +
                    (v.uv1 * _RampRadialWeight)
                    * _RampScale
                ) + _RampOffset;
                rampIndex = abs(rampIndex % 1);
                // )
                float3 baseColor = tex2D(_RampTex, rampIndex)  * _RampIntensity;
                baseColor += v.color * _VertexColorIntensity;
                // float3 color = tex2D(_RampTex, v.face) * _BaseColor;
                float3 col =0;//hsv( float(v.face) * .3 , 1,1);
                float dt = _DeltaSize;
                float t = 0;
                float c = 0.;
                float3 p = 0;

                float totalSmoke = 0;
                float3 rd = v.rd;
                for(int i =0 ; i < _NumSteps; i++ )
                {
                    t+=dt*exp(-2.*c);
                    p = v.ro - rd * t * 2;

                    float3 smoke = nT3D( p * _NoiseSize);
                    float3 nor = normalize(smoke);
                    float noiseDensity = length(smoke);
                    noiseDensity =   pow( noiseDensity , _NoiseSharpness)  * _NoiseImportance;
                    float centerOrbDensity = saturate(-(length(p * densityFalloff)-_DensityRadius))* _DensityImportance; + c * _NoiseImportance;

                    //float regularDensity = 
                    //c = saturate(1-abs(p.y * 10)) * c;
                    //c = centerOrbDensity;//(length(p)-3);// * .1 + c *1;
                    //c *= 1;
                    //  c = c * c* c* c* c * 50;
                    //  c =   c ;//aturate(-.4+abs(p.y * 10)) * c;
                    // c = c * c * c * c * c * c  * 600;
                    // c = (c*c*c*3);
                    //c *= c * c *c ;
                    //c = saturate(c);
                    totalSmoke += centerOrbDensity + c;
                    c = centerOrbDensity + noiseDensity;
                    rd = normalize(rd * (1-c*_DensityRefractionMultiplier) + nor *  c*_DensityRefractionMultiplier);
                    col = .99 * col +  lerp(lerp(_BaseColor, _NoiseColor , noiseDensity), baseColor , saturate(centerOrbDensity));// saturate(dot(v.lightDir , nor)) * .1 *c;//hsv(c,.4, dT3D(p*3,float3(0,-1,0))) * c;//hsv(c * .8 + .3,1,1)*c;;// hsv(smoke,1,1) * saturate(smoke);
                }

                // float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
                float4 refractedPos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(p,1)));
                float4 backgroundCol = tex2Dproj(_BackgroundTexture, refractedPos);

                col /= float(_NumSteps);
                //col = hsv(col.x * .1 + .3,1,1);// * (1-totalSmoke);
                //col = v.nor * .5 + .5;

                float m = dot( v.rd, v.nor );

                float3 baseCol =_BaseColor.xyz;
                col += baseCol;

                col = lerp(col*backgroundCol,col,saturate(totalSmoke * _Opaqueness));
                col += pow((1-m),5) * 60;

                float3 reflection=-v.eye;//normalize(reflect( normalize(-v.eye) , -v.worldNor));
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflection,0); //UNITY_SAMPLE_TEXCUBE_LOD('cubemap', 'sample coordinate', 'map-map level')
                //col = skyColor;

                //col = v.nor * .5 + .5;
                // col *= pow((1-m),5) * 60;
                // col += (v.nor * .5 + .5 ) * .4;

                float3 edges = calcEdges(v.uv1, v.uv2);
                float3 result = float3(col.xyz * edges.xyz);//saturate(float4(col,3*length(col) ));
                return float4(result * edges, 1);
            }

            ENDCG

        }
        
    }

    Fallback Off

}
