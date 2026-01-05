#version 430 core

struct Light
{
    vec4 positionIntensity;   // xyz = position (point), w = intensity
    vec4 colorRadius;         // xyz = light color, w = radius (point)
    vec4 directionType;       // xyz = direction (dir), w = type (0 point, 1 dir)
    vec4 specularPadding;     // x = specularStrength
};

// SSBO: array of lights
layout(std430, binding = 0) buffer LightBuffer
{
    Light lights[];
};

uniform sampler2D diffuseTexture;
uniform sampler2D specularTexture;

uniform sampler2D shadowMap;

uniform float ambientIntensity;
uniform int lightCount;
uniform vec3 viewPos;

in vec4 fragPosLightSpace;
in vec3 fragPos;
in vec2 texCoord;
in vec3 normal;

out vec4 FragColor;

const float GAMMA = 2.2;

vec3 ToLinear(vec3 srgb)
{
    return pow(srgb, vec3(GAMMA));
}

vec3 ToSrgb(vec3 linear)
{
    return pow(linear, vec3(1.0 / GAMMA));
}

float ShadowFactor(vec4 fragPosLS, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;

    projCoords = projCoords * 0.5 + 0.5;

    if (projCoords.z > 1.0) return 0.0;

    float currentDepth = projCoords.z;

    float bias = max(0.0007 * (1.0 - dot(normal, -lightDir)), 0.0003);

    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0));
    float shadow = 0.0;
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float closestDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += (currentDepth - bias) > closestDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;
    return shadow;
}

void main()
{
    vec3 albedo = ToLinear(texture(diffuseTexture, texCoord).rgb);
    float specMask = texture(specularTexture, texCoord).r;

    vec3 N = normalize(normal);
    vec3 V = normalize(viewPos - fragPos);

    vec3 diffuseSum = vec3(0.0);
    vec3 specSum    = vec3(0.0);

    for (int i = 0; i < lightCount; i++)
    {
        int type = int(lights[i].directionType.w + 0.5);

        vec3 L;
        float attenuation = 1.0;
        float shadowAtten = 1.0;

        if (type == 0) // POINT
        {
            vec3 toLight = lights[i].positionIntensity.xyz - fragPos;
            float dist = length(toLight);

            float radius = lights[i].colorRadius.w;
            if (dist > radius) continue;

            L = toLight / max(dist, 1e-6);

            attenuation = 1.0 - (dist / max(radius, 1e-6));
            attenuation = clamp(attenuation, 0.0, 1.0);
            attenuation *= attenuation;
        }
        else // DIRECTIONAL
        {
            L = normalize(-lights[i].directionType.xyz);
            attenuation = 1.0;
            shadowAtten = 1.0 - ShadowFactor(fragPosLightSpace, N, L);
        }

        float NdotL = max(dot(N, L), 0.0);
        if (NdotL <= 0.0) continue;

        vec3 lightColor = lights[i].colorRadius.rgb;
        float intensity = lights[i].positionIntensity.w;

        vec3 radiance = lightColor * intensity * attenuation * shadowAtten;

        // Diffuse: albedo tinted by light
        diffuseSum += albedo * radiance * NdotL;

        // Specular: NOT multiplied by albedo (keeps highlights clean)
        vec3 H = normalize(V + L); // Blinn-Phong
        float spec = pow(max(dot(N, H), 0.0), 32.0);

        float specStrength = lights[i].specularPadding.x;
        specSum += radiance * (specStrength * specMask) * spec;
    }

    // Ambient should usually be tinted by albedo
    vec3 ambient = albedo * ambientIntensity;

    vec3 color = ambient + diffuseSum + specSum;
    FragColor = vec4(ToSrgb(max(color, vec3(0.0))), 1.0);
}
