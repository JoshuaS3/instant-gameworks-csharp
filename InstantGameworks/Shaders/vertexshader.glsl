﻿#version 450 core
#line 2 "vertexshader.glsl"


out vec4 fragPos;
out vec3 fragNorm;
out vec3 eye;
layout (location = 0) in vec4 position;
layout (location = 1) in vec3 normal;
layout (location = 3) uniform mat4 modelView;
layout (location = 4) uniform mat4 projection;

void main(void)
{
	fragPos = position;
	fragNorm = normalize(normal);
	eye = vec3(modelView * position);

	gl_Position = projection * modelView * position;
}