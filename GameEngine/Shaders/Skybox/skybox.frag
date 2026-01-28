#version 430 core

in vec3 vDir;
out vec4 FragColor;

uniform sampler2D skyboxTexture;
uniform float exposure;
uniform int flipV;

const float PI = 3.14159265359;

vec2 EnvMapUV(vec3 dir)
{
    vec3 d = normalize(dir);
    float u = atan(d.z, d.x) / (2.0 * PI) + 0.5;
    float v = asin(clamp(d.y, -1.0, 1.0)) / PI + 0.5;
    if (flipV == 1)
        v = 1.0 - v;
    return vec2(u, v);
}

void main()
{
    vec3 hdrColor = texture(skyboxTexture, EnvMapUV(vDir)).rgb;
    vec3 mapped = vec3(1.0) - exp(-hdrColor * max(exposure, 0.0));
    FragColor = vec4(mapped, 1.0);
}
