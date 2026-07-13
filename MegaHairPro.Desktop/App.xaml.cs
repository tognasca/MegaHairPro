using System.Windows;

namespace MegaHairPro.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        Services = AppHost.Configure();
        base.OnStartup(e);
    }
}
