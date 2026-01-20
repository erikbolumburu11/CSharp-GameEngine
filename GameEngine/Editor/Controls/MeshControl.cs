using GameEngine.Engine;
using SharpGLTF.Schema2;

namespace GameEngine.Editor
{
    public class MeshControl : FieldControlBase<MeshReference>
    {
        private const string DefaultLabel = "(Default)";
        private static readonly HashSet<string> MeshExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".gltf",
            ".glb"
        };

        private readonly ComboBox combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };

        private Guid selectedGuid = Guid.Empty;

        public MeshControl()
        {
            Controls.Add(combo);
            combo.SelectedIndexChanged += (s, e) =>
            {
                selectedGuid = GetSelectedGuid();
                NotifyValueChanged();
            };

            RefreshOptions();
            SetSelectedGuid(selectedGuid);
        }

        protected override void SetControlValue(MeshReference value)
        {
            selectedGuid = value.Guid;
            RefreshOptions();
            SetSelectedGuid(selectedGuid);
        }

        protected override MeshReference GetControlValue()
        {
            return new MeshReference(selectedGuid);
        }

        private void RefreshOptions()
        {
            combo.Items.Clear();
            combo.Items.Add(new MeshOption(Guid.Empty, DefaultLabel));

            if (ProjectContext.Current != null)
            {
                string assetRoot = ProjectContext.Current.Paths.AssetRootAbsolute;
                if (Directory.Exists(assetRoot))
                    AssetDatabase.ScanAssets(assetRoot);

                if (Directory.Exists(assetRoot))
                {
                    var added = new HashSet<Guid>();
                    var items = Directory.EnumerateFiles(assetRoot, "*.*", SearchOption.AllDirectories)
                        .Where(path => MeshExtensions.Contains(Path.GetExtension(path)))
                        .Select(path =>
                        {
                            try
                            {
                                return ProjectContext.Current.Paths.ToProjectRelative(path);
                            }
                            catch
                            {
                                return null;
                            }
                        })
                        .Where(path => !string.IsNullOrWhiteSpace(path))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

                    foreach (var path in items)
                    {
                        if (!TryGetGuidFromPath(path!, out var guid))
                            continue;
                        if (!AssetDatabase.TryLoad(guid, out ModelRoot model))
                            continue;

                        AssetDatabase.RegisterModelPrimitives(guid, model, path);

                        foreach (var mesh in model.LogicalMeshes)
                        {
                            foreach (var primitive in mesh.Primitives)
                            {
                                Guid primitiveGuid = AssetDatabase.GetMeshPrimitiveGuid(
                                    guid,
                                    mesh.LogicalIndex,
                                    primitive.LogicalIndex
                                );

                                if (!added.Add(primitiveGuid))
                                    continue;

                                string label = TryGetPathFromGuid(primitiveGuid, out var primLabel)
                                    ? primLabel
                                    : $"{Path.GetFileNameWithoutExtension(path)}:Prim[{primitive.LogicalIndex}]";
                                combo.Items.Add(new MeshOption(primitiveGuid, label));
                            }
                        }
                    }
                }
            }
        }

        private void SetSelectedGuid(Guid guid)
        {
            foreach (var item in combo.Items)
            {
                if (item is MeshOption option && option.Guid == guid)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            if (guid == Guid.Empty)
            {
                combo.SelectedIndex = 0;
                return;
            }

            string label = TryGetPathFromGuid(guid, out var path)
                ? NormalizeMeshPathForDisplay(path)
                : guid.ToString();
            var fallback = new MeshOption(guid, label);
            combo.Items.Add(fallback);
            combo.SelectedItem = fallback;
        }

        private Guid GetSelectedGuid()
        {
            if (combo.SelectedItem is MeshOption option)
                return option.Guid;

            return Guid.Empty;
        }

        private static string NormalizeMeshPathForDisplay(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            string normalized = path.Replace('\\', '/').Trim();
            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return normalized;

            if (ProjectContext.Current == null)
                return normalized;

            try
            {
                return ProjectContext.Current.Paths.ToProjectRelative(path);
            }
            catch
            {
                return normalized;
            }
        }

        private static bool TryGetGuidFromPath(string path, out Guid guid)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                guid = default;
                return false;
            }

            if (TryGetGuidByLookup(path, out guid))
                return true;

            string altPath = path.Replace('/', '\\');
            if (!string.Equals(altPath, path, StringComparison.Ordinal) && TryGetGuidByLookup(altPath, out guid))
                return true;

            if (!Path.IsPathRooted(path) && ProjectContext.Current != null)
            {
                string absPath = ProjectContext.Current.Paths.ToAbsolute(path);
                if (TryGetGuidByLookup(absPath, out guid))
                    return true;

                string altAbsPath = absPath.Replace('/', '\\');
                if (!string.Equals(altAbsPath, absPath, StringComparison.Ordinal) && TryGetGuidByLookup(altAbsPath, out guid))
                    return true;
            }

            guid = default;
            return false;
        }

        private static bool TryGetGuidByLookup(string path, out Guid guid)
        {
            try
            {
                guid = AssetDatabase.PathToGuid(path);
                return true;
            }
            catch
            {
                guid = default;
                return false;
            }
        }

        private static bool TryGetPathFromGuid(Guid guid, out string path)
        {
            try
            {
                path = AssetDatabase.GuidToPath(guid);
                return true;
            }
            catch
            {
                path = string.Empty;
                return false;
            }
        }

        private sealed class MeshOption
        {
            public Guid Guid { get; }
            public string Label { get; }

            public MeshOption(Guid guid, string label)
            {
                Guid = guid;
                Label = label;
            }

            public override string ToString() => Label;
        }
    }
}
