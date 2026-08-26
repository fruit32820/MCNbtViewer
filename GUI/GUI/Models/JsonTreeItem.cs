using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GUI.Models;

public class JsonTreeItem : INotifyPropertyChanged
{
    private string _type = string.Empty;
    private string? _listType;
    private string _name = string.Empty;
    private object? _rawValue;
    private bool _isExpanded;
    private string _nodeSuffix = string.Empty;
    private string _listItemPreview = string.Empty;

    public ObservableCollection<JsonTreeItem> Children { get; } = [];

    /// <summary>$type：compound / list / byte / int / string / intarray …</summary>
    public string Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    /// <summary>list类型独有 $listType</summary>
    public string? ListType
    {
        get => _listType;
        set { _listType = value; OnPropertyChanged(); }
    }

    /// <summary>节点键名；list内部子元素为空字符串</summary>
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    /// <summary>原始值；compound/list 此字段不用，数据放Children</summary>
    public object? RawValue
    {
        get => _rawValue;
        set
        {
            _rawValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(ListItemPreview));
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    /// <summary>节点后缀，仅UI显示</summary>
    public string NodeSuffix
    {
        get => _nodeSuffix;
        set { _nodeSuffix = value; OnPropertyChanged(); }
    }

    /// <summary>List内部子节点的值预览，仅UI；List子项Name为空，显示这个</summary>
    public string ListItemPreview
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_listItemPreview))
                return _listItemPreview;

            // 回退自动从RawValue生成预览
            if (Type is "compound" or "list" or "bytearray" or "intarray" or "longarray")
                return DisplayText;

            return RawValue?.ToString() ?? string.Empty;
        }
        set { _listItemPreview = value; OnPropertyChanged(); }
    }

    /// <summary>UI显示的值文本</summary>
    public string DisplayText
    {
        get
        {
            if (Type == "compound") return "{compound}";
            if (Type == "list") return $"[list<{ListType}>]";
            if (Type is "bytearray" or "intarray" or "longarray")
            {
                if (RawValue is object[] arr)
                    return $"[{Type}] length={arr.Length}";
                return $"[{Type}]";
            }
            return RawValue?.ToString() ?? "";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
