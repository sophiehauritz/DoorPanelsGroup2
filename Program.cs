using System;
using Avalonia;

namespace DoorPanels;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called.
    [STAThread]
    public static void Main(string[] args) // main entry point for the desktop app
    {
        // Seed our project accounts (Laerke, Sophie, Sofie, Ida)
        // This is safe to call every startup because the seeder checks if a username already exists.
        DatabaseSeeder.AddProjectAccountsAsync().GetAwaiter().GetResult();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>() // configure app class (root of UI)
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}