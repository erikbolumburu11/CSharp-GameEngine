using GameEngine.Engine;

namespace GameEngine.Editor
{
    public class MaterialControl : FieldControlBase<MaterialReference>
    {
        private const string DefaultLabel = "(Default)";
        private readonly ComboBox combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };

        private string selectedPath = string.Empty;

        public MaterialControl()
        {
            Controls.Add(combo);
            combo.SelectedIndexChanged += (s, e) =>
            {
                selectedPath = GetSelectedPath();
                NotifyValueChanged();
            };

            RefreshOptions();
        }

        protected override void SetControlValue(MaterialReference value)
        {
            selectedPath = value.Path ?? string.Empty;
            RefreshOptions();
            SetSelectedPath(selectedPath);
        }

        protected override MaterialReference GetControlValue()
        {
            return new MaterialReference(selectedPath);
        }

        private void RefreshOptions()
        {
            combo.Items.Clear();
            combo.Items.Add(DefaultLabel);

            if (ProjectContext.Current != null)
            {
                var items = Directory.EnumerateFiles(ProjectContext.Current.RootPath, "*.mat", SearchOption.AllDirectories)
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
                    combo.Items.Add(path);
            }
        }

        private void SetSelectedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                combo.SelectedItem = DefaultLabel;
                return;
            }

            int index = combo.Items.IndexOf(path);
            if (index >= 0)
                combo.SelectedIndex = index;
            else
            {
                combo.Items.Add(path);
                combo.SelectedItem = path;
            }
        }

        private string GetSelectedPath()
        {
            if (combo.SelectedItem is string selected)
            {
                if (string.Equals(selected, DefaultLabel, StringComparison.Ordinal))
                    return string.Empty;
                return selected;
            }

            return string.Empty;
        }
    }
}
