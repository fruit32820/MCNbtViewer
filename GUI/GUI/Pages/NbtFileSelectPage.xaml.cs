using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using Windows.Storage.Pickers;
using WinRT.Interop;
using static GUI.Converters.OtherConverters;

namespace GUI;

public sealed partial class NbtFileSelectPage : Page
{
    private Action<string?, string?>? _completeCallback;
    private Action? _closeDialogCallback;
    private IntPtr _hostHwnd;
    public NbtFileSelectMode _mode;

    public NbtFileSelectPage()
    {
        InitializeComponent();
    }

    // 调用方用来注入回调
    public void SetCallbacks(Action<string?, string?> dataCallback, Action closeDlg, IntPtr hostHwnd, NbtFileSelectMode mode)
    {
        _completeCallback = dataCallback;
        _closeDialogCallback = closeDlg;
        _hostHwnd = hostHwnd;
        _mode = mode;

        if (_mode == NbtFileSelectMode.Open)
        {
            VersionPanel.Visibility = Visibility.Visible;
            VersionComboBox.SelectedIndex = 0;
        }
        else
        {
            VersionPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        string? ver = null;
        if (_mode == NbtFileSelectMode.Open && VersionComboBox.SelectedItem is ComboBoxItem item)
        {
            ver = item.Content?.ToString();
        }

        _completeCallback?.Invoke(PathTextBox.Text, VersionConvert(ver));
        _closeDialogCallback?.Invoke();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        _completeCallback?.Invoke(null, null);
        _closeDialogCallback?.Invoke();
    }

    private async void BtnChoosePath_Click(object sender, RoutedEventArgs e)
    {
        if (_hostHwnd == IntPtr.Zero) return;

        if (_mode == NbtFileSelectMode.Open)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, _hostHwnd);
            var f = await picker.PickSingleFileAsync();
            if (f != null) PathTextBox.Text = f.Path;
        }
        else
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("NBT 文件", [".nbt", ".dat"]);
            InitializeWithWindow.Initialize(picker, _hostHwnd);
            var f = await picker.PickSaveFileAsync();
            if (f != null) PathTextBox.Text = f.Path;
        }
    }
}
