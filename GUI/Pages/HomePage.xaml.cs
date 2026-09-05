using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;

namespace McNbtViewerGUI;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void BtnOpenNbtSelect_Click(object sender, RoutedEventArgs e)
    {
        var page = new NbtFileSelectPage();
        NbtSelectResult? result = null;
        ContentDialog? dialog = null;
        IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWin);

        page.SetCallbacks(
            (path, ver) =>
            {
                result = new NbtSelectResult { Path = path, Version = ver };
            },
            () =>
            {
                dialog?.Hide();
            },
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

        if (result != null && !result.IsCancelled)
        {
            string? json = await CliHelper.ReadNbtToJson(result.Path!, result.Version!);
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

            // 传递复合对象：json + 原路径 + edition，解决导航丢失路径问题
            var navArg = new EditPageNavParam
            {
                JsonText = json,
                NbtFilePath = result.Path!,
                Edition = result.Version!
            };
            App.MainWin?.NavigateToPage("EditPage");
            Frame.Navigate(typeof(EditPage), navArg);
        }
    }

    private async void BtnSaveNbtSelect_Click(object sender, RoutedEventArgs e)
    {
        var page = new NbtFileSelectPage();

        NbtSelectResult? result = null;
        ContentDialog? dialog = null;
        IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWin);

        dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "保存NBT文件",
            Content = page,
            MinWidth = 520,
            MinHeight = 340
        };

        page.SetCallbacks(
            (path, ver) =>
            {
                result = new NbtSelectResult { Path = path, Version = ver };
            },
            () =>
            {
                dialog?.Hide();
            },
            mainHwnd,
            NbtFileSelectMode.Save);

        await dialog.ShowAsync();

        if (result != null && !result.IsCancelled)
        {
            await new ContentDialog
            {
                Title = "已选择保存路径",
                Content = $"路径：{result.Path}",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
    }
}
