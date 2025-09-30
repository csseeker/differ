using System.Collections.ObjectModel;

namespace Differ.UI.Models
{
    public class UITreeViewItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public ObservableCollection<UITreeViewItem> Children { get; set; } = new();
    }
}
