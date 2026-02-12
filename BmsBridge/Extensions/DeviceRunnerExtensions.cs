public static class DeviceRunnerExtensions
{
    public static void AddDeviceRunnerFactory(
        this IServiceCollection services,
        string[] args,
        IHostEnvironment env)
    {
        var replayRequested = args.Contains("--replay", StringComparer.OrdinalIgnoreCase);

        if (replayRequested && env.IsDevelopment())
        {
            services.AddSingleton<IDeviceRunnerFactory, ReplayDeviceRunnerFactory>();
        }
        else
        {
            services.AddSingleton<IDeviceRunnerFactory, DeviceRunnerFactory>();
        }
    }
}
