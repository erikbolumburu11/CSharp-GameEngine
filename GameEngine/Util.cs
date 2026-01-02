using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Util
    {
        public static float[] cubeVertices =
        {
            // ---- Back face (-Z) ----
            -0.5f,-0.5f,-0.5f,  0f,0f,   0f,0f,-1f,
            0.5f,-0.5f,-0.5f,  1f,0f,   0f,0f,-1f,
            0.5f, 0.5f,-0.5f,  1f,1f,   0f,0f,-1f,
            0.5f, 0.5f,-0.5f,  1f,1f,   0f,0f,-1f,
            -0.5f, 0.5f,-0.5f,  0f,1f,   0f,0f,-1f,
            -0.5f,-0.5f,-0.5f,  0f,0f,   0f,0f,-1f,

            // ---- Front face (+Z) ----
            -0.5f,-0.5f, 0.5f,  0f,0f,   0f,0f,1f,
            0.5f,-0.5f, 0.5f,  1f,0f,   0f,0f,1f,
            0.5f, 0.5f, 0.5f,  1f,1f,   0f,0f,1f,
            0.5f, 0.5f, 0.5f,  1f,1f,   0f,0f,1f,
            -0.5f, 0.5f, 0.5f,  0f,1f,   0f,0f,1f,
            -0.5f,-0.5f, 0.5f,  0f,0f,   0f,0f,1f,

            // ---- Left face (-X) ----
            -0.5f, 0.5f, 0.5f,  1f,0f,  -1f,0f,0f,
            -0.5f, 0.5f,-0.5f,  1f,1f,  -1f,0f,0f,
            -0.5f,-0.5f,-0.5f,  0f,1f,  -1f,0f,0f,
            -0.5f,-0.5f,-0.5f,  0f,1f,  -1f,0f,0f,
            -0.5f,-0.5f, 0.5f,  0f,0f,  -1f,0f,0f,
            -0.5f, 0.5f, 0.5f,  1f,0f,  -1f,0f,0f,

            // ---- Right face (+X) ----
            0.5f, 0.5f, 0.5f,  1f,0f,   1f,0f,0f,
            0.5f, 0.5f,-0.5f,  1f,1f,   1f,0f,0f,
            0.5f,-0.5f,-0.5f,  0f,1f,   1f,0f,0f,
            0.5f,-0.5f,-0.5f,  0f,1f,   1f,0f,0f,
            0.5f,-0.5f, 0.5f,  0f,0f,   1f,0f,0f,
            0.5f, 0.5f, 0.5f,  1f,0f,   1f,0f,0f,

            // ---- Bottom face (-Y) ----
            -0.5f,-0.5f,-0.5f,  0f,1f,   0f,-1f,0f,
            0.5f,-0.5f,-0.5f,  1f,1f,   0f,-1f,0f,
            0.5f,-0.5f, 0.5f,  1f,0f,   0f,-1f,0f,
            0.5f,-0.5f, 0.5f,  1f,0f,   0f,-1f,0f,
            -0.5f,-0.5f, 0.5f,  0f,0f,   0f,-1f,0f,
            -0.5f,-0.5f,-0.5f,  0f,1f,   0f,-1f,0f,

            // ---- Top face (+Y) ----
            -0.5f, 0.5f,-0.5f,  0f,1f,   0f,1f,0f,
            0.5f, 0.5f,-0.5f,  1f,1f,   0f,1f,0f,
            0.5f, 0.5f, 0.5f,  1f,0f,   0f,1f,0f,
            0.5f, 0.5f, 0.5f,  1f,0f,   0f,1f,0f,
            -0.5f, 0.5f, 0.5f,  0f,0f,   0f,1f,0f,
            -0.5f, 0.5f,-0.5f,  0f,1f,   0f,1f,0f
        };


        public static string GetProjectDir()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            return projectDir;
        }

        public static string GetDefaultVertPath() => GetProjectDir() + "/Shaders/shader.vert";
        public static string GetDefaultFragPath() => GetProjectDir() + "/Shaders/shader.frag";


        public static object GetObjectValue(object obj)
        {
            var typeOfObject = ((JsonElement)obj).ValueKind;

            switch (typeOfObject)
            {
                case JsonValueKind.Number:
                    return float.Parse(obj.ToString());
                default:
                    return obj.ToString();
            }
        }
    }
}
