using System.Text.Json;

namespace GameEngine.Engine
{
    public record MaterialDto
    (
        string diffuseTex
        string specularTex
    );

    public class Material
    {
        public string relPath;
        public string? diffuseTex;
        public string? specularTex;

        public Texture GetDiffuse(TextureManager textureManager)
        {
            return textureManager.Get(diffuseTex) ?? textureManager.White;
        }

        public Texture GetSpecular(TextureManager textureManager)
        {
            return textureManager.Get(specularTex) ?? textureManager.Grey;
        }

        public MaterialDto ToDto() => new
        (
            diffuseTex: diffuseTex,
            specularTex: specularTex
        );

        public void FromDto(MaterialDto dto)
        {
            diffuseTex = dto.diffuseTex;
            specularTex = dto.specularTex;
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