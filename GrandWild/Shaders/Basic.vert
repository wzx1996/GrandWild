#version 400
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
layout(std140,binding=0) uniform RenderingInfoStructure {
	mat4 Model;
	mat4 View;
	mat4 Projection;
	mat4 Clip;
	vec4 GlobalLightDirection;
} RenderingInfo;

layout (location=0) in vec4 Pos;
layout (location=1) in vec4 InColor;
layout (location=2) in vec4 Normal;

layout (location=0) out vec4 OutColor;
out gl_PerVertex {
	vec4 gl_Position;
};

void main(){
	OutColor=InColor+InColor*0.2*(-1.0)*(dot(Normal,RenderingInfo.GlobalLightDirection)/(abs(Normal)*abs(RenderingInfo.GlobalLightDirection)));
	mat4 mvpc=RenderingInfo.Clip * RenderingInfo.Projection * RenderingInfo.View * RenderingInfo.Model;
	gl_Position=mvpc * Pos;
}