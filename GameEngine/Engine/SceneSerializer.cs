using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameEngine.Engine
{
    public static class SceneSerializer
    {
        public static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static void SaveScene(GameObjectManager gameObjectManager, Scene gameScene, string path)
        {
            SceneData scene = new();

            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                GameObjectData gameObjectData = new GameObjectData
                {
                    name = gameObject.name,
                    transform = new TransformData
                    {
                        positionX = gameObject.transform.position.X,
                        positionY = gameObject.transform.position.Y,
                        positionZ = gameObject.transform.position.Z,

                        rotationX = gameObject.transform.rotation.X,
                        rotationY = gameObject.transform.rotation.Y,
                        rotationZ = gameObject.transform.rotation.Z,

                        scaleX = gameObject.transform.scale.X,
                        scaleY = gameObject.transform.scale.Y,
                        scaleZ = gameObject.transform.scale.Z,
                    },
                    components = SerializeComponents(gameObject)
                };

                scene.gameObjects.Add(gameObjectData);
            }

            scene.ambientLightIntensity = gameScene.ambientLightIntensity;

            scene.skyboxColorR = gameScene.skyboxColor.R;
            scene.skyboxColorG = gameScene.skyboxColor.G;
            scene.skyboxColorB = gameScene.skyboxColor.B;

            string json = JsonSerializer.Serialize(scene, options);

            Console.WriteLine("Saving " + scene.gameObjects.Count + " objects");
            File.WriteAllText(path, json);
        }

        private static List<ComponentData> SerializeComponents(GameObject gameObject)
        {
            List<ComponentData> componentData = new();
            foreach (var c in gameObject.Components)
            {
                if (c is IComponentSerializable serializable)
                {
                    componentData.Add(new ComponentData
                    {
                        type = ComponentTypeRegistry.Get(c.GetType()),
                        fields = serializable.Save()
                    });
                }
            }

            return componentData;
        }

        public static void LoadScene(GameObjectManager gameObjectManager, Scene gameScene, string path)
        {
            string json = File.ReadAllText(path);
            SceneData scene = JsonSerializer.Deserialize<SceneData>(json, options);

            gameObjectManager.Clear();

            foreach (GameObjectData data in scene.gameObjects)
            {
                GameObject gameObject = gameObjectManager.CreateCube();

                gameObject.transform.position = new Vector3
                (
                    data.transform.positionX,
                    data.transform.positionY,
                    data.transform.positionZ
                );

                gameObject.transform.rotation = new Quaternion
                (
                    data.transform.rotationX,
                    data.transform.rotationY,
                    data.transform.rotationZ
                );

                gameObject.transform.scale = new Vector3
                (
                    data.transform.scaleX,
                    data.transform.scaleY,
                    data.transform.scaleZ
                );

                DeserializeComponents(gameObject, data.components);
            }

            gameScene.ambientLightIntensity = scene.ambientLightIntensity;
            gameScene.skyboxColor = Color.FromArgb(255, scene.skyboxColorR, scene.skyboxColorG, scene.skyboxColorB);
        }

        private static void DeserializeComponents(GameObject gameObject, List<ComponentData> components)
        {
            foreach (ComponentData compData in components)
            {
                var type = ComponentTypeRegistry.Get(compData.type);
                var comp = (Component)Activator.CreateInstance(type);

                if (comp is IComponentSerializable serializable)
                    serializable.Load(compData.fields);

                gameObject.AddComponent(comp);
            }
        }
    }
}

public class SceneData
{
    public List<GameObjectData> gameObjects = new();

    public float ambientLightIntensity;

    public int skyboxColorR;
    public int skyboxColorG;
    public int skyboxColorB;
}

public class GameObjectData
{
    public string name;
    public TransformData transform;
    public List<ComponentData> components = new();
}

public class TransformData
{
    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public float scaleX = 1f;
    public float scaleY = 1f;
    public float scaleZ = 1f;
}

public class ComponentData
{
    public string type;
    public Dictionary<string, object> fields;
}
