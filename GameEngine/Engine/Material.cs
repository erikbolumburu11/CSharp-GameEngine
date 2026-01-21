using System.Text.Json;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public record MaterialDto
    (
        Guid? diffuseTexGuid,
        Guid? specularTexGuid,
        TextureColorSpace? diffuseColorSpace,
        TextureColorSpace? specularColorSpace,
        float[] uvTiling,
        float[] uvOffset
    );

    public class Material
    {
        public string relPath;

        public Guid? diffuseTexGuid;
        public Guid? specularTexGuid;
        public TextureColorSpace diffuseColorSpace = TextureColorSpace.Srgb;
        public TextureColorSpace specularColorSpace = TextureColorSpace.Linear;

        public Vector2 uvTiling = new(1f, 1f);
        public Vector2 uvOffset = new(0f, 0f);

        public Texture GetDiffuse(TextureManager textureManager)
        {
            if (diffuseTexGuid is null || diffuseTexGuid.Value == Guid.Empty)
                return textureManager.White;

            if (TryResolveTexture(textureManager, diffuseTexGuid.Value, diffuseColorSpace, textureManager.White, out var tex))
                return tex;

            diffuseTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.White;
        }

        public Texture GetSpecular(TextureManager textureManager)
        {
            if (specularTexGuid is null || specularTexGuid.Value == Guid.Empty)
                return textureManager.Black;

            if (TryResolveTexture(textureManager, specularTexGuid.Value, specularColorSpace, textureManager.Black, out var tex))
                return tex;

            specularTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.Black;
        }

        private static bool TryResolveTexture(
            TextureManager textureManager,
            Guid guid,
            TextureColorSpace colorSpace,
            Texture fallback,
            out Texture texture
        )
        {
            if (AssetDatabase.IsVirtual(guid))
            {
                texture = GetBuiltInTexture(textureManager, guid, colorSpace, fallback);
                return true;
            }

            if (!AssetDatabase.TryGetPath(guid, out var path)
                || string.IsNullOrWhiteSpace(path)
                || !File.Exists(path))
            {
                texture = null!;
                return false;
            }

            try
            {
                texture = textureManager.Get(path, colorSpace);
                return true;
            }
            catch
            {
                texture = null!;
                return false;
            }
        }

        private static Texture GetBuiltInTexture(
            TextureManager textureManager,
            Guid guid,
            TextureColorSpace colorSpace,
            Texture fallback
        )
        {
            if (guid == BuiltInGuids.WhiteTexture)
                return colorSpace == TextureColorSpace.Srgb ? textureManager.WhiteSrgb : textureManager.White;
            if (guid == BuiltInGuids.GreyTexture)
                return colorSpace == TextureColorSpace.Srgb ? textureManager.GreySrgb : textureManager.Grey;
            if (guid == BuiltInGuids.BlackTexture)
                return colorSpace == TextureColorSpace.Srgb ? textureManager.BlackSrgb : textureManager.Black;
            if (guid == BuiltInGuids.FlatNormal)
                return textureManager.FlatNormal;

            return fallback;
        }

        public MaterialDto ToDto() => new
        (
            diffuseTexGuid: diffuseTexGuid,
            specularTexGuid: specularTexGuid,
            diffuseColorSpace: diffuseColorSpace,
            specularColorSpace: specularColorSpace,
            uvTiling: new[] { uvTiling.X, uvTiling.Y },
            uvOffset: new[] { uvOffset.X, uvOffset.Y }
        );

        public void FromDto(MaterialDto dto)
        {
            diffuseTexGuid = dto.diffuseTexGuid;
            specularTexGuid = dto.specularTexGuid;
            diffuseColorSpace = dto.diffuseColorSpace ?? TextureColorSpace.Srgb;
            specularColorSpace = dto.specularColorSpace ?? TextureColorSpace.Linear;

            if (dto.uvTiling is { Length: >= 2 })
                uvTiling = new Vector2(dto.uvTiling[0], dto.uvTiling[1]);

            if (dto.uvOffset is { Length: >= 2 })
                uvOffset = new Vector2(dto.uvOffset[0], dto.uvOffset[1]);
        }
    }

    public static class MaterialSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static void SaveMaterial(Material material, string relPath)
        {
            if (ProjectContext.Current == null)
            throw new InvalidOperationException("Cannot save material without an active project.");

            MaterialDto dto = material.ToDto();

            string absPath = ProjectContext.Current.Paths.ToAbsolute(relPath);

            Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);

            string json = JsonSerializer.Serialize(dto, Options);
            File.WriteAllText(absPath, json);

            material.relPath = relPath;
        }

        public static Material LoadMaterial(string relPath)
        {
            if (ProjectContext.Current == null)
                throw new InvalidOperationException("Cannot load material without an active project.");

            string absPath = ProjectContext.Current.Paths.ToAbsolute(relPath);
            string json = File.ReadAllText(absPath);

            MaterialDto dto = JsonSerializer.Deserialize<MaterialDto>(json, Options)
                            ?? throw new InvalidOperationException("Invalid material file.");

            var material = new Material();
            material.FromDto(dto);
            material.relPath = relPath;

            return material;
        }

    }
}
