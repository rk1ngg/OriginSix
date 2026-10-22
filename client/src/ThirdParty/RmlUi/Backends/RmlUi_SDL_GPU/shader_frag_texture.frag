#include "shader_common.hlsli"

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 main(Varyings input) : SV_Target0 {
    return input.Color * Texture.Sample(Sampler, input.TexCoord);
}
