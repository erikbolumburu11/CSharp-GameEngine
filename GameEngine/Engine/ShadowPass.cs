using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public sealed class ShadowPass
    {
        readonly int shadowMapSize;

        ShadowMap? shadowMap;
        Shader? shadowDepthShader;

        public ShadowPass(int shadowMapSize = 2048)
        {
            this.shadowMapSize = shadowMapSize;
        }

        public bool Execute(
            ShaderManager shaderManager,
            GameObjectManager gameObjectManager,
            LightManager lightManager,
            out Matrix4 lightSpaceMatrix
        )
        {
            lightSpaceMatrix = Matrix4.Identity;

            Light? dirLight = lightManager.lights.FirstOrDefault(l => l.type == LightType.Directional);
            if (dirLight == null)
                return false;

            Vector3 lightDir = Vector3.Transform(-Vector3.UnitZ, dirLight.gameObject.transform.WorldRotation).Normalized();
            Vector3 sceneCenter = Vector3.Zero;
            lightSpaceMatrix = CreateDirectionalLightSpaceMatrix(lightDir, sceneCenter);

            shadowMap ??= new ShadowMap(shadowMapSize);
            shadowMap.BindForWriting();
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Front);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(2.0f, 4.0f);

            string depthVert = Util.GetProjectDir() + "/Shaders/Depth/shadow_depth.vert";
            string depthFrag = Util.GetProjectDir() + "/Shaders/Depth/shadow_depth.frag";
            shadowDepthShader ??= shaderManager.Get(depthVert, depthFrag);

            shadowDepthShader.Use();
            shadowDepthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

            foreach (GameObject go in gameObjectManager.gameObjects)
            {
                MeshRenderer? mr = go.GetComponent<MeshRenderer>();
                if (mr == null || mr.vao == null || mr.vertexCount <= 0) continue;

                Matrix4 model = RenderMath.CreateModelMatrix(go.transform);
                shadowDepthShader.SetMatrix4("model", model);

                mr.vao.Bind();
                GL.DrawArrays(PrimitiveType.Triangles, 0, mr.vertexCount);
                mr.vao.Unbind();
            }

            GL.CullFace(TriangleFace.Back);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return true;
        }

        public void BindForReading(TextureUnit unit)
        {
            shadowMap?.BindForReading(unit);
        }

        static Matrix4 CreateDirectionalLightSpaceMatrix(Vector3 lightDir, Vector3 sceneCenter)
        {
            float distanceBack = 20f;
            Vector3 lightPos = sceneCenter - lightDir.Normalized() * distanceBack;

            float orthoSize = 7;
            float near = 1f;
            float far = 40f;

            var lightView = Matrix4.LookAt(lightPos, sceneCenter, Vector3.UnitY);
            var lightProj = Matrix4.CreateOrthographic(orthoSize, orthoSize, near, far);

            return lightView * lightProj;
        }
    }
}
