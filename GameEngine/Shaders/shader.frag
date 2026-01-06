#version 430 core
#define DEBUG_OUTPUT_SHADOWMAP 1

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

in vec3 fragPos;
in vec2 texCoord;
in vec3 normal;
in vec4 fragPosLightSpace;

out vec4 FragColor;

float ShadowFactor(vec4 fragPosLS, vec3 normal, vec3 lightDir)
{
    // Perspective divide (for ortho, w is 1, still fine)
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;

    // Map from [-1,1] to [0,1]
    projCoords = projCoords * 0.5 + 0.5;

    // Outside shadow map = not in shadow
    if (projCoords.z > 1.0) return 0.0;

    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;

    // Bias reduces shadow acne (scale bias by surface angle)
    float bias = max(0.0007 * (1.0 - dot(normal, -lightDir)), 0.0003);

    // Basic hard shadow
    float shadow = (currentDepth - bias) > closestDepth ? 1.0 : 0.0;
    return shadow;
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

            shadow = ShadowFactor(fragPosLightSpace, N, lights[i].directionType.xyz);
        }

        float NdotL = max(dot(N, L), 0.0);
        if (NdotL <= 0.0) continue;

        vec3 lightColor = lights[i].colorRadius.rgb;
        float intensity = lights[i].positionIntensity.w;
        float visibility = 1.0 - shadow;


        vec3 radiance = lightColor * intensity * attenuation;

        // Diffuse: albedo tinted by light
        diffuseSum += albedo * radiance * NdotL * visibility;

        // Specular: NOT multiplied by albedo (keeps highlights clean)
        vec3 H = normalize(V + L); // Blinn-Phong
        float spec = pow(max(dot(N, H), 0.0), 32.0);

        float specStrength = lights[i].specularPadding.x;
        specSum += radiance * (specStrength * specMask) * spec * visibility;
    }

    // Ambient should usually be tinted by albedo
    vec3 ambient = albedo * ambientIntensity;

    FragColor = vec4(ambient + diffuseSum + specSum, 1.0);
}
