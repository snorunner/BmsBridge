public static class IotDeviceExtensions
{
    public static void AddIotDevice(this IServiceCollection services, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Default dev device
            // If you ever want to switch to ConsoleIotDevice or AzureIotDevice,
            // you can do it here without touching Program.cs.

            // services.AddSingleton<IIotDevice, VoidIotDevice>();
            // or:
            services.AddSingleton<IIotDevice, ConsoleIotDevice>();
            // or:
            // services.AddSingleton<IIotDevice, AzureIotDevice>();

        }
        else
        {
            services.AddSingleton<IIotDevice, AzureIotDevice>();
        }
    }
}
