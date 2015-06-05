//
// Opaque surface shader for Wall
//
// Vertex format:
// position.xyz = vertex position
// texcoord0.xy  = uv for texturing
// texcoord1.xy  = uv for position/rotation/scale texture
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
        _PositionTex       ("-", 2D)     = ""{}
        _RotationTex       ("-", 2D)     = ""{}
        _ScaleTex          ("-", 2D)     = ""{}
        _Color             ("-", Color)  = (1, 1, 1, 1)
        _Color2            ("-", Color)  = (1, 1, 1, 1)
        _MainTex           ("-", 2D)     = "white"{}
		_BumpMap           ("-", 2D)     = "bump"{}
		_OcclusionMap      ("-", 2D)     = "white"{}
		_OcclusionStrength ("-", Float)  = 1.0
        _PbrParams         ("-", Vector) = (0.5, 0.5, 0, 0) // (metalness, smoothness)
        _BufferOffset      ("-", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma multi_compile COLOR_SINGLE COLOR_RANDOM COLOR_ANIMATE
        #pragma multi_compile _ _ALBEDOMAP
        #pragma multi_compile _ _NORMALMAP
        #pragma multi_compile _ _OCCLUSIONMAP
        #pragma target 3.0

        sampler2D _PositionTex;
        sampler2D _RotationTex;
        sampler2D _ScaleTex;

        half4 _Color;
        half4 _Color2;

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;
        half _OcclusionStrength;

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
            float2 uv_MainTex;
            half4 color : COLOR;
        };

        void vert(inout appdata_full v)
        {
            float4 uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

            float4 p = tex2Dlod(_PositionTex, uv);
            float4 r = tex2Dlod(_RotationTex, uv);
            float4 s = tex2Dlod(_ScaleTex, uv);

            v.vertex.xyz = rotate_vector(v.vertex.xyz * s.xyz, r) + p.xyz;
            v.normal = rotate_vector(v.normal, r);
        #if _NORMALMAP
            v.tangent.xyz = rotate_vector(v.tangent.xyz, r);
        #endif
            v.color = calc_color(uv, p.w);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
        #if _ALBEDOMAP
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color.rgb * c.rgb;
            o.Alpha = IN.color.a * c.a;
        #else
            o.Albedo = IN.color.rgb;
            o.Alpha = IN.color.a;
        #endif

        #if _NORMALMAP
            half4 n = tex2D(_BumpMap, IN.uv_MainTex);
            o.Normal = UnpackNormal(n);
        #endif

        #if _OCCLUSIONMAP
            half4 occ = tex2D(_OcclusionMap, IN.uv_MainTex);
            o.Occlusion = lerp((half4)1, occ, _OcclusionStrength);
        #endif

            o.Metallic = _PbrParams.x;
            o.Smoothness = _PbrParams.y;
        }

        ENDCG
    }
}
