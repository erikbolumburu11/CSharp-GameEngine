using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using System.Text;

namespace GameEngine.Engine
{
    public static class GltfMaterialImporter
    {
        public static IReadOnlyDictionary<SharpGLTF.Schema2.Material, Guid> ImportMaterials(ModelRoot model, string? modelPath)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var results = new Dictionary<SharpGLTF.Schema2.Material, Guid>();

            if (ProjectContext.Current == null)
                return results;

            string modelLabel = MakeSafeFileName(GetModelLabel(modelPath, model));
            string materialsDir = NormalizeRel(Path.Combine(ProjectContext.Current.Paths.AssetRootRelative, "Materials", "Imported"));
            string texturesDir = NormalizeRel(Path.Combine(ProjectContext.Current.Paths.AssetRootRelative, "Textures", "Imported"));

            foreach (var gltfMaterial in model.LogicalMaterials)
            {
                string materialName = MakeSafeFileName(GetMaterialName(gltfMaterial));
                string baseName = $"{modelLabel}_{materialName}";
                string relMaterialPath = NormalizeRel(Path.Combine(materialsDir, baseName + ".mat"));
                string absMaterialPath = ProjectContext.Current.Paths.ToAbsolute(relMaterialPath);

                if (!File.Exists(absMaterialPath))
                {
                    var material = new Material();
                    material.diffuseColorSpace = TextureColorSpace.Srgb;
                    material.specularColorSpace = TextureColorSpace.Linear;
                    material.metallicRoughnessColorSpace = TextureColorSpace.Linear;
                    material.aoColorSpace = TextureColorSpace.Linear;
                    material.diffuseTexGuid = TryImportTexture(
                        gltfMaterial.FindChannel("BaseColor"),
                        texturesDir,
                        baseName,
                        "diffuse",
                        modelPath
                    );
                    material.specularTexGuid = TryImportTexture(
                        gltfMaterial.FindChannel("SpecularGlossiness"),
                        texturesDir,
                        baseName,
                        "specular",
                        modelPath
                    );
                    material.metallicRoughnessTexGuid = TryImportTexture(
                        gltfMaterial.FindChannel("MetallicRoughness"),
                        texturesDir,
                        baseName,
                        "metalrough",
                        modelPath
                    );
                    material.aoTexGuid = TryImportTexture(
                        gltfMaterial.FindChannel("Occlusion"),
                        texturesDir,
                        baseName,
                        "ao",
                        modelPath
                    );
                    material.useCombinedMR = material.metallicRoughnessTexGuid is not null
                        && material.metallicRoughnessTexGuid.Value != Guid.Empty;
                    MaterialSerializer.SaveMaterial(material, relMaterialPath);
                }

                var meta = MetaFile.LoadOrCreate(absMaterialPath);
                results[gltfMaterial] = Guid.Parse(meta.guid);
            }

            AssetDatabase.ScanAssets(ProjectContext.Current.Paths.AssetRootAbsolute);
            return results;
        }

        private static Guid? TryImportTexture(
            MaterialChannel? channel,
            string texturesDir,
            string baseName,
            string suffix,
            string? modelPath
        )
        {
            if (channel == null)
                return null;

            var texture = channel.Value.Texture;
            if (texture == null)
                return null;

            var image = texture.PrimaryImage ?? texture.FallbackImage;
            if (image == null || image.Content.IsEmpty)
                return null;

            string? sourcePath = image.Content.SourcePath;
            if (!string.IsNullOrWhiteSpace(sourcePath)
                && !Path.IsPathRooted(sourcePath)
                && !string.IsNullOrWhiteSpace(modelPath))
            {
                string modelAbsPath = ProjectContext.Current!.Paths.ToAbsolute(modelPath);
                string? modelDir = Path.GetDirectoryName(modelAbsPath);
                if (!string.IsNullOrWhiteSpace(modelDir))
                    sourcePath = Path.GetFullPath(Path.Combine(modelDir, sourcePath));
            }

            string extension = GetImageExtension(image.Content, sourcePath);
            string relTexturePath = NormalizeRel(Path.Combine(texturesDir, baseName + "_" + suffix + extension));
            string absTexturePath = ProjectContext.Current!.Paths.ToAbsolute(relTexturePath);

            if (!File.Exists(absTexturePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(absTexturePath)!);

                if (!string.IsNullOrWhiteSpace(sourcePath) && File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, absTexturePath, true);
                }
                else
                {
                    image.Content.SaveToFile(absTexturePath);
                }
            }

            var meta = MetaFile.LoadOrCreate(absTexturePath);
            return Guid.Parse(meta.guid);
        }

        private static string GetModelLabel(string? modelPath, ModelRoot model)
        {
            if (!string.IsNullOrWhiteSpace(modelPath))
            {
                string name = Path.GetFileNameWithoutExtension(modelPath);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            if (model.DefaultScene != null && !string.IsNullOrWhiteSpace(model.DefaultScene.Name))
                return model.DefaultScene.Name;

            return "Model";
        }

        private static string GetMaterialName(SharpGLTF.Schema2.Material material)
        {
            if (!string.IsNullOrWhiteSpace(material.Name))
                return material.Name;

            return $"Material_{material.LogicalIndex}";
        }

        private static string GetImageExtension(MemoryImage image, string? sourcePath)
        {
            if (!string.IsNullOrWhiteSpace(sourcePath))
            {
                string srcExt = Path.GetExtension(sourcePath);
                if (!string.IsNullOrWhiteSpace(srcExt))
                    return NormalizeExtension(srcExt);
            }

            if (image.IsJpg)
                return ".jpg";
            if (image.IsPng)
                return ".png";
            if (image.IsWebp)
                return ".webp";
            if (image.IsKtx2)
                return ".ktx2";
            if (image.IsDds)
                return ".dds";

            return NormalizeExtension(string.IsNullOrWhiteSpace(image.FileExtension) ? ".png" : image.FileExtension);
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return ".png";

            if (extension[0] != '.')
                return "." + extension;

            return extension;
        }

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unnamed";

            var builder = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if ((c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '_'
                    || c == '-')
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('_');
                }
            }

            string result = builder.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(result) ? "Unnamed" : result;
        }

        private static string NormalizeRel(string rel)
        {
            rel = rel.Replace('\\', '/').Trim();
            while (rel.StartsWith("./", StringComparison.Ordinal)) rel = rel.Substring(2);
            while (rel.StartsWith("/", StringComparison.Ordinal)) rel = rel.Substring(1);
            return rel;
        }
    }
}
