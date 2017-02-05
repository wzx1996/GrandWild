#version 400
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(std140,push_constant) uniform PushConstantsStruct{ 
	mat4 Model;
	mat4 ViewProjectionClip;
} PushConstants;
layout(std140,set=0,binding=0) uniform SceneStruct{
	float globalLightStrength;
    vec4 globalLightDirection;
    vec4 globalLightColor;
    float fogDensity;
    vec4 fogColor;
} Scene;

layout (location=0) in vec4 Pos;
layout (location=1) in vec4 InColor;
layout (location=2) in vec4 Normal;
layout (location=3) in vec2 InTextureCoord;

layout (location=0) out vec4 OutColor;
layout (location=1) out vec2 TextureCoord;
layout (location=2) out float Brightness;
out gl_PerVertex {
	vec4 gl_Position;
};

void main(){
	vec4 TransformedNormal=PushConstants.Model*PushConstants.ViewProjectionClip*Normal;
	float AbsoluteBrightness=dot(TransformedNormal,Scene.globalLightDirection);
	Brightness=(-1.0)*Scene.globalLightStrength*AbsoluteBrightness;
	OutColor=InColor;
	gl_Position=PushConstants.Model*PushConstants.ViewProjectionClip * Pos;
	TextureCoord=InTextureCoord;
}