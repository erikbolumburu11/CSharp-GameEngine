#version 430 core

struct Light
{
    vec3 position;
    float intensity;
    vec3 color;
    float radius;
};

// SSBO: array of lights
layout(std430, binding = 0) buffer LightBuffer
{
    Light lights[];
};

uniform sampler2D texture0; // your 2D texture

uniform int lightCount;    // number of active lights
in vec3 fragPos;           // from vertex shader
in vec2 texCoord;

out vec4 FragColor;

void main()
{
    vec3 texColor = texture(texture0, texCoord).rgb;

    vec3 lighting = vec3(0.0);

    // Loop over all lights
    for (int i = 0; i < lightCount; i++)
    {
        vec3 L = lights[i].position - fragPos;
        float dist = length(L);
        float attenuation = 1.0 - clamp(dist / lights[i].radius, 0.0, 1.0);

        lighting += lights[i].color * lights[i].intensity * attenuation;
    }

    FragColor = vec4(texColor * lighting, 1.0);
}

