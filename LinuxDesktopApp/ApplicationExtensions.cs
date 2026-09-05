namespace LinuxDesktopApp;

using System.Runtime.InteropServices;

using BunnyTail.DependencyInjection;

using LinuxDesktopApp.Settings;
using LinuxDesktopApp.Views;

using LinuxDotNet.GameInput;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Serilog;

using Smart.Avalonia;

public static partial class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Container
    //--------------------------------------------------------------------------------

    public static HostApplicationBuilder ConfigureContainer(this HostApplicationBuilder builder)
    {
        builder.ConfigureContainer(new GeneratedServiceProviderFactory(static options => options.TrackTransientDisposables = false));

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static HostApplicationBuilder ConfigureLogging(this HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(options =>
        {
            options.ReadFrom.Configuration(builder.Configuration);
        });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    public static HostApplicationBuilder ConfigureComponents(this HostApplicationBuilder builder)
    {
        builder.Services.AddAvaloniaServices();

        // Setting
        builder.Services.AddOptions<ControllerSetting>().BindConfiguration("Controller").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<ControllerSetting>>().Value);
        builder.Services.AddOptions<MotorSetting>().BindConfiguration("Motor").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<MotorSetting>>().Value);
        builder.Services.AddOptions<BarcodeSetting>().BindConfiguration("Barcode").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<BarcodeSetting>>().Value);
        builder.Services.AddOptions<CameraSetting>().BindConfiguration("Camera").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<CameraSetting>>().Value);
        builder.Services.AddOptions<DetectSetting>().BindConfiguration("Detect").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<DetectSetting>>().Value);
        builder.Services.AddOptions<PrinterSetting>().BindConfiguration("Printer").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<PrinterSetting>>().Value);

        // Messenger
        builder.Services.AddSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigation
        builder.Services.AddNavigator(static (_, config) =>
        {
            config.UseAvaloniaNavigationProvider();
            config.UseIdViewMapper(static m => m.AutoRegister(ViewSource()));
        });

        // Service
        builder.Services.AddServices();

        // Components
        builder.Services.AddSingleton(static _ => new GameController());

        // Window
        builder.Services.AddSingleton<MainWindow>();
        // View & ViewModel
        builder.Services.AddViews();
        builder.Services.AddViewModels();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Startup
    //--------------------------------------------------------------------------------

    public static async ValueTask StartApplicationAsync(this IHost host)
    {
        // Start host
        await host.StartAsync().ConfigureAwait(false);

        // Startup log
        var log = host.Services.GetRequiredService<ILogger<App>>();
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(App).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

        // Navigate to view
        var navigator = host.Services.GetRequiredService<INavigator>();
        await navigator.ForwardAsync(ViewId.Dashboard).ConfigureAwait(false);
    }

    public static async ValueTask ExitApplicationAsync(this IHost host)
    {
        // Stop host
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    //--------------------------------------------------------------------------------
    // Service
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Singleton, "Service$")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);

    //--------------------------------------------------------------------------------
    // View & ViewModel
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Transient, "View$")]
    public static partial IServiceCollection AddViews(this IServiceCollection services);

    [ComponentRegistration(Lifetime.Transient, "ViewModel$")]
    public static partial IServiceCollection AddViewModels(this IServiceCollection services);
}
