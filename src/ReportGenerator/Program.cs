using Serilog;
using Serilog.Templates;

namespace ReportGenerator;

public class Program
{
    public static int Main(string[] args)
    {
        // The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
        // logger configured in `UseSerilog()` below, once configuration and dependency-injection have both been
        // set up successfully.
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateBootstrapLogger();

        Log.Information("Starting up!");

        try
        {
            CreateHostBuilder(args).Build().Run();

            Log.Information("Stopped cleanly");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Host builder.
    /// </summary>
    /// <param name="args">arguments.</param>
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog(
                (context, services, configuration) =>
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .MinimumLevel.Debug()
                        .WriteTo.Console(
                            new ExpressionTemplate(
                                // Include trace and span ids when present.
                                "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"
                            )
                        )
            )
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
