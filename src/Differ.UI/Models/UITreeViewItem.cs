using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Differ.UI.Models
{
    public partial class UITreeViewItem : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded;

        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public ObservableCollection<UITreeViewItem> Children { get; set; } = new();
        public UITreeViewItem? Parent { get; set; }
    }
}
