using System.Text.Json;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public record MaterialDto
    (
        string diffuseTex,
        string specularTex,
        float[] uvTiling,
        float[] uvOffset
    );

    public class Material
    {
        public string relPath;

        public string? diffuseTex;
        public string? specularTex;

        public Vector2 uvTiling = new(1f, 1f);
        public Vector2 uvOffset = new(0f, 0f);

        private static bool TryGetBuiltInTexture(TextureManager textureManager, string? textureId, out Texture texture)
        {
            if (string.IsNullOrWhiteSpace(textureId))
            {
                texture = textureManager.White;
                return false;
            }

            string normalized = textureId.Trim();
            if (string.Equals(normalized, "White", StringComparison.OrdinalIgnoreCase))
            {
                texture = textureManager.White;
                return true;
            }

            if (string.Equals(normalized, "Gray", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Grey", StringComparison.OrdinalIgnoreCase))
            {
                texture = textureManager.Grey;
                return true;
            }

            if (string.Equals(normalized, "Black", StringComparison.OrdinalIgnoreCase))
            {
                texture = textureManager.Black;
                return true;
            }

            texture = textureManager.White;
            return false;
        }

        public Texture GetDiffuse(TextureManager textureManager)
        {
            if (TryGetBuiltInTexture(textureManager, diffuseTex, out var builtIn))
                return builtIn;

            if (string.IsNullOrWhiteSpace(diffuseTex) || ProjectContext.Current == null)
                return textureManager.White;

            string absPath = ProjectContext.Current.Paths.ToAbsolute(diffuseTex);
            if (!File.Exists(absPath))
            {
                diffuseTex = null;
                if (!string.IsNullOrWhiteSpace(relPath))
                    MaterialSerializer.SaveMaterial(this, relPath);
                return textureManager.White;
            }

            return textureManager.Get(diffuseTex) ?? textureManager.White;
        }

        public Texture GetSpecular(TextureManager textureManager)
        {
            if (TryGetBuiltInTexture(textureManager, specularTex, out var builtIn))
                return builtIn;

            if (string.IsNullOrWhiteSpace(specularTex) || ProjectContext.Current == null)
                return textureManager.Grey;

            string absPath = ProjectContext.Current.Paths.ToAbsolute(specularTex);
            if (!File.Exists(absPath))
            {
                specularTex = null;
                if (!string.IsNullOrWhiteSpace(relPath))
                    MaterialSerializer.SaveMaterial(this, relPath);
                return textureManager.Grey;
            }

            return textureManager.Get(specularTex) ?? textureManager.Grey;
        }

        public MaterialDto ToDto() => new
        (
            diffuseTex: diffuseTex,
            specularTex: specularTex,
            uvTiling: new[] { uvTiling.X, uvTiling.Y },
            uvOffset: new[] { uvOffset.X, uvOffset.Y }
        );

        public void FromDto(MaterialDto dto)
        {
            diffuseTex = dto.diffuseTex;
            specularTex = dto.specularTex;

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
