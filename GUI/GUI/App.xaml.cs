using Microsoft.UI.Xaml;

namespace GUI;

public partial class App : Application
{
    public static MainWindow? MainWin { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWin = new MainWindow();
        MainWin.Activate();
    }
}
