//
// Opaque surface shader for Wall
//
// Vertex format:
// position.xyz = vertex position
// texcoord.xy  = uv for position/rotation/scale texture
//
// Texture format:
// _PositionTex.xyz = object position
// _PositionTex.w   = color parameter
// _RotationTex.xyz = object rotation (quaternion)
// _ScaleTex.xyz    = scale factor
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

        // Calculate a color.
        float4 calc_color(float2 uv, float param)
        {
        #ifdef COLOR_ANIMATE
            return lerp(_Color, _Color2, param);
        #elif COLOR_RANDOM
            return lerp(_Color, _Color2, nrand(uv));
        #else
            return _Color;
        #endif
        }

        struct Input
        {
            half4 color : COLOR;
        };

        void vert(inout appdata_full v)
        {
            float4 uv = float4(v.texcoord.xy + _BufferOffset, 0, 0);

            float4 p = tex2Dlod(_PositionTex, uv);
            float4 r = tex2Dlod(_RotationTex, uv);
            float4 s = tex2Dlod(_ScaleTex, uv);

            v.vertex.xyz = rotate_vector(v.vertex.xyz * s.xyz, r) + p.xyz;
            v.normal = rotate_vector(v.normal, r);
            v.color = calc_color(uv, p.w);
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
