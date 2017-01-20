#version 400
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
layout(std140,binding=0) uniform RenderingInfo {
	mat4 Model;
	mat4 View;
	mat4 Projection;
	mat4 Clip;
	vec3 GlobalLightDirection;
};

layout location=0 in vec3 Pos;
layout location=1 in vec3 InColor;
layout location=2 in vec3 Normal;

layout location=0 out vec3 OutColor;
out gl_PerVertex {
	vec4 gl_Position;
}

void main(){
	OutColor=InColor+InColor*0.2*(-dot(Normal,RenderingInfo.GlobalLightDirection)/(abs(Normal)*abs(GlobalLightDirection));
	gl_Position=RenderingInfo.Clip * RenderingInfo.Projection * RenderingInfo.View * RenderngInfo.Model * Pos;
}