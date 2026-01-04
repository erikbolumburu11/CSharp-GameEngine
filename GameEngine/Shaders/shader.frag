#version 430 core

struct Light
{
    vec3 position;
    float intensity;
    vec3 color;
    float radius;
    float specularStrength;
};

// SSBO: array of lights
layout(std430, binding = 0) buffer LightBuffer
{
    Light lights[];
};

uniform sampler2D diffuseTexture;
uniform sampler2D specularTexture;

uniform float ambientIntensity;
uniform int lightCount;    
uniform vec3 viewPos;

in vec3 fragPos;           
in vec2 texCoord;
in vec3 normal;

out vec4 FragColor;

void main()
{
    vec3 diffTexColor = texture(diffuseTexture, texCoord).rgb;
    vec3 specTexColor = texture(specularTexture, texCoord).rgb;

    vec3 lighting = vec3(0.0);

    for (int i = 0; i < lightCount; i++)
    {
        vec3 norm = normalize(normal);

        vec3 lightDir = lights[i].position - fragPos;
        float distance = length(lightDir);

        if (distance > lights[i].radius)
            continue;

        lightDir = normalize(lightDir);

        float diff = max(dot(norm, lightDir), 0.0);

        float attenuation = 1.0 - (distance / lights[i].radius);
        attenuation = clamp(attenuation, 0.0, 1.0);
        attenuation *= attenuation; // smoother falloff

        vec3 viewDir = normalize(viewPos - fragPos);
        vec3 reflectDir = reflect(-lightDir, norm);

        float specMask = specTexColor.r;
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = lights[i].specularStrength * spec * specMask * lights[i].color;

        lighting += lights[i].color * diff * lights[i].intensity * attenuation + specular * attenuation;
    }

    FragColor = vec4(diffTexColor * (lighting + vec3(ambientIntensity)), 1.0);
}

