using GameEngine.Engine.Components;
using SharpGLTF.Schema2;

namespace GameEngine.Engine
{
    public class GltfInstantiator
    {
        private readonly GameObjectManager gameObjectManager;

        public GltfInstantiator(GameObjectManager gameObjectManager)
        {
            this.gameObjectManager = gameObjectManager ?? throw new ArgumentNullException(nameof(gameObjectManager));
        }

        public GameObject Instantiate(ModelRoot model, Guid modelGuid, string? modelPath = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var materialMap = GltfMaterialImporter.ImportMaterials(model, modelPath);
            AssetDatabase.RegisterModelPrimitives(modelGuid, model, modelPath);

            string rootName = "Model";
            if (model.DefaultScene != null && !string.IsNullOrWhiteSpace(model.DefaultScene.Name))
                rootName = model.DefaultScene.Name;

            GameObject root = gameObjectManager.CreateGameObject(rootName);

            foreach (var mesh in model.LogicalMeshes)
            {
                string meshName = !string.IsNullOrWhiteSpace(mesh.Name)
                    ? mesh.Name
                    : $"Mesh_{mesh.LogicalIndex}";

                foreach (var primitive in mesh.Primitives)
                {
                    string childName = $"{meshName}_Prim_{primitive.LogicalIndex}";
                    GameObject child = gameObjectManager.CreateGameObject(childName);
                    child.transform.parent = root.transform;
                    var renderer = child.AddComponent<MeshRenderer>();
                    renderer.mesh = AssetDatabase.GetMeshPrimitiveGuid(
                        modelGuid,
                        mesh.LogicalIndex,
                        primitive.LogicalIndex
                    );
                    if (primitive.Material != null
                        && materialMap.TryGetValue(primitive.Material, out var materialGuid))
                    {
                        renderer.material = materialGuid;
                    }
                }
            }

            return root;
        }
    }
}
