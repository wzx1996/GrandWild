#version 400
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(std140,set=0,binding=0) uniform SceneStruct {
	float globalLightStrength;
    vec4 globalLightDirection;
    vec4 globalLightColor;
	vec4 fogColor;
    float fogDensity;
} Scene;
layout(set=1,binding=0) uniform sampler2D Texture;

layout (location=0) in vec4 InColor;
layout (location=1) in vec2 TextureCoord;
layout (location=2) in float Brightness;

in vec4 gl_FragCoord;

layout (location=0) out vec4 OutColor;

void main(){
	vec4 OutColorRaw=vec4(InColor.rgb*InColor.a,0.0)+texture(Texture,TextureCoord);
	float actualFogDensity=Scene.fogDensity*gl_FragCoord.z;
	OutColor=OutColorRaw*(vec4(1.0,1.0,1.0,1.0)+Brightness*Scene.globalLightColor)*(1-actualFogDensity)+actualFogDensity*Scene.fogColor;

}