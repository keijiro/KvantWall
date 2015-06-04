//
// GPGPU kernels for Spray
//
// Texture format in position kernels:
// .xyz = particle position
// .w   = life
//
// Texture format in rotation kernels:
// .xyz = particle rotation
// .w   = scale factor
//
// In the rotation kernels, each rotation is represented in a unit quaternion.
// It lacks the w component (scalar part of quaternion), and it can be
// recalculated by sqrt(1-x^2-y^2-z^2). Note that the w component should be
// kept positive to make this calculation valid.
// 
Shader "Hidden/Kvant/Wall/Kernel"
{
    Properties
    {
        _MainTex     ("-", 2D)     = ""{}
        _Size        ("-", Vector) = (10, 10, 0, 0)
        _NoiseParams ("-", Vector) = (0.2, 5, 1, 0)   // (frequency, amplitude, animation)
        _Config      ("-", Vector) = (0, 0, 0, 0)     // (random seed, time)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise2D.cginc"

    #define PI2 6.28318530718

    sampler2D _MainTex;
    float2 _Size;
    float3 _NoiseParams;
    float2 _Config;

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, _Config.x);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Quaternion multiplication.
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    // Get a random rotation axis in a deterministic fashion.
    float3 get_rotation_axis(float2 uv)
    {
        // Uniformaly distributed points.
        // http://mathworld.wolfram.com/SpherePointPicking.html
        float u = nrand(uv, 13) * 2 - 1;
        float theta = nrand(uv, 14) * PI2;
        float u2 = sqrt(1 - u * u);
        return float3(u2 * cos(theta), u2 * sin(theta), u);
    }

    float3 initial_position(float2 uv)
    {
        return float3((uv - 0.5) * _Size, 0);
    }

    float3 position_animation(float2 uv, float t)
    {
        float2 p = (uv + t * _NoiseParams.z) * _NoiseParams.x;
        float nx = cnoise(p + float2(138.2, 0));
        float ny = cnoise(p + float2(0, 138.2));
        float nz = cnoise(p + float2(1000, 238.2));
        return float3(nx, ny, nz) * _NoiseParams.y;
    }

    // Pass 0: Position
    float4 frag_position(v2f_img i) : SV_Target 
    {
        return float4(initial_position(i.uv) + position_animation(i.uv, _Config.y), 0);
    }

    // Pass 1: Rotation
    float4 frag_rotation(v2f_img i) : SV_Target 
    {
        float2 p = (i.uv + _Config.y * _NoiseParams.z) * _NoiseParams.x;
        float nx = cnoise(p + float2(138.2, 2000));
        return float4(get_rotation_axis(i.uv) * sin(nx), cos(nx));
    }

    // Pass 2: Scale
    float4 frag_scale(v2f_img i) : SV_Target 
    {
        float2 p = (i.uv + _Config.y * _NoiseParams.z) * _NoiseParams.x;
        return (float4)(cnoise(p + float2(138.2, 28000)) / 1.4 + 0.5);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_rotation
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_scale
            ENDCG
        }
    }
}
