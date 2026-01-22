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
uniform sampler2D normalTexture;
uniform sampler2D heightTexture;
uniform int useCombinedMR;
uniform float heightScale;
uniform sampler2D environmentMap;
uniform sampler2D brdfLut;
uniform int useEnvironmentMap;

uniform sampler2D shadowMap;

uniform float ambientIntensity;
uniform float iblSpecularIntensity;
uniform int lightCount;
uniform vec3 viewPos;

in vec3 fragPos;
in vec2 texCoord;
in vec3 normal;
in vec4 fragPosLightSpace;

out vec4 FragColor;

const float PI = 3.14159265359;
const float MinParallaxLayers = 8.0;
const float MaxParallaxLayers = 32.0;

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

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    vec3 oneMinusRoughness = vec3(1.0 - roughness);
    return F0 + (max(oneMinusRoughness, F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

vec2 EnvMapUV(vec3 dir)
{
    vec3 d = normalize(dir);
    float u = atan(d.z, d.x) / (2.0 * PI) + 0.5;
    float v = asin(clamp(d.y, -1.0, 1.0)) / PI + 0.5;
    return vec2(u, v);
}

mat3 CotangentFrame(vec3 N, vec3 p, vec2 uv)
{
    vec3 dp1 = dFdx(p);
    vec3 dp2 = dFdy(p);
    vec2 duv1 = dFdx(uv);
    vec2 duv2 = dFdy(uv);

    vec3 dp2perp = cross(dp2, N);
    vec3 dp1perp = cross(N, dp1);
    vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;

    float invMax = inversesqrt(max(dot(T, T), dot(B, B)));
    return mat3(T * invMax, B * invMax, N);
}

vec2 ParallaxOcclusionMapping(vec2 texCoords, vec3 viewDirTangent)
{
    float ndotv = abs(viewDirTangent.z);
    float numLayers = mix(MaxParallaxLayers, MinParallaxLayers, ndotv);
    float layerDepth = 1.0 / numLayers;
    float currentLayerDepth = 0.0;

    vec2 P = viewDirTangent.xy / max(viewDirTangent.z, 0.001) * heightScale;
    vec2 deltaTexCoords = P / numLayers;

    vec2 currentTexCoords = texCoords;
    float currentDepthMapValue = 1.0 - texture(heightTexture, currentTexCoords).r;

    while (currentLayerDepth < currentDepthMapValue)
    {
        currentTexCoords -= deltaTexCoords;
        currentDepthMapValue = 1.0 - texture(heightTexture, currentTexCoords).r;
        currentLayerDepth += layerDepth;
    }

    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;
    float prevDepth = 1.0 - texture(heightTexture, prevTexCoords).r;
    float afterDepth = currentDepthMapValue - currentLayerDepth;
    float beforeDepth = prevDepth - (currentLayerDepth - layerDepth);
    float weight = afterDepth / (afterDepth - beforeDepth);
    return mix(currentTexCoords, prevTexCoords, clamp(weight, 0.0, 1.0));
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

    vec3 N = normalize(normal);
    mat3 TBN = CotangentFrame(N, fragPos, texCoord);
    vec3 V = normalize(viewPos - fragPos);
    vec3 viewDirTangent = normalize(transpose(TBN) * V);

    vec2 tiledTexCoord = texCoord;
    vec2 localTexCoord = fract(tiledTexCoord);
    vec2 tileOffset = tiledTexCoord - localTexCoord;

    vec2 parallaxLocal = localTexCoord;
    if (heightScale > 0.0001)
        parallaxLocal = ParallaxOcclusionMapping(localTexCoord, viewDirTangent);

    vec2 parallaxTexCoord = parallaxLocal + tileOffset;

    vec3 albedo = texture(diffuseTexture, parallaxTexCoord).rgb;
    float metallic;
    float roughness;
    if (useCombinedMR == 1)
    {
        vec4 mrSample = texture(metallicRoughnessTexture, parallaxTexCoord);
        metallic = mrSample.b;
        roughness = mrSample.g;
    }
    else
    {
        metallic = texture(metallicTexture, parallaxTexCoord).r;
        roughness = texture(roughnessTexture, parallaxTexCoord).r;
    }

    metallic = clamp(metallic, 0.0, 1.0);
    roughness = clamp(roughness, 0.04, 1.0);
    float ao = texture(aoTexture, parallaxTexCoord).r;

    vec3 mapN = texture(normalTexture, parallaxTexCoord).xyz * 2.0 - 1.0;
    N = normalize(TBN * mapN);
    float NdotV = max(dot(N, V), 0.0);
    vec3 F0 = mix(vec3(0.04), albedo, metallic);

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
        float HdotV = max(dot(H, V), 0.0);

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
    if (useEnvironmentMap == 1)
    {
        float maxLod = float(textureQueryLevels(environmentMap) - 1);
        vec3 R = reflect(-V, N);
        vec3 diffuseIbl = textureLod(environmentMap, EnvMapUV(N), maxLod).rgb;
        vec3 specIbl = textureLod(environmentMap, EnvMapUV(R), roughness * maxLod).rgb * iblSpecularIntensity;

        vec2 brdf = texture(brdfLut, vec2(NdotV, roughness)).rg;
        vec3 F = FresnelSchlickRoughness(NdotV, F0, roughness);
        vec3 kS = F;
        vec3 kD = (vec3(1.0) - kS) * (1.0 - metallic);
        vec3 specular = specIbl * (F0 * brdf.x + brdf.y);

        ambient = (kD * albedo * diffuseIbl + specular) * ambientIntensity * ao;
    }
    FragColor = vec4(ambient + Lo, 1.0);
}
