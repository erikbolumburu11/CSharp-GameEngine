#version 430 core
#define DEBUG_OUTPUT_SHADOWMAP 0

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
uniform sampler2D metallicRoughnessTexture;
uniform sampler2D metallicTexture;
uniform sampler2D roughnessTexture;
uniform sampler2D aoTexture;
uniform int useCombinedMR;

uniform sampler2D shadowMap;

uniform float ambientIntensity;
uniform int lightCount;
uniform vec3 viewPos;

in vec3 fragPos;
in vec2 texCoord;
in vec3 normal;
in vec4 fragPosLightSpace;

out vec4 FragColor;

const float PI = 3.14159265359;

float ShadowFactor(vec4 fragPosLS, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;

    projCoords = projCoords * 0.5 + 0.5;

    if (projCoords.z > 1.0) return 0.0;

    float currentDepth = projCoords.z;
    float NdotL = max(dot(normal, lightDir), 0.0);

    float bias = max(0.005 * (1.0 - dot(normal, lightDir)), 0.0005);


    float shadow = 0.0;

    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0));
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow = shadow / 9.0;

    shadow = pow(shadow, 0.7);
    return shadow * 0.7;
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    return a2 / max(PI * denom * denom, 1e-6);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    return NdotV / max(NdotV * (1.0 - k) + k, 1e-6);
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggxV = GeometrySchlickGGX(NdotV, roughness);
    float ggxL = GeometrySchlickGGX(NdotL, roughness);
    return ggxV * ggxL;
}

vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

void main()
{
#if DEBUG_OUTPUT_SHADOWMAP
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float depth = 1.0;
    if (projCoords.z <= 1.0)
        depth = texture(shadowMap, projCoords.xy).r;

    FragColor = vec4(vec3(depth), 1.0);
    return;
#endif

    vec3 albedo = texture(diffuseTexture, texCoord).rgb;
    float metallic;
    float roughness;
    if (useCombinedMR == 1)
    {
        vec4 mrSample = texture(metallicRoughnessTexture, texCoord);
        metallic = mrSample.b;
        roughness = mrSample.g;
    }
    else
    {
        metallic = texture(metallicTexture, texCoord).r;
        roughness = texture(roughnessTexture, texCoord).r;
    }

    metallic = clamp(metallic, 0.0, 1.0);
    roughness = clamp(roughness, 0.04, 1.0);
    float ao = texture(aoTexture, texCoord).r;

    vec3 N = normalize(normal);
    vec3 V = normalize(viewPos - fragPos);

    vec3 Lo = vec3(0.0);

    for (int i = 0; i < lightCount; i++)
    {
        int type = int(lights[i].directionType.w + 0.5);

        vec3 L;
        float attenuation = 1.0;
        float shadow = 0.0;


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
            // directionType.xyz = direction the light points (from light toward scene)
            // Use -dir as "from fragment to light"
            L = normalize(-lights[i].directionType.xyz);
            attenuation = 1.0;

            shadow = ShadowFactor(fragPosLightSpace, N, L);
        }

        float NdotL = max(dot(N, L), 0.0);
        if (NdotL <= 0.0) continue;

        vec3 lightColor = lights[i].colorRadius.rgb;
        float intensity = lights[i].positionIntensity.w;
        float visibility = 1.0 - shadow;


        vec3 radiance = lightColor * intensity * attenuation;

        vec3 H = normalize(V + L);
        float NdotV = max(dot(N, V), 0.0);
        float HdotV = max(dot(H, V), 0.0);

        vec3 F0 = mix(vec3(0.04), albedo, metallic);
        float D = DistributionGGX(N, H, roughness);
        float G = GeometrySmith(N, V, L, roughness);
        vec3  F = FresnelSchlick(HdotV, F0);

        vec3 numerator = D * G * F;
        float denom = max(4.0 * NdotV * NdotL, 0.001);
        vec3 specular = numerator / denom;

        vec3 kS = F;
        vec3 kD = (vec3(1.0) - kS) * (1.0 - metallic);
        vec3 diffuse = kD * albedo / PI;

        Lo += (diffuse + specular) * radiance * NdotL * visibility;
    }

    vec3 ambient = albedo * ambientIntensity * ao;
    FragColor = vec4(ambient + Lo, 1.0);
}
