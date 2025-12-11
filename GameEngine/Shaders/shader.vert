#version 430 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 fragPos;       // world-space position
out vec2 texCoord;      // pass through texture coordinates

void main()
{
    // Transform vertex to world space
    vec4 worldPos = model * vec4(aPosition, 1.0);
    fragPos = worldPos.xyz;

    // Pass texture coords to fragment shader
    texCoord = aTexCoord;

    // Final clip-space position
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection; 
}