Shader "Custom/WaterShader"
{

    Properties
    {
        _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        _WaveB ("Wave B", Vector) = (0,1,0.25,20)
        _WaveC ("Wave C", Vector) = (1,1,0.15,10)
        _SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5)
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _VL("VL Maximum Distance", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull off
        Pass
        {
            CGPROGRAM
            #define UNITY_BRDF_PBS BRDF2_Unity_PBS

            #pragma target 3.0
            #pragma vertex vertex_data
            #pragma fragment v2f
            #include "UnityPBSLighting.cginc"
            #include "UnityCG.cginc"

            float4 _SpecularTint;
            float4 _WaveA, _WaveB, _WaveC;

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;

            float _DepthMaxDistance;

            sampler2D _CameraDepthTexture;


            float _GameTime;
            float _Param1;
            float _Param2;
            float _Param3;
            float _Param4;
            float _FadeTime;
            float _Speed;
            float _VL;

            int _RippleNum = 0;
            const int _ArraySize = 100;
            float4 _Ripples[300];


            float RippleFunction(float x, float z)
            {
                float d = sqrt(x * x + z * z);
                float y = sin(UNITY_PI * (d - _GameTime * _Speed)) * _Param1;
                return y / (_Param2 + _Param3 * d);
            }

            float CalculateY(float offsetX, float positionX, float offsetZ, float positionZ, float startTime)
            {
                float deltaTime = _GameTime - startTime;
                float3 currentPosition = float3(positionX + offsetX * 2, 0,
                                                positionZ + offsetZ * 2);

                float xSquared = (currentPosition.x - offsetX) * (currentPosition.x - offsetX);
                float zSquared = (currentPosition.z - offsetZ) * (currentPosition.z - offsetZ);
                float distance = sqrt(xSquared + zSquared);


                float distanceTimeRatio = distance / _Speed;
                if (distanceTimeRatio > deltaTime)
                {
                    return 0;
                }

                if (_FadeTime < _GameTime - startTime && distanceTimeRatio < _GameTime - (startTime + _FadeTime))
                {
                    return 0;
                }
                float y = RippleFunction((positionX + offsetX) * _Param4,
                                         (positionZ + offsetZ) * _Param4);

                return y;
            }


            struct Interpolators
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 screenPosition : TEXCOORD3;
                float vl: TEXCOORD4;
            };

            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            float3 GerstnerWave(
                float4 wave, float3 p, inout float3 tangent, inout float3 binormal
            )
            {
                float steepness = wave.z;
                float wavelength = wave.w;
                float k = 2 * UNITY_PI / wavelength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = steepness / k;

                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                binormal += float3(
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.y * d.y * (steepness * sin(f))
                );
                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                );
            }

            Interpolators vertex_data(VertexData v)
            {
                Interpolators i;
                i.vl = 0;
                for (int j = 0; j < _RippleNum; j++)
                {
                    i.vl += CalculateY(_Ripples[j].x, v.position.x,
                                       _Ripples[j].z, v.position.z, _Ripples[j].y);
                }
                float3 gridPoint = v.position.xyz;
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                float3 p = gridPoint;
                p += GerstnerWave(_WaveA, gridPoint, tangent, binormal);
                p += GerstnerWave(_WaveB, gridPoint, tangent, binormal);
                p += GerstnerWave(_WaveC, gridPoint, tangent, binormal);

                v.position.y = i.vl;
                v.position.xyz += p;
                i.position = UnityObjectToClipPos(v.position);

                i.uv = v.uv;
                i.worldPos = mul(unity_ObjectToWorld, v.position);
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.normal = normalize(i.normal);
                i.screenPosition = ComputeScreenPos(i.position);
                return i;
            }


            float4 v2f(Interpolators i) : SV_TARGET
            {
                //depth color
                float existingDepth01 = tex2D(_CameraDepthTexture,
                                              UNITY_PROJ_COORD(i.screenPosition.xy / i.screenPosition.w)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);
                float depthDifference = existingDepthLinear - i.screenPosition.w;
                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

                i.vl = i.vl * 0.5 + 0.5;
                
                //fake specular 
                if (i.worldPos.y + i.vl < _VL) return saturate(float4(i.vl * waterColor + pow(i.vl, i.vl))*_SpecularTint);
                
                return float4(i.vl * waterColor);
            }
            ENDCG
        }
    }
}