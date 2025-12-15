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

uniform sampler2D texture0;

uniform float ambientIntensity;
uniform int lightCount;    
in vec3 fragPos;           
in vec2 texCoord;
in vec3 normal;

out vec4 FragColor;

void main()
{
    vec3 texColor = texture(texture0, texCoord).rgb;

    vec3 lighting = vec3(0.0);


    // Loop over all lights
    for (int i = 0; i < lightCount; i++)
    {
        vec3 norm = normalize(normal);

        // Vector from fragment to light
        vec3 lightDir = lights[i].position - fragPos;
        float distance = length(lightDir);

        // Skip if outside light radius
        if (distance > lights[i].radius)
            continue;

        lightDir = normalize(lightDir);

        // Diffuse (Lambert)
        float diff = max(dot(norm, lightDir), 0.0);

        // Smooth attenuation based on radius
        float attenuation = 1.0 - (distance / lights[i].radius);
        attenuation = clamp(attenuation, 0.0, 1.0);
        attenuation *= attenuation; // smoother falloff

        // Accumulate lighting
        lighting += lights[i].color * diff * lights[i].intensity * attenuation;
    }

    FragColor = vec4(texColor * (lighting + vec3(ambientIntensity)), 1.0);
}

