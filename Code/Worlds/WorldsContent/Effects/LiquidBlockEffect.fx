#include "Utils.fxu"

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float TimeOfDay;
float4 CurrentAtmosphereColor;
float4 NextAtmosphereColor;

int PreviousHour;
int NextHour;

//#EDED28
float4 SkyColor = float4(1.0f, 1.0f, 1.0f, 0.0f);

// Set the intensity of the hemisphere color
float HemisphereIntensity = 1.0f;

Texture TextureAtlas;
sampler TextureAtlasSampler = sampler_state
{
    texture = <TextureAtlas>;
    magfilter = Point;
    //minfilter = Anisotropic;
    minfilter = Linear;
    mipfilter = Linear;
    //MaxAnisotropy = 16;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VSInput
{
    short4 PositionAndOcclusion : POSITION0;
    float2 TexCoords1 : TEXCOORD0;
    // We would need to mix colors first, and then darken them overall
    // W part of color will be the amount of darkness (0-15)
    float4 Light : COLOR1;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float2 TexCoords1 : TEXCOORD0;
    float3 CameraView : TEXCOORD1;
    float Distance : TEXCOORD2;
    float4 Color : COLOR0;
};

VSOutput VertexShaderCommon(VSInput input, float4x4 instanceTransform)
{
    VSOutput output;

    float4 position = {(float)input.PositionAndOcclusion.x, (float)input.PositionAndOcclusion.y, (float)input.PositionAndOcclusion.z, 1};

    float4 worldPosition = mul(position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.CameraView = normalize(CameraPosition - worldPosition);
    output.Distance = length(CameraPosition - worldPosition);

    output.TexCoords1 = input.TexCoords1;

    //float4 sColor = ColorByTimeOfDay(CurrentAtmosphereColor, TimeOfDay);
    float4 sColor = LerpColors(CurrentAtmosphereColor, NextAtmosphereColor, PreviousHour, NextHour, TimeOfDay);	

    // adjust brightness
    int brightness = (int)input.Light.a;

    if (brightness > 15){
        brightness = 15;
    }

    brightness = 15;

    float brightnessValue = BrightnessValues[brightness];

    sColor = sColor * brightnessValue;

    //Calculating ambient occlusion here.
    if (input.PositionAndOcclusion.w != 3){
        output.Color.rgb = sColor / (0.6 * (4 - input.PositionAndOcclusion.w));
    }
    else{
        output.Color.rgb = sColor;
    }
    
	if (output.Color.a > 0.5){
		output.Color.a = 0.5;
	}

    return output;
}

VSOutput VertexShaderFunction(VSInput input)
{
    return VertexShaderCommon(input, World);
}

VSOutput HardwareInstancingVertexShader(VSInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
    return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}

float4 BaseColorPS(VSOutput input) : COLOR0
{
    float mipLevel = GetMipLevel(input.TexCoords1, 32);
    if (mipLevel > 7)
    {
        mipLevel = 7;
    }
    float4 mipCoords = float4(input.TexCoords1.x, input.TexCoords1.y, 0, mipLevel);

    //We're cutting needed texture from the atlas here (although I'm not exactly sure we're doing exactly that)
    //float4 texColor1 = tex2D(TextureAtlasSampler, input.TexCoords1);
    float4 texColor1 = tex2Dlod(TextureAtlasSampler, mipCoords);

    float4 color;
    color.rgb = texColor1.rgb * input.Color.rgb;
    color.a = texColor1.a;

    // Simple transparency magic
    clip(input.Color.a < 0.1f ? -1:1);

    return color;
}

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float4 color = BaseColorPS(input);

    return color;
}

technique BlockTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
        //VertexShader = compile vs_2_0 VertexShaderFunction();
        //PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
