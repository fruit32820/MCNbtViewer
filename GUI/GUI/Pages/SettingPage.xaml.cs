using GUI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Storage.Pickers;

namespace GUI;

public sealed partial class SettingPage : Page
{
    // 公开属性，给x:Bind使用
    public SettingViewModel ViewModel { get; }

    public SettingPage()
    {
        InitializeComponent();
        ViewModel = new SettingViewModel();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private async void BtnBrowseCli_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".exe");
        picker.FileTypeFilter.Add("*");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWin!);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.CliExecutablePath = file.Path;
        }
    }

    private async void BtnResetSettings_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ContentDialog
        {
            Title = "重置设置",
            Content = "确定要重置全部设置吗？",
            PrimaryButtonText = "重置",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };
        var res = await dlg.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            ViewModel.ResetAll();
        }
    }
}
