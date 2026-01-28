using GameEngine.Engine.Components;
using SharpGLTF.Schema2;
using System.Numerics;

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

            // Import materials using your existing importer
            var materialMap = GltfMaterialImporter.ImportMaterials(model, modelPath);
            AssetDatabase.RegisterModelPrimitives(modelGuid, model, modelPath);

            // Create root GameObject
            string rootName = "Model";
            if (model.DefaultScene != null && !string.IsNullOrWhiteSpace(model.DefaultScene.Name))
                rootName = model.DefaultScene.Name;

            GameObject root = gameObjectManager.CreateGameObject(rootName);

            // Process the scene hierarchy starting from root nodes
            if (model.DefaultScene != null)
            {
                foreach (var node in model.DefaultScene.VisualChildren)
                {
                    ProcessNode(node, root.transform, modelGuid, materialMap);
                }
            }
            else
            {
                // Fallback: if no default scene, process all root nodes
                foreach (var node in model.LogicalNodes.Where(n => n.VisualParent == null))
                {
                    ProcessNode(node, root.transform, modelGuid, materialMap);
                }
            }

            return root;
        }

        private void ProcessNode(Node node, Transform parentTransform, Guid modelGuid, IReadOnlyDictionary<SharpGLTF.Schema2.Material, Guid> materialMap)
        {
            // Create GameObject for this node
            string nodeName = !string.IsNullOrWhiteSpace(node.Name) 
                ? node.Name 
                : $"Node_{node.LogicalIndex}";
            
            GameObject nodeObject = gameObjectManager.CreateGameObject(nodeName);
            nodeObject.transform.parent = parentTransform;

            // Apply the node's transformation
            ApplyNodeTransform(node, nodeObject.transform);

            // If this node has a mesh, create child GameObjects for each primitive
            if (node.Mesh != null)
            {
                var mesh = node.Mesh;
                string meshName = !string.IsNullOrWhiteSpace(mesh.Name) 
                    ? mesh.Name 
                    : $"Mesh_{mesh.LogicalIndex}";
                
                foreach (var primitive in mesh.Primitives)
                {
                    // Create a descriptive name combining node, mesh, and material info
                    string materialName = primitive.Material?.Name ?? "default";
                    string primitiveName = $"{nodeName}_{materialName}_{primitive.LogicalIndex}";
                    
                    GameObject primitiveObject = gameObjectManager.CreateGameObject(primitiveName);
                    primitiveObject.transform.parent = nodeObject.transform;
                    
                    // Add MeshRenderer component with mesh and material
                    var renderer = primitiveObject.AddComponent<MeshRenderer>();
                    renderer.mesh = AssetDatabase.GetMeshPrimitiveGuid(
                        modelGuid,
                        mesh.LogicalIndex,
                        primitive.LogicalIndex
                    );
                    
                    // Assign material if available (primitive.Material is SharpGLTF.Schema2.Material)
                    if (primitive.Material != null 
                        && materialMap.TryGetValue(primitive.Material, out var materialGuid))
                    {
                        renderer.material = materialGuid;
                    }
                }
            }

            // Recursively process child nodes
            foreach (var childNode in node.VisualChildren)
            {
                ProcessNode(childNode, nodeObject.transform, modelGuid, materialMap);
            }
        }

        private void ApplyNodeTransform(Node node, Transform transform)
        {
            // Get the local transform matrix from the node
            // SharpGLTF handles both matrix and TRS representations internally
            var matrix = node.LocalTransform.Matrix;

            // Decompose the 4x4 matrix into Translation, Rotation, Scale
            if (Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
            {
                // Apply translation
                transform.localPosition.X = translation.X;
                transform.localPosition.Y = translation.Y;
                transform.localPosition.Z = translation.Z;

                // Apply rotation (quaternion)
                transform.localRotation.X = rotation.X;
                transform.localRotation.Y = rotation.Y;
                transform.localRotation.Z = rotation.Z;
                transform.localRotation.W = rotation.W;

                // Apply scale
                transform.localScale.X = scale.X;
                transform.localScale.Y = scale.Y;
                transform.localScale.Z = scale.Z;
            }
            else
            {
                // Matrix decomposition failed (extremely rare, but handle gracefully)
                // This can happen if the matrix is not a valid TRS matrix (e.g., has shear)
                Console.WriteLine($"Warning: Failed to decompose transform for node '{node.Name}'. Using identity transform.");
            }
        }
    }
}