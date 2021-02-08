Shader "FantasyCrystals/AndyB"
{

    Properties
    {

        _BaseColor ("BaseColor", Color) = (1,1,1,1)
        
        _NumSteps("Num Trace Steps",int) = 10
        _DeltaStepSize("DeltaStepSize",float) = .01
        _StepRefractionMultiplier("StepRefractionMultiplier", float) = 0

        _ColorMultiplier("ColorMultiplier",float)=1

        _Opaqueness("_Opaqueness",float) = 1
        _IndexOfRefraction("_IndexOfRefraction",float) = .8
        _RefractionBackgroundSampleExtraStep("_RefractionBackgroundSampleExtraStep",float) = 0

        _ReflectionColor ("ReflectionColor", Color) = (1,1,1,1)
        _ReflectionSharpness("ReflectionSharpness",float)=1
        _ReflectionMultiplier("_ReflectionMultiplier",float)=1

        _CenterOrbOffset ("CenterOrbOffset", Vector) = (0,0,0)
        _CenterOrbColor ("CenterOrbColor", Color) = (1,1,1,1)
        _CenterOrbFalloff("CenterOrbFalloff", float) = 6
        _CenterOrbFalloffSharpness("CenterOrbFalloffSharpness", float) = 1

        _CenterOrbImportance("CenterOrbImportance", float) = .3

        _Truth ("Truth", Vector) = (0,0,0)
        _Beauty ("Beauty", Vector) = (0,0,0)
        _Charm ("Charm", Vector) = (0,0,0)
        _Strangeness ("_Strangeness", Vector) = (0,0,0)
        
        _NoiseColor ("NoiseColor", Color) = (1,1,1,1)
        _NoiseOffset ("NoiseOffset", Vector) = (0,0,0)
        _NoiseSize("NoiseSize", float) = 1
        _NoiseImportance("NoiseImportance", float) = 1
        _NoiseSharpness("NoiseSharpness",float) = 1
        _NoiseSubtractor("NoiseSubtractor",float)=0
        _NoiseSpeed("NoiseSpeed", Vector) = (0,0,0)

        _EdgeThickness("EdgeThickness", Range(0, 1)) = .1
        _EdgeSoftness("EdgeSoftness", Range(0.001, .5)) = .1
        _EdgeColor("EdgeColor", Color) = (1,1,1,1)
        _EdgeSpecularSharpness("EdgeSpecularSharpness",float)=24
        _EdgeSpecularStrength("EdgeSpecularStrength",float)=1
    }
    
    SubShader
    {
        // Draw ourselves after all opaque geometry
        Tags
        {
            "Queue" = "Geometry+10"
        }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _BaseColor;
            float4 _CenterOrbColor;
            float4 _NoiseColor;
            float3 _Truth;
            float3 _Beauty;
            float3 _Charm;
            float3 _Strangeness;
            float3 _NoiseSpeed;
            int _NumSteps;
            float _DeltaStepSize;
            float _NoiseSize;
            float _CenterOrbFalloff;
            float _NoiseImportance;
            float _CenterOrbImportance;
            float _CenterOrbFalloffSharpness;
            float _StepRefractionMultiplier;
            float _NoiseSharpness;
            float _Opaqueness;
            float _NoiseSubtractor;
            float _ColorMultiplier;
            float _RefractionBackgroundSampleExtraStep;
            float _IndexOfRefraction;
            float3 _CenterOrbOffset;
            float3 _NoiseOffset;

            float _ReflectionSharpness;
            float _ReflectionMultiplier;
            float4 _ReflectionColor;

            float _EdgeSpecularSharpness;
            float _EdgeSpecularStrength;
            float4 _EdgeColor;
            float _EdgeThickness;
            float _EdgeSoftness;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : NORMAL;
                float3 ro : TEXCOORD11;
                float3 rd : TEXCOORD12;
                float3 eye : TEXCOORD3;
                float3 localPos : TEXCOORD4;
                float3 worldNor : TEXCOORD5;
                float3 lightDir : TEXCOORD6;
                float4 grabPos : TEXCOORD7;
                float3 unrefracted : TEXCOORD8;
                float uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
            };


            sampler2D _BackgroundTexture;


            struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
            };

            // Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            // which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert(appdata vertex)
            {
                varyings o;
                float4 pos = vertex.position;
                float3 normal = vertex.normal;

                o.uv1 = vertex.uv1;
                o.uv2 = vertex.uv2;
                
                float3 worldPos = mul(unity_ObjectToWorld, float4(pos.xyz, 1.0f)).xyz;
                o.pos = UnityObjectToClipPos(float4(pos.xyz, 1.0f));
                o.nor = normalize(normal);
                o.ro = pos;
                o.localPos = pos.xyz;

                float3 localP = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                float3 eye = normalize(localP - pos.xyz);

                o.unrefracted = eye;
                o.rd = refract(eye, -normal, _IndexOfRefraction);
                o.eye = refract(
                    -normalize(_WorldSpaceCameraPos - worldPos),
                    normalize(mul(unity_ObjectToWorld, float4(normal.xyz, 0.0f))),
                    _IndexOfRefraction
                );
                
                o.worldNor = normalize(mul(unity_ObjectToWorld, float4(-normal, 0.0f)).xyz);
                o.lightDir = normalize(mul(unity_ObjectToWorld, float4(1, -1, 0, 0)).xyz);

                float4 refractedPos = UnityObjectToClipPos(float4(o.ro + o.rd * 1.5, 1));
                o.grabPos = ComputeGrabScreenPos(refractedPos);
                return o;
            }

            float calcTriEdge(float input)
            {
                return 1 - smoothstep(_EdgeThickness - _EdgeSoftness, _EdgeThickness + _EdgeSoftness, input);
            }

            float calcEdges(float4 uv1, float4 uv2)
            {
                float triEdge = max(max(calcTriEdge(uv2.x), calcTriEdge(uv2.y)), calcTriEdge(uv2.z));
                float polyEdge = (1 - smoothstep(1 - (_EdgeThickness - _EdgeSoftness), 1 - (_EdgeThickness + _EdgeSoftness), uv1).r);
                return 1 - min(polyEdge, triEdge);
            }


            #ifndef __noise_hlsl_
            #define __noise_hlsl_
            float hash(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            float noise(float3 x)
            {
                // The noise function returns a value in the range -1.0f -> 1.0f

                float3 p = floor(x);
                float3 f = frac(x);

                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;

                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                                 lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                            lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                 lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }
            #endif



            float tri(in float x)
            {
                return sin(x);
            }
            
            float3 tri3(in float3 p)
            {
                return float3(
                    tri(p.y + tri(p.z)),
                    tri(p.z + tri(p.x)),
                    tri(p.y + tri(p.x))
                );
            }

            float triAdd(in float3 p)
            {
                return (tri(p.x + tri(p.y + tri(p.z))));
            }

            float triangularNoise(float3 p)
            {
                float totalFog = 0;
                p *= _NoiseSize;
                
                p += tri3(p.xyz * .3 ) * 2.6;
                totalFog += triAdd(p.yxz * .3) * .2;
                
                p += tri3(p.xyz * .8 + 121) * 1;
                totalFog += triAdd(p.yxz * 1) * .2;
                
                p += tri3(p.xyz * 1.8 + 121) * 1;
                totalFog += triAdd(p.yxz * 1.3) * .2;
                
                return totalFog;
            }

            float noiseFn(float val)
            {
                val = sin(val);
                return abs(val) > 0.5 ? val : val;
            }

            float t3D(float3 pos)
            {
                float3 fPos = pos * .1 + _NoiseOffset;
                fPos += (_Time * _NoiseSpeed);
                // return triangularNoise(fPos);
                // return (.5 * noise(fPos) + .3 * noise(fPos * 6) + .2 * noise(fPos * 18)) * 2 - 1;
                return (
                    (noiseFn((fPos.x + _Strangeness.x) * _Charm.x) * _Truth.x) +
                    (noiseFn((fPos.y + _Strangeness.y) * _Charm.y) * _Truth.y) +
                    (noiseFn((fPos.z + _Strangeness.z) * _Charm.z) * _Truth.z)
                );
            }

            float3 nT3D(float3 pos)
            {
                float3 eps = float3(.0001, 0, 0);
                return t3D(pos) * normalize(
                    float3(
                        t3D(pos + eps.xyy) - t3D(pos - eps.xyy),
                        t3D(pos + eps.yxy) - t3D(pos - eps.yxy),
                        t3D(pos + eps.yyx) - t3D(pos - eps.yyx)
                    )
                );
            }

            float4 frag(varyings v) : COLOR
            {
                float3 col = 0;
                float dt = _DeltaStepSize;
                float counter = 0;
                float c = 0.0;
                float3 steppedPos = 0;

                float totalSmoke = 0;
                float3 rd = v.rd;
                
                for (int i = 0; i < _NumSteps; i++)
                {
                    counter += dt * exp(-2. * c);
                    steppedPos = v.ro - rd * counter * 2;

                    float3 smoke = nT3D(steppedPos * _NoiseSize);
                    float3 smokeColor = nT3D(steppedPos * _NoiseSize * .125);
                    float3 nor = normalize(smoke);

                    float noiseDensity = saturate(length(smoke) - _NoiseSubtractor);
                    noiseDensity = pow(noiseDensity, _NoiseSharpness) * _NoiseImportance;
                    
                    float centerOrbDensity = _CenterOrbImportance / (
                        pow(
                            length(steppedPos - _CenterOrbOffset),
                            _CenterOrbFalloffSharpness
                        ) * _CenterOrbFalloff
                    );

                    c = saturate(centerOrbDensity + noiseDensity);
                    centerOrbDensity -= noiseDensity;
                    totalSmoke += c;

                    float3 noiseColor = _NoiseColor * float3(smokeColor.x * _Beauty.x, smokeColor.y * _Beauty.y, smokeColor.z * _Beauty.z); //(sin(fPos.x * 10) * .2 + cos(fPos.y * 10) * .2 + sin(fPos.z * 10) * .2) * .5;;

                    rd = normalize(rd * (1 - c * _StepRefractionMultiplier) + nor * c * _StepRefractionMultiplier);
                    col = .99 * col;
                    float3 col2 = lerp(_BaseColor, _CenterOrbColor, saturate(centerOrbDensity));
                    col += lerp(col2, noiseColor, saturate(noiseDensity));
                }

                float4 refractedPos = ComputeGrabScreenPos(
                    UnityObjectToClipPos(
                        float4(steppedPos + rd * _RefractionBackgroundSampleExtraStep, 1)
                    )
                );
                
                float4 backgroundCol = tex2Dproj(_BackgroundTexture, refractedPos);
                col /= float(_NumSteps);
                col *= _ColorMultiplier;
                col = lerp(col * backgroundCol, col, saturate(totalSmoke * _Opaqueness));
                float edges = calcEdges(v.uv1, v.uv2);
                // float4 epsilonR = float4(0.01, 0, 0, 0);
                // float edgeR = calcEdges(v.uv1 + epsilonR, v.uv2 + epsilonR);
                // float4 epsilonT = float4(0, 0.01, 0, 0);
                // float edgeT = calcEdges(v.uv1 + epsilonT, v.uv2 + epsilonT);
                // float3 edgeNormal = normalize(
                //     float3(2 * (edgeR - edges), 2 * (edges - edgeT), -4)
                // );
                float3 spec = pow(
                    max(dot(normalize(v.unrefracted), normalize(v.nor)), 0.0),
                    _EdgeSpecularSharpness
                );
                float3 specular = spec * _EdgeSpecularStrength;
                float3 reflect = min(pow(
                    min(1 - saturate(dot(normalize(v.unrefracted), normalize(v.nor))),.8),
                    _ReflectionSharpness
                ), .95);
                float3 result = lerp(_EdgeColor + specular, col.xyz, max(edges, 1-saturate(_EdgeColor.a)));
                result += reflect * _ReflectionMultiplier * _ReflectionColor;
                return float4(result, 1);
            }
            
            ENDCG

        }
    }

    Fallback Off


}