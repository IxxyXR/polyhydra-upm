

Shader "FantasyCrystals/TrippyCrystal1"
{

    Properties {

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

    _NoiseColor ("NoiseColor", Color) = (1,1,1,1)
    _NoiseOffset ("NoiseOffset", Vector) = (0,0,0)
    _NoiseSize("NoiseSize", float) = 1
    _NoiseImportance("NoiseImportance", float) = 1
    _NoiseSharpness("NoiseSharpness",float) = 1
    _NoiseSubtractor("NoiseSubtractor",float)=0
    }


  SubShader{

            // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Geometry+10" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

      Cull Off
    Pass{
CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      
    float4 _BaseColor;
    float4 _CenterOrbColor;
    float4 _NoiseColor;
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


      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
          float3 ro : TEXCOORD1;
          float3 rd : TEXCOORD2;
          float3 eye : TEXCOORD3;
          float3 localPos : TEXCOORD4;
          float3 worldNor : TEXCOORD5;
          float3 lightDir : TEXCOORD6;
          float4 grabPos : TEXCOORD7;
          float3 unrefracted : TEXCOORD8;
          
          
      };


            sampler2D _BackgroundTexture;


             struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
//which we transform with the view-projection matrix before passing to the pixel program.
varyings vert ( appdata vertex ){



  varyings o;
     float4 p = vertex.position;
     float3 n =  vertex.normal;//_NormBuffer[id/3];

        float3 worldPos = mul (unity_ObjectToWorld, float4(p.xyz,1.0f)).xyz;
        o.pos = UnityObjectToClipPos (float4(p.xyz,1.0f));
        o.nor = n;//normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f)));; 
        o.ro = p;//worldPos.xyz;
        o.localPos = p.xyz;
        
        
        float3 localP = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
        float3 eye = normalize(localP - p.xyz);


        o.unrefracted = eye;
        o.rd = refract( eye , -n , _IndexOfRefraction);
        o.eye = refract( -normalize(_WorldSpaceCameraPos - worldPos) , normalize(mul (unity_ObjectToWorld, float4(n.xyz,0.0f))) , _IndexOfRefraction);
        //o.worldNor = mul (unity_ObjectToWorld, float4(n.xyz,0.0f)).xyz;
        o.worldNor = normalize(mul (unity_ObjectToWorld, float4(-n,0.0f)).xyz);
        o.lightDir = normalize(mul( unity_ObjectToWorld , float4(1,-1,0,0)).xyz);

        float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
    o.grabPos = ComputeGrabScreenPos(refractedPos);
    
        //o.triID = float(id)%3;


   


  

  return o;

}

float tri(in float x){return abs(frac(x)-.5);}
float3 tri3(in float3 p){return float3( tri(p.y+tri(p.z)), tri(p.z+tri(p.x)), tri(p.y+tri(p.x)));}
           
float triAdd( in float3 p ){ return (tri(p.x+tri(p.y+tri(p.z)))); }

float triangularNoise( float3 p ){

    float totalFog = 0;

    float noiseScale = 1;

    float3 tmpPos = p;

    float noiseContribution = 1;

    float3 offset = 0;

    p *= _NoiseSize;
    p *= 2;

   float speed = 1.1;
 
   p +=  tri3(p.xyz * .3 ) *1.6;
   totalFog += triAdd(p.yxz * .3) * .35;
    
   p +=  tri3(p.xyz * .4 + 121 ) * 1;
   totalFog += triAdd(p.yxz * 1) * .25;
    
   p +=  tri3(p.xyz * .8 + 121 ) * 1;
   totalFog += triAdd(p.yxz* 1.3) * .15;

  return totalFog;

}


float t3D( float3 pos ){
  float3 fPos = pos * .05 + _NoiseOffset;

  // Adds Randomness to noise for each crystal
 // fPos += 100 * mul(unity_ObjectToWorld,float4(0,0,0,1)).xyz;
  return triangularNoise( fPos);
}
float dT3D( float3 pos , float3 lightDir ){

  float eps = .0001;

  
  return ((t3D(pos) - t3D(pos+ lightDir * eps))/eps+.5);
}

float3 nT3D( float3 pos ){

  float3 eps = float3(.0001,0,0);

  return t3D(pos) * normalize(
         float3(  t3D(pos + eps.xyy) - t3D(pos - eps.xyy), 
                  t3D(pos + eps.yxy) - t3D(pos - eps.yxy),
                  t3D(pos + eps.yyx) - t3D(pos - eps.yyx) ));


}


//Pixel function returns a solid color for each point.
float4 frag (varyings v) : COLOR {
  float3 col =0;//hsv( float(v.face) * .3 , 1,1);


  
  float dt = _DeltaStepSize;
  float t = 0;
  float c = 0.;
float3 p = 0;

float totalSmoke = 0;
  float3 rd = v.rd;
  for(int i =0 ; i < _NumSteps; i++ ){
      t+=dt*exp(-2.*c);
    p = v.ro - rd * t * 2;
    
  float3 smoke = nT3D( p * _NoiseSize );
  float3 nor = normalize(smoke);

  float noiseDensity = saturate(length(smoke) - _NoiseSubtractor);


    noiseDensity =   pow( noiseDensity , _NoiseSharpness)  * _NoiseImportance;


    float centerOrbDensity = ((_CenterOrbImportance)/(pow(length(p-_CenterOrbOffset),_CenterOrbFalloffSharpness) * _CenterOrbFalloff)) ;
  
    c= saturate(centerOrbDensity +noiseDensity);   
    centerOrbDensity -= noiseDensity;
    totalSmoke += c;

    rd = normalize(rd * (1-c*_StepRefractionMultiplier) + nor *  c*_StepRefractionMultiplier);
     col = .99*col + lerp( lerp(_BaseColor,_CenterOrbColor * (nor * .5 + .5) , saturate(centerOrbDensity)),_NoiseColor* (nor * .5 + .5) , saturate(noiseDensity ));// saturate(dot(v.lightDir , nor)) * .1 *c;//hsv(c,.4, dT3D(p*3,float3(0,-1,0))) * c;//hsv(c * .8 + .3,1,1)*c;;// hsv(smoke,1,1) * saturate(smoke);

 
  }


       // float4 refractedPos = UnityObjectToClipPos( float4(o.ro + o.rd * 1.5,1));
  float4 refractedPos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(p+rd * _RefractionBackgroundSampleExtraStep,1)));
float4 backgroundCol = tex2Dproj(_BackgroundTexture, refractedPos);

 col /= float(_NumSteps);
 col *= _ColorMultiplier;

 // col = normalize(col);

  float3 baseCol =_BaseColor.xyz;

 col = lerp(col*backgroundCol,col,saturate(totalSmoke * _Opaqueness));

       
 float m = dot( normalize(v.unrefracted), normalize(v.nor) );
 col += pow((1-m),_ReflectionSharpness) * _ReflectionMultiplier * _ReflectionColor;


 float3 reflection=-v.eye;//normalize(reflect( normalize(-v.eye) , -v.worldNor));
      half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflection,0); //UNITY_SAMPLE_TEXCUBE_LOD('cubemap', 'sample coordinate', 'map-map level')
         half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR); // This is done because the cubemap is stored HDR
        //col = skyColor;

    //col = v.nor * .5 + .5;
    // col *= pow((1-m),5) * 60;
    // col += (v.nor * .5 + .5 ) * .4;

    return float4( col.xyz , 1);//saturate(float4(col,3*length(col) ));




}

      ENDCG

    }
  }

  Fallback Off


}
