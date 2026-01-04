using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Engine
{
    public sealed class Project
    {
        public const string ProjectExtension = ".proj";

        public string Name { get; }
        public string ProjectFilePath { get; }
        public string RootPath { get; }               // Folder containing the .geproj
        public string AssetRootRelative { get; }      // Usually "Assets"
        public string? StartSceneRelative { get; private set; } // "Assets/Scenes/Main.scene" etc.

        [JsonIgnore]
        public ProjectPaths Paths { get; }

        private Project(string name, string projectFilePath, string rootPath, string assetRootRelative, string? startSceneRelative)
        {
            Name = name;
            ProjectFilePath = projectFilePath;
            RootPath = rootPath;
            AssetRootRelative = assetRootRelative;
            StartSceneRelative = startSceneRelative;
            Paths = new ProjectPaths(rootPath, assetRootRelative);
        }

        public static Project CreateNew(string targetDirectory, string projectName)
        {
            if (string.IsNullOrWhiteSpace(targetDirectory))
                throw new ArgumentException("Target directory is empty.", nameof(targetDirectory));
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name is empty.", nameof(projectName));

            string root = Path.Combine(targetDirectory, projectName);
            Directory.CreateDirectory(root);

            Directory.CreateDirectory(Path.Combine(root, "Assets"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Scenes"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Materials"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Textures"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Shaders"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Models"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Scripts"));

            Directory.CreateDirectory(Path.Combine(root, "Library"));

            string projectFilePath = Path.Combine(root, projectName + ProjectExtension);

            string startScenePath = "Assets/Scenes/Main.scene";

            var dto = new ProjectFileDto
            {
                name = projectName,
                assetRoot = "Assets",
                startScene = startScenePath
            };

            WriteProjectFile(projectFilePath, dto);

            SceneSerializer.SaveScene
            (
                new GameObjectManager(),
                new Scene(startScenePath),
                Path.Combine(root, startScenePath),
                false
            );

            return new Project(projectName, projectFilePath, root, dto.assetRoot!, dto.startScene);
        }

        public static Project Open(Game game, string projectFilePath)
        {
            if (string.IsNullOrWhiteSpace(projectFilePath))
                throw new ArgumentException("Project file path is empty.", nameof(projectFilePath));
            if (!File.Exists(projectFilePath))
                throw new FileNotFoundException("Project file not found.", projectFilePath);
            if (!projectFilePath.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Not a {ProjectExtension} file: {projectFilePath}");

            var dto = ReadProjectFile(projectFilePath);

            string root = Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) 
                          ?? throw new InvalidOperationException("Could not determine project root folder.");

            string name = string.IsNullOrWhiteSpace(dto.name)
                ? Path.GetFileNameWithoutExtension(projectFilePath)
                : dto.name;

            string assetRoot = string.IsNullOrWhiteSpace(dto.assetRoot) ? "Assets" : dto.assetRoot;

            SceneSerializer.LoadScene
            (
                game.gameObjectManager,
                game.scene,
                Path.Combine(root, dto.startScene)
            );

            return new Project(name, Path.GetFullPath(projectFilePath), root, assetRoot, dto.startScene);
        }

        public void SetStartScene(string? projectRelativeScenePath)
        {
            StartSceneRelative = projectRelativeScenePath;

            var dto = new ProjectFileDto
            {
                name = Name,
                assetRoot = AssetRootRelative,
                startScene = StartSceneRelative
            };

            WriteProjectFile(ProjectFilePath, dto);
        }

        // ---- JSON ----

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static ProjectFileDto ReadProjectFile(string projectFilePath)
        {
            string json = File.ReadAllText(projectFilePath);
            var dto = JsonSerializer.Deserialize<ProjectFileDto>(json, JsonOptions);
            return dto ?? new ProjectFileDto();
        }

        private static void WriteProjectFile(string projectFilePath, ProjectFileDto dto)
        {
            string json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(projectFilePath, json);
        }

        private sealed class ProjectFileDto
        {
            public string? name { get; set; }
            public string? assetRoot { get; set; }   // usually "Assets"
            public string? startScene { get; set; }  // "Assets/Scenes/Main.scene"
        }
    }

    public sealed class ProjectPaths
    {
        public string RootPath { get; }
        public string AssetRootRelative { get; }
        public string AssetRootAbsolute { get; }

        public ProjectPaths(string rootPath, string assetRootRelative)
        {
            RootPath = Path.GetFullPath(rootPath);
            AssetRootRelative = NormalizeRel(assetRootRelative);
            AssetRootAbsolute = Path.Combine(RootPath, AssetRootRelative);
        }

        public string ToAbsolute(string projectRelativePath)
        {
            projectRelativePath = NormalizeRel(projectRelativePath);
            return Path.GetFullPath(Path.Combine(RootPath, projectRelativePath));
        }

        public string ToProjectRelative(string absolutePath)
        {
            absolutePath = Path.GetFullPath(absolutePath);

            // If it's inside the project root, return relative to root
            if (absolutePath.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
            {
                string rel = Path.GetRelativePath(RootPath, absolutePath);
                return NormalizeRel(rel);
            }

            // Otherwise we can't safely store it as a project-relative reference
            throw new InvalidOperationException($"Path is not inside the project folder: {absolutePath}");
        }

        public string ToAssetRelative(string absolutePath)
        {
            absolutePath = Path.GetFullPath(absolutePath);

            if (absolutePath.StartsWith(AssetRootAbsolute, StringComparison.OrdinalIgnoreCase))
            {
                string rel = Path.GetRelativePath(RootPath, absolutePath);
                return NormalizeRel(rel);
            }

            throw new InvalidOperationException($"Path is not inside Assets/: {absolutePath}");
        }

        private static string NormalizeRel(string rel)
        {
            rel = rel.Replace('\\', '/').Trim();
            // avoid "./Assets/..." and leading slashes
            while (rel.StartsWith("./", StringComparison.Ordinal)) rel = rel.Substring(2);
            while (rel.StartsWith("/", StringComparison.Ordinal)) rel = rel.Substring(1);
            return rel;
        }
    }

    public static class ProjectContext
    {
        public static Project? Current { get; private set; }

        public static bool HasProject => Current != null;

        public static void Set(Project project)
        {
            Current = project ?? throw new ArgumentNullException(nameof(project));
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
