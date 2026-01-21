using System.Text.Json;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public record MaterialDto
    (
        Guid? diffuseTexGuid,
        Guid? specularTexGuid,
        Guid? metallicRoughnessTexGuid,
        Guid? metallicTexGuid,
        Guid? roughnessTexGuid,
        Guid? aoTexGuid,
        bool? useCombinedMR,
        TextureColorSpace? diffuseColorSpace,
        TextureColorSpace? specularColorSpace,
        TextureColorSpace? metallicRoughnessColorSpace,
        TextureColorSpace? metallicColorSpace,
        TextureColorSpace? roughnessColorSpace,
        TextureColorSpace? aoColorSpace,
        float[] uvTiling,
        float[] uvOffset
    );

    public class Material
    {
        public string relPath;

        public Guid? diffuseTexGuid;
        public Guid? specularTexGuid;
        public Guid? metallicRoughnessTexGuid;
        public Guid? metallicTexGuid;
        public Guid? roughnessTexGuid;
        public Guid? aoTexGuid;
        public bool useCombinedMR;
        public TextureColorSpace diffuseColorSpace = TextureColorSpace.Srgb;
        public TextureColorSpace specularColorSpace = TextureColorSpace.Linear;
        public TextureColorSpace metallicRoughnessColorSpace = TextureColorSpace.Linear;
        public TextureColorSpace metallicColorSpace = TextureColorSpace.Linear;
        public TextureColorSpace roughnessColorSpace = TextureColorSpace.Linear;
        public TextureColorSpace aoColorSpace = TextureColorSpace.Linear;

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

        public Texture GetMetallicRoughness(TextureManager textureManager)
        {
            if (metallicRoughnessTexGuid is null || metallicRoughnessTexGuid.Value == Guid.Empty)
                return textureManager.MetallicRoughnessDefault;

            if (TryResolveTexture(
                textureManager,
                metallicRoughnessTexGuid.Value,
                metallicRoughnessColorSpace,
                textureManager.MetallicRoughnessDefault,
                out var tex))
            {
                return tex;
            }

            metallicRoughnessTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.MetallicRoughnessDefault;
        }

        public Texture GetAmbientOcclusion(TextureManager textureManager)
        {
            if (aoTexGuid is null || aoTexGuid.Value == Guid.Empty)
                return textureManager.AmbientOcclusionDefault;

            if (TryResolveTexture(
                textureManager,
                aoTexGuid.Value,
                aoColorSpace,
                textureManager.AmbientOcclusionDefault,
                out var tex))
            {
                return tex;
            }

            aoTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.AmbientOcclusionDefault;
        }

        public Texture GetMetallic(TextureManager textureManager)
        {
            if (metallicTexGuid is null || metallicTexGuid.Value == Guid.Empty)
                return textureManager.MetallicDefault;

            if (TryResolveTexture(
                textureManager,
                metallicTexGuid.Value,
                metallicColorSpace,
                textureManager.MetallicDefault,
                out var tex))
            {
                return tex;
            }

            metallicTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.MetallicDefault;
        }

        public Texture GetRoughness(TextureManager textureManager)
        {
            if (roughnessTexGuid is null || roughnessTexGuid.Value == Guid.Empty)
                return textureManager.RoughnessDefault;

            if (TryResolveTexture(
                textureManager,
                roughnessTexGuid.Value,
                roughnessColorSpace,
                textureManager.RoughnessDefault,
                out var tex))
            {
                return tex;
            }

            roughnessTexGuid = null;

            if (!string.IsNullOrWhiteSpace(relPath))
                MaterialSerializer.SaveMaterial(this, relPath);

            return textureManager.RoughnessDefault;
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
            metallicRoughnessTexGuid: metallicRoughnessTexGuid,
            metallicTexGuid: metallicTexGuid,
            roughnessTexGuid: roughnessTexGuid,
            aoTexGuid: aoTexGuid,
            useCombinedMR: useCombinedMR,
            diffuseColorSpace: diffuseColorSpace,
            specularColorSpace: specularColorSpace,
            metallicRoughnessColorSpace: metallicRoughnessColorSpace,
            metallicColorSpace: metallicColorSpace,
            roughnessColorSpace: roughnessColorSpace,
            aoColorSpace: aoColorSpace,
            uvTiling: new[] { uvTiling.X, uvTiling.Y },
            uvOffset: new[] { uvOffset.X, uvOffset.Y }
        );

        public void FromDto(MaterialDto dto)
        {
            diffuseTexGuid = dto.diffuseTexGuid;
            specularTexGuid = dto.specularTexGuid;
            metallicRoughnessTexGuid = dto.metallicRoughnessTexGuid;
            metallicTexGuid = dto.metallicTexGuid;
            roughnessTexGuid = dto.roughnessTexGuid;
            aoTexGuid = dto.aoTexGuid;
            useCombinedMR = dto.useCombinedMR
                ?? (metallicRoughnessTexGuid is not null && metallicRoughnessTexGuid.Value != Guid.Empty);
            diffuseColorSpace = dto.diffuseColorSpace ?? TextureColorSpace.Srgb;
            specularColorSpace = dto.specularColorSpace ?? TextureColorSpace.Linear;
            metallicRoughnessColorSpace = dto.metallicRoughnessColorSpace ?? TextureColorSpace.Linear;
            metallicColorSpace = dto.metallicColorSpace ?? TextureColorSpace.Linear;
            roughnessColorSpace = dto.roughnessColorSpace ?? TextureColorSpace.Linear;
            aoColorSpace = dto.aoColorSpace ?? TextureColorSpace.Linear;

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
