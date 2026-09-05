using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage.Pickers;

namespace McNbtViewerGUI;

public sealed partial class SettingPage : Page
{
    private AppConfig? _config;
    private bool _needRestartAfterLanguageChange;
    private bool _isLoadingUi;
    private bool _pageUnloaded;
    private readonly List<string> _availableLanguages = ["en‑US", "zh‑CN"];
    private readonly List<string> _editionOptions = ["java", "bedrock"];

    public SettingPage1()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        _pageUnloaded = true;
        Loaded -= Page_Loaded;
        Unloaded -= Page_Unloaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (_pageUnloaded) return;

        var mainWin = (MainWindow?)App.MainWin;
        if (mainWin == null) return;
        _config = mainWin.Config;

        _isLoadingUi = true;
        LoadSettingsToUi();
        CbLanguage.ItemsSource = _availableLanguages;
        CbDefaultEdition.ItemsSource = _editionOptions;
        _isLoadingUi = false;
    }

    private void LoadSettingsToUi()
    {
        if (_config == null) return;

        CbLanguage.SelectedItem = _config.SelectedLanguage;
        TbCliPath.Text = _config.CliExecutablePath;
        CbDefaultEdition.SelectedItem = _config.DefaultNbtEdition;

        _needRestartAfterLanguageChange = false;
        InfoBarLangRestart.IsOpen = false;
    }

    private void SaveUiToConfig()
    {
        if (_config == null) return;

        _config.SelectedLanguage = CbLanguage.SelectedItem as string ?? "en‑US";
        _config.CliExecutablePath = TbCliPath.Text;
        _config.DefaultNbtEdition = CbDefaultEdition.SelectedItem as string ?? "java";

        _config.SaveToFile();
    }

    private void CbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoadingUi || _pageUnloaded || _config == null) return;
        if (!_needRestartAfterLanguageChange)
        {
            _needRestartAfterLanguageChange = true;
            InfoBarLangRestart.IsOpen = true;
        }
        SaveUiToConfig();
    }

    private void CbDefaultEdition_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoadingUi || _pageUnloaded || _config == null) return;
        SaveUiToConfig();
    }

    private async void BtnBrowseCli_Click(object sender, RoutedEventArgs e)
    {
        var mainWin = (MainWindow?)App.MainWin;
        if (mainWin is null || _pageUnloaded || _config == null) return;

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".exe");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWin);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file == null || _pageUnloaded) return;

        _config.CliExecutablePath = file.Path;
        TbCliPath.Text = file.Path;
        _config.SaveToFile();
    }

    private void BtnApplyCliPath_Click(object sender, RoutedEventArgs e)
    {
        if (_pageUnloaded) return;
        SaveUiToConfig();
    }

    private async void BtnResetSettings_Click(object sender, RoutedEventArgs e)
    {
        if (_pageUnloaded || _config == null) return;
        var dlg = new ContentDialog
        {
            Title = "重置设置",
            Content = "确定重置全部配置？",
            PrimaryButtonText = "重置",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };
        var res = await dlg.ShowAsync();
        if (res == ContentDialogResult.Primary && !_pageUnloaded)
        {
            _config = new AppConfig();
            _config.SaveToFile();
            _isLoadingUi = true;
            LoadSettingsToUi();
            _isLoadingUi = false;
        }
    }

    private void BtnRestartApp_Click(object sender, RoutedEventArgs e)
    {
        if (_pageUnloaded) return;
        // 获取当前exe完整路径
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
            return;

        // 启动新实例
        Process.Start(new ProcessStartInfo(exePath));

        // 退出当前程序
        Application.Current.Exit();
    }
}
