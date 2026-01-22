using OpenTK.Mathematics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design;

namespace GameEngine.Engine {
    public static class SceneSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static void SaveScene(GameObjectManager gameObjectManager, Scene gameScene, string path, bool relativePath)
        {
            SceneDto dto = new SceneDto();

            string savePath = path;
            if (relativePath)
            {
                if (ProjectContext.Current == null)
                    throw new InvalidOperationException("Cannot save a relative scene path without an active project.");

                dto.RelPath = path;
                savePath = ProjectContext.Current.Paths.ToAbsolute(path);
            }
            else
            {
                dto.RelPath = path;
            }

            foreach (var gameObject in gameObjectManager.gameObjects)
                dto.GameObjects.Add(ToDto(gameObject));

            dto.AmbientLightIntensity = gameScene.ambientLightIntensity;
            dto.SkyboxColorR = gameScene.skyboxColor.R;
            dto.SkyboxColorG = gameScene.skyboxColor.G;
            dto.SkyboxColorB = gameScene.skyboxColor.B;
            dto.SkyboxHdrPath = gameScene.skyboxHdrPath;
            dto.SkyboxExposure = gameScene.skyboxExposure;
            dto.SkyboxFlipV = gameScene.skyboxFlipV;
            dto.IblSpecularIntensity = gameScene.iblSpecularIntensity;

            string json = JsonSerializer.Serialize(dto, Options);
            File.WriteAllText(savePath, json);
        }

        public static void LoadScene(GameObjectManager gameObjectManager, Scene gameScene, string absolutePath)
        {
            string json = File.ReadAllText(absolutePath);
            var dto = JsonSerializer.Deserialize<SceneDto>(json, Options)
                        ?? throw new InvalidOperationException("Invalid scene file.");

            gameObjectManager.Clear();

            Dictionary<Guid, GameObject> map = new();

            foreach (var goDto in dto.GameObjects)
            {
                var go = gameObjectManager.CreateGameObject(goDto.Name, goDto.Id);
                map[goDto.Id] = go;
            }

            foreach (var goDto in dto.GameObjects)
            {
                if (map.TryGetValue(goDto.Parent, out var parent))
                {
                    map[goDto.Id].transform.parent = parent.transform;
                }
            }

            foreach (var goDto in dto.GameObjects)
            {
                var go = map[goDto.Id];

                go.transform.WorldPosition = new Vector3(goDto.Transform.PositionX, goDto.Transform.PositionY, goDto.Transform.PositionZ);
                go.transform.WorldRotation = new Quaternion(
                    goDto.Transform.RotationX,
                    goDto.Transform.RotationY,
                    goDto.Transform.RotationZ,
                    goDto.Transform.RotationW
                );
                go.transform.WorldScale = new Vector3(goDto.Transform.ScaleX, goDto.Transform.ScaleY, goDto.Transform.ScaleZ);

                DeserializeComponents(go, goDto.Components);
            }

            gameScene.ambientLightIntensity = dto.AmbientLightIntensity;
            gameScene.skyboxColor = Color.FromArgb(255, dto.SkyboxColorR, dto.SkyboxColorG, dto.SkyboxColorB);
            gameScene.skyboxHdrPath = dto.SkyboxHdrPath;
            gameScene.skyboxExposure = dto.SkyboxExposure;
            gameScene.skyboxFlipV = dto.SkyboxFlipV;
            gameScene.iblSpecularIntensity = dto.IblSpecularIntensity;
            gameScene.relPath = dto.RelPath;
        }

        private static GameObjectDto ToDto(GameObject gameObject)
        {
            Guid parentGuid = Guid.Empty;
            if(gameObject.transform.parent != null)
                parentGuid = gameObject.transform.parent.GameObject.Id;

            var goDto = new GameObjectDto
            {
                Id = gameObject.Id,
                Parent = parentGuid,
                Name = gameObject.name,
                Transform = new TransformDto
                {
                    PositionX = gameObject.transform.WorldPosition.X,
                    PositionY = gameObject.transform.WorldPosition.Y,
                    PositionZ = gameObject.transform.WorldPosition.Z,

                    RotationX = gameObject.transform.WorldRotation.X,
                    RotationY = gameObject.transform.WorldRotation.Y,
                    RotationZ = gameObject.transform.WorldRotation.Z,
                    RotationW = gameObject.transform.WorldRotation.W,

                    ScaleX = gameObject.transform.WorldScale.X,
                    ScaleY = gameObject.transform.WorldScale.Y,
                    ScaleZ = gameObject.transform.WorldScale.Z,
                }
            };

            foreach (var c in gameObject.Components)
            {
                string typeKey = ComponentTypeRegistry.Get(c.GetType());

                try
                {
                    object cDto = ComponentDtoRegistry.ToDto(c, typeKey);

                    JsonElement elem = JsonSerializer.SerializeToElement(cDto, cDto.GetType(), Options);

                    goDto.Components.Add(new ComponentEntryDto
                    {
                        Type = typeKey,
                        Data = elem
                    });
                }
                catch
                {
                    // Not registered or failed to serialize; skip or log
                }
            }

            return goDto;
        }

        private static void DeserializeComponents(GameObject gameObject, List<ComponentEntryDto> components)
        {
            foreach (var entry in components)
            {
                if (!ComponentDtoRegistry.TryCreate(entry.Type, out var comp))
                {
                    continue;
                }

                var dtoType = ComponentDtoRegistry.GetDtoType(entry.Type);

                object dtoObj = entry.Data.Deserialize(dtoType, Options)
                                ?? throw new InvalidOperationException($"Invalid component data for '{entry.Type}'.");

                ComponentDtoRegistry.FromDto(comp, entry.Type, dtoObj);
                gameObject.AddComponent(comp);
            }
        }
    }

    public sealed class SceneDto
    {
        public string? RelPath { get; set; }

        public float AmbientLightIntensity { get; set; }

        public int SkyboxColorR { get; set; }
        public int SkyboxColorG { get; set; }
        public int SkyboxColorB { get; set; }

        public string? SkyboxHdrPath { get; set; }
        public float SkyboxExposure { get; set; } = 1.0f;
        public bool SkyboxFlipV { get; set; }
        public float IblSpecularIntensity { get; set; } = 0.5f;

        public List<GameObjectDto> GameObjects { get; set; } = new();
    }

    public sealed class GameObjectDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("parent")]
        public Guid Parent { get; set; }

        public string Name { get; set; } = "GameObject";
        public TransformDto Transform { get; set; } = new();
        public List<ComponentEntryDto> Components { get; set; } = new();
    }

    public sealed class TransformDto
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }

        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public float ScaleZ { get; set; } = 1f;
    }

    public sealed class ComponentEntryDto
    {
        public string Type { get; set; } = "";
        public JsonElement Data { get; set; }
    }
}
