//
// KinoGlitch - Video glitch effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Hidden/Kino/Glitch/Analog"
{
	HLSLINCLUDE
	#pragma target 3.0

	// #pragma vertex VertUVTransform
	// #pragma fragment FragGlitch

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float4 _MainTex_TexelSize;

	float2 _ScanLineJitter; // (displacement, threshold)
	float2 _VerticalJump;   // (amount, time)
	float _HorizontalShake;
	float2 _ColorDrift;     // (amount, time)

	float nrand(float x, float y)
	{
		return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
	}
	
	float4 FragGlitch(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;

		// Scan line jitter
		float jitter = nrand(uv.y, _Time.x) * 2 - 1;
		jitter *= step(_ScanLineJitter.y, abs(jitter)) * _ScanLineJitter.x;

		// Vertical jump
		float jump = lerp(uv.y, frac(uv.y + _VerticalJump.y), _VerticalJump.x);

		// Horizontal shake
		float shake = (nrand(_Time.x, 2) - 0.5) * _HorizontalShake;

		// Color drift
		float drift = sin(jump + _ColorDrift.y) * _ColorDrift.x;

		float4 src1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, frac(float2(uv.x + jitter + shake, jump)));
		float4 src2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, frac(float2(uv.x + jitter + shake + drift, jump)));

		return float4(src1.r, src2.g, src1.b, 1);
	}

	ENDHLSL
	
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragGlitch

			ENDHLSL
		}
	}
}