#version 430 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
>>>>>>> Stashed changes
uniform mat4 lightSpaceMatrix;

uniform vec2 uvTiling;
uniform vec2 uvOffset;

>>>>>>> Stashed changes
out vec3 normal;
out vec3 fragPos;       // world-space position
out vec2 texCoord;      
out vec4 fragPosLightSpace;

void main()
{
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection; 
    fragPos = vec3(vec4(aPosition, 1.0f) * model);
    normal = mat3(transpose(inverse(model))) * aNormal;
<<<<<<< Updated upstream
    texCoord = aTexCoord;
=======
    texCoord = aTexCoord * uvTiling + uvOffset;
    fragPosLightSpace = lightSpaceMatrix * fragPos;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
}