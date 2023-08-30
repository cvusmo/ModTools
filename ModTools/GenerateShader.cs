using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ModTools
{
    internal class GenerateShader
    {
        public static void Create3DTextureShader()
        {
            List<string> orientations = ModToolsSettings.modData.Orientations;
            int resolution = ModToolsSettings.modData.Resolution;
            {
                string shaderContent = @"

Shader ""ModTools/GeneratedShader""
{
    Properties
    {
        _Albedo (""Albedo"", 2D) = ""white"" {}
        _EmissionMap (""Emission Map"", 2D) = ""white"" {}
        _UseEmission (""Use Emission"", Int) = 1 // 0 for off, 1 for on
        
        _Tiling (""Tiling"", Vector) = (1, 1, 1, 1)
        _Offset (""Offset"", Vector) = (0, 0, 0, 0)

        _MainTexBack (""Texture Back"", 3D) = ""white"" {}
        _MainTexBackRight (""Texture BackRight"", 3D) = ""white"" {}
        _MainTexRight (""Texture Right"", 3D) = ""white"" {}
        _MainTexFrontRight (""Texture FrontRight"", 3D) = ""white"" {}
        _MainTexFront (""Texture Front"", 3D) = ""white"" {}
        _MainTexFrontLeft (""Texture FrontLeft"", 3D) = ""white"" {}
        _MainTexLeft (""Texture Left"", 3D) = ""white"" {}
        _MainTexBackLeft (""Texture BackLeft"", 3D) = ""white"" {}
        _MainTexTop (""Texture Top"", 3D) = ""white"" {}
        _MainTexBottom (""Texture Bottom"", 3D) = ""white"" {}
        _MainTexTopDiagonal (""Texture TopDiagonal"", 3D) = ""white"" {}
        _MainTexBottomDiagonal (""Texture BottomDiagonal"", 3D) = ""white"" {}
        _Alpha (""Alpha"", float) = 0.02
        _StepSize (""Step Size"", float) = 0.01
        _AnimationTime (""Animation Time"", float) = 0.5
    }

    SubShader
    {
        Tags { ""Queue"" = ""Transparent"" ""RenderType"" = ""Transparent"" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include ""UnityCG.cginc""
            #define MAX_STEP_COUNT 128
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objectVertex : TEXCOORD0;
                float3 vectorToSurface : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler3D _MainTexBottom;
            sampler3D _MainTexTop;
            sampler3D _MainTexLeft;
            sampler3D _MainTexRight;
            sampler3D _MainTexBack;
            sampler3D _MainTexFront;
            sampler3D _MainTexFrontLeft;
            sampler3D _MainTexBackRight;
            sampler3D _MainTexTopDiagonal;
            sampler3D _MainTexFrontRight;
            sampler3D _MainTexBackLeft;
            sampler3D _MainTexBottomDiagonal;
            sampler2D _Albedo;
            sampler2D _EmissionMap;

            float4 _MainTex_ST;
            float _Alpha;
            float _StepSize;
            float _AnimationTime;
            float2 _Tiling;
            float2 _Offset;
            int _UseEmission;

            v2f vert (appdata v)
            {
                v2f o;
                o.objectVertex = v.vertex;
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling.xy + _Offset.xy;
                return o;
            }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = i.objectVertex;
                float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));
                float4 color = float4(0, 0, 0, 0);
                rayOrigin.z += _AnimationTime;

                float3 samplePosition = rayOrigin;
                float3 lightDir = float3(0, -1, 0); 

                for (int j = 0; j < MAX_STEP_COUNT; j++)
                {
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        float4 sampledColor;

                        if (rayDirection.y > 0) sampledColor = tex3D(_MainTexTop, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        else if (rayDirection.y < 0) sampledColor = tex3D(_MainTexBottom, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        else if (rayDirection.x > 0) sampledColor = tex3D(_MainTexLeft, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        else if (rayDirection.x < 0) sampledColor = tex3D(_MainTexRight, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        else if (rayDirection.z > 0) sampledColor = tex3D(_MainTexFront, samplePosition + float3(0.5f, 0.5f, 0.5f));
                        else sampledColor = tex3D(_MainTexBack, samplePosition + float3(0.5f, 0.5f, 0.5f));

                        
                        float3 normal = normalize(sampledColor.rgb * 2.0 - 1.0); 
                        float lambert = max(dot(normal, lightDir), 0.0);

                        sampledColor.rgb *= lambert; 
                        sampledColor.a *= _Alpha;

                        color = BlendUnder(color, sampledColor);
                        samplePosition += rayDirection * _StepSize;

                        // Add Albedo and Emission:                  
                        float4 albedoColor = tex2D(_Albedo, i.uv);
                        color.rgb *= albedoColor.rgb;
                        if (_UseEmission == 1) 
                        {
                            float3 emission = tex2D(_EmissionMap, i.uv).rgb;
                            color.rgb += emission;
                        }
                    }
                }
                return color;
            }
            ENDCG
        }
    }
}
";

                string path = "Assets/GeneratedShaders/3DTextureShader.shader";
                Directory.CreateDirectory("Assets/GeneratedShaders");
                File.WriteAllText(path, shaderContent);

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", "3D Texture Shader added successfully!", "OK");
            }

        }
    }
}