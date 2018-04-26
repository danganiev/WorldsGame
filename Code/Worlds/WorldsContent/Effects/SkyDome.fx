 #include "Utils.fxu"
 
 //------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

// Maybe there is something about horizon color I should do later
float4 HorizonColor;

float4 CurrentAtmosphereColor;		
//float4 NightColor;
float4 NextAtmosphereColor;

int PreviousHour;
int NextHour;

float4 MorningTint;		
float4 EveningTint;	

float TimeOfDay;
 
//------- Texture Samplers --------
Texture xTexture0;

//------- Technique: SkyDome --------
 struct SDVertexToPixel
 {    
     float4 Position         : POSITION;
     float2 TextureCoords    : TEXCOORD0;
     float4 ObjectPosition    : TEXCOORD1;
 };
 
 struct SDPixelToFrame
 {
     float4 Color : COLOR0;
 };

 SDVertexToPixel SkyDomeVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
 {    
     SDVertexToPixel Output = (SDVertexToPixel)0;
     float4x4 preViewProjection = mul (xView, xProjection);
     float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
     
     Output.Position = mul(inPos, preWorldViewProjection);
     Output.TextureCoords = inTexCoords;
     Output.ObjectPosition = inPos;
     
     return Output;    
 }

 SDPixelToFrame SkyDomePS(SDVertexToPixel PSIn)
 {
     SDPixelToFrame Output = (SDPixelToFrame)0;        
 	 
	 float4 topColor = LerpColors(CurrentAtmosphereColor, NextAtmosphereColor, PreviousHour, NextHour, TimeOfDay);
     float4 bottomColor = HorizonColor;    
	 //float4 nColor = NightColor;

	 //nColor *= (4 - PSIn.TextureCoords.y) * .125f;

	 bottomColor += (MorningTint * .05) * ((24 - TimeOfDay)/24);
	 bottomColor += (EveningTint * .05) * (TimeOfDay / 24);	
	 //topColor += nColor;
	 //bottomColor += nColor;

     //float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.9f));
     float4 baseColor = topColor;
        
	 Output.Color = baseColor;
 
     return Output;
 }


 technique SkyDome
 {
     pass Pass0
     {
         VertexShader = compile vs_3_0 SkyDomeVS();
         PixelShader = compile ps_3_0 SkyDomePS();
     }
 }