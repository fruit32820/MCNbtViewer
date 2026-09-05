using McNbtViewerGUI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace McNbtViewerGUI;

public sealed partial class EditPage : Page
{
    public ObservableCollection<JsonTreeItem> TreeItems { get; } = [];

    private JsonTreeItem? _selectedNode;

    // 记录当前打开文件信息
    private string? _currentNbtFilePath;
    private string? _currentEdition;

    public EditPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is EditPageNavParam param)
        {
            LoadFromNbtJson(param.JsonText);
            _currentNbtFilePath = param.NbtFilePath;
            _currentEdition = param.Edition;
        }
    }

    /// <summary>加载NBT‑JSON文本到TreeView</summary>
    public void LoadFromNbtJson(string jsonStr)
    {
        TreeItems.Clear();
        _selectedNode = null;

        var rootNode = NbtJsonTreeParser.Parse(jsonStr);
        TreeItems.Add(rootNode);

        Debug.WriteLine($"根节点子节点计数：{rootNode.Children.Count}");
    }

    /// <summary>把编辑后的树重新导出为NBT‑JSON字符串</summary>
    public string ExportToNbtJson()
    {
        if (TreeItems.Count == 0)
            return "{}";
        return NbtJsonTreeParser.WriteBack(TreeItems[0]);
    }

    /// <summary>TreeView选中变更 → 刷新右侧属性面板</summary>
    private void TreeView_OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        // 只取第一个选中项，我们使用单选模式
        if (args.AddedItems.Count > 0 && args.AddedItems[0] is JsonTreeItem n)
        {
            _selectedNode = n;
            FillPropertyPanel(_selectedNode);
        }
        else
        {
            _selectedNode = null;
            ClearPropertyPanel();
        }
    }

    /// <summary>填充右侧编辑面板</summary>
    private void FillPropertyPanel(JsonTreeItem node)
    {
        if (string.IsNullOrEmpty(node.Name))
        {
            TxtTagKey.IsEnabled = false;
            TxtTagKey.Text = string.Empty;
            TxtTagKey.PlaceholderText = "List项无Key";
        }
        else
        {
            TxtTagKey.IsEnabled = true;
            TxtTagKey.Text = node.Name;
            TxtTagKey.PlaceholderText = "修改标签名称";
        }

        CmbTagType.Items.Clear();
        CmbTagType.Items.Add(node.Type);
        CmbTagType.SelectedIndex = 0;
        HostValueEditor.IsEnabled = false;
        if(node.RawValue is string text)
        {
            HostValueEditor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, text);
            HostValueEditor.IsEnabled = true;
        }
    }

    /// <summary>清空右侧编辑面板</summary>
    private void ClearPropertyPanel()
    {
        TxtTagKey.Text = string.Empty;
        TxtTagKey.IsEnabled = false;
        CmbTagType.Items.Clear();
        CmbTagType.IsEnabled = false;
        HostValueEditor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, null);
        HostValueEditor.IsEnabled = false;
    }

    /// <summary>页面内【打开】按钮</summary>
    private async void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        var page = new NbtFileSelectPage();
        NbtSelectResult? result = null;
        ContentDialog? dialog = null;
        IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWin);

        page.SetCallbacks(
            (path, ver) => result = new NbtSelectResult { Path = path, Version = ver },
            () => dialog?.Hide(),
            mainHwnd,
            NbtFileSelectMode.Open);

        dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "打开NBT文件",
            Content = page,
            MinWidth = 520,
            MinHeight = 340
        };
        await dialog.ShowAsync();

        if (result is { IsCancelled: false, Path: not null, Version: not null })
        {
            string? json = await CliHelper.ReadNbtToJson(result.Path, result.Version);
            if (string.IsNullOrEmpty(json))
            {
                await new ContentDialog
                {
                    Title = "读取失败",
                    Content = "CLI读取NBT文件失败",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            LoadFromNbtJson(json);
            _currentNbtFilePath = result.Path;
            _currentEdition = result.Version;
        }
    }

    /// <summary>【保存】直接覆写当前文件</summary>
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (TreeItems.Count == 0)
        {
            await new ContentDialog
            {
                Title = "提示",
                Content = "没有可保存的内容",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
            return;
        }

        if (string.IsNullOrEmpty(_currentNbtFilePath) || string.IsNullOrEmpty(_currentEdition))
        {
            BtnSaveAs_Click(sender, e);
            return;
        }

        string json = ExportToNbtJson();
        bool ok = await CliHelper.WriteJsonToNbt(_currentNbtFilePath, json, _currentEdition);

        await new ContentDialog
        {
            Title = ok ? "保存成功" : "保存失败",
            Content = ok ? "文件已更新" : "CLI写出NBT失败",
            CloseButtonText = "确定",
            XamlRoot = this.XamlRoot
        }.ShowAsync();
    }

    /// <summary>【另存为】按钮</summary>
    private async void BtnSaveAs_Click(object sender, RoutedEventArgs e)
    {
        if (TreeItems.Count == 0)
        {
            await new ContentDialog
            {
                Title = "提示",
                Content = "没有可保存的内容",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
            return;
        }

        var page = new NbtFileSelectPage();
        NbtSelectResult? result = null;
        ContentDialog? dialog = null;
        IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWin);

        page.SetCallbacks(
            (path, ver) => result = new NbtSelectResult { Path = path, Version = ver },
            () => dialog?.Hide(),
            mainHwnd,
            NbtFileSelectMode.Save);

        dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "另存为NBT文件",
            Content = page,
            MinWidth = 520,
            MinHeight = 340
        };
        await dialog.ShowAsync();

        if (result is { IsCancelled: false, Path: not null, Version: not null })
        {
            string json = ExportToNbtJson();
            bool ok = await CliHelper.WriteJsonToNbt(result.Path, json, result.Version);

            await new ContentDialog
            {
                Title = ok ? "保存成功" : "保存失败",
                Content = ok ? "文件已写出" : "CLI写出NBT失败",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            if (ok)
            {
                _currentNbtFilePath = result.Path;
                _currentEdition = result.Version;
            }
        }
    }

    private void TxtTagKey_LostFocus(object sender, RoutedEventArgs _)
    {
        if (_selectedNode == null) return;
        if (sender is TextBox tb)
        {
            if (string.IsNullOrEmpty(_selectedNode.Name))
                return;

            var newName = tb.Text?.Trim() ?? string.Empty;
            _selectedNode.Name = newName;
        }
    }

    private void BtnAddTag_Click(object sender, RoutedEventArgs _)
    {
        // TODO 新建标签逻辑
    }
}
