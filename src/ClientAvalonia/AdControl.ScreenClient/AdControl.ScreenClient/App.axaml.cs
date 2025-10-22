using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AdControl.ScreenClient;

public partial class App : Application
{
    public static IServiceProvider? Services { get; set; }

    public override void Initialize()
    {
        Core.Initialize();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Services?.GetService<MainWindow>() ?? new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}