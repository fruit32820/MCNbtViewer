using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;

using System.IO;

namespace GUI;

public sealed partial class MainWindow : Window
{
    private bool _isClosing;

    public MainWindow()
    {
        InitializeComponent();

        ExtendTitleBarIntoClientArea();

        AppWindow.Closing += AppWindow_Closing;
        this.SizeChanged += Window_SizeChanged;
        this.Activated += Window_Activated;

        MainNav.Loaded += MainNav_Loaded;
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs e)
    {
        this.Activated -= Window_Activated;
        UpdateDragRect();
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        if (_isClosing) return;
        UpdateDragRect();
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        _isClosing = true;
        AppWindow.Closing -= AppWindow_Closing;
        this.SizeChanged -= Window_SizeChanged;
    }

    private void ExtendTitleBarIntoClientArea()
    {
        var tb = AppWindow.TitleBar;
        tb.ExtendsContentIntoTitleBar = true;

        tb.ButtonBackgroundColor = Colors.Transparent;
        tb.ButtonForegroundColor = Colors.White;
        tb.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(0x22, 0xff, 0xff, 0xff);
        tb.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(0x33, 0xff, 0xff, 0xff);
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(IntPtr hwnd);

    private void UpdateDragRect()
    {
        if (_isClosing || !AppWindow.IsVisible) return;

        IntPtr hwnd = WindowNative.GetWindowHandle(this);
        uint dpi = GetDpiForWindow(hwnd);
        float scale = dpi / 96f;

        int physWidth = (int)AppWindow.Size.Width;
        int physDragHeight = (int)(40 * scale);

        if (physWidth <= 0) return;

        try
        {
            AppWindow.TitleBar.SetDragRectangles([
                new RectInt32(0, 0, physWidth, physDragHeight)
            ]);
        }
        catch
        {
        }
    }

    private void MainNav_Loaded(object sender, RoutedEventArgs e)
    {
        MainNav.SelectedItem = NavHome;
        MainFrame.Navigate(typeof(HomePage));
    }

    private void MainNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag != null)
        {
            var tag = item.Tag.ToString();
            //NavigateToPage(tag);
            switch (tag)
            {
                case "HomePage": MainFrame.Navigate(typeof(HomePage)); break;
                case "EditPage": MainFrame.Navigate(typeof(EditPage)); break;
                case "SettingPage": MainFrame.Navigate(typeof(SettingPage)); break;
            }
        }
    }

    public void NavigateToPage(string? tag = "HomePage")
    {
        NavigationViewItem item = NavHome;
        switch (tag)
        {
            case "HomePage": item = NavHome; break;
            case "EditPage": item = NavEdit; break;
            case "SettingPage": item = NavSetting; break;
        }
        MainNav.SelectedItem = item;
        
    }
}
