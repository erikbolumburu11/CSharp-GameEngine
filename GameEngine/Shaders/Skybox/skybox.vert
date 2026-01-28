#version 430 core
layout(location = 0) in vec3 aPosition;

out vec3 vDir;

uniform mat4 view;
uniform mat4 projection;

void main()
{
    vDir = aPosition;
    vec4 pos = vec4(aPosition, 1.0) * view * projection;
    gl_Position = pos.xyww;
}
