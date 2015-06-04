//
// Opaque surface shader for Spray
//
// Vertex format:
// position.xyz = vertex position
// texcoord.xy  = uv for PositionTex/RotationTex
//
// Texture format:
// _PositionTex.xyz = particle position
// _PositionTex.w   = life
// _RotationTex.xyz = particle rotation
// _RotstionTex.w   = scale factor
//
Shader "Hidden/Kvant/Wall/Surface"
{
    Properties
    {
        _PositionTex  ("-", 2D)     = ""{}
        _RotationTex  ("-", 2D)     = ""{}
        _ScaleTex     ("-", 2D)     = ""{}
        _Color        ("-", Color)  = (1, 1, 1, 1)
        _Color2       ("-", Color)  = (1, 1, 1, 1)
        _PbrParams    ("-", Vector) = (0.5, 0.5, 0, 0) // (metalness, smoothness)
        _BufferOffset ("-", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma multi_compile COLOR_SINGLE COLOR_RANDOM COLOR_ANIMATE
        #pragma target 3.0

        sampler2D _PositionTex;
        sampler2D _RotationTex;
        sampler2D _ScaleTex;

        half4 _Color;
        half4 _Color2;
        half2 _PbrParams;
        float2 _BufferOffset;

        // PRNG function.
        float nrand(float2 uv)
        {
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

        // Rotate a vector with a rotation quaternion.
        // http://mathworld.wolfram.com/Quaternion.html
        float3 rotate_vector(float3 v, float4 r)
        {
            float4 r_c = r * float4(-1, -1, -1, 1);
            return qmul(r, qmul(float4(v, 0), r_c)).xyz;
        }

        struct Input
        {
            half4 color : COLOR;
        };

        void vert(inout appdata_full v)
        {
            float2 uv = v.texcoord.xy + _BufferOffset;

            float4 p = tex2D(_PositionTex, uv);
            float4 r = tex2D(_RotationTex, uv);
            float4 s = tex2D(_ScaleTex, uv);

            v.vertex.xyz = rotate_vector(v.vertex.xyz, r) * s + p.xyz;
            v.normal = rotate_vector(v.normal, r);
            v.color = _Color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = IN.color.rgb;
            o.Metallic = _PbrParams.x;
            o.Smoothness = _PbrParams.y;
            o.Alpha = IN.color.a;
        }

        ENDCG
    }
}
