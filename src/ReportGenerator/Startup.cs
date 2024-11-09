using System.Text.Json;
using ReportGenerator.Core;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using ReportGenerator.ReportGeneration;
using Serilog;
using Serilog.Templates;

namespace ReportGenerator;

public class Startup
{
    /// <summary>
    /// Constructs a startup object.
    /// </summary>
    /// <param name="configuration">WebAPI configuration.</param>
    /// <param name="environment">Web hosting environment.</param>
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    /// <summary>
    /// The configuration object.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// The environment object.
    /// </summary>
    public IWebHostEnvironment Environment { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProperty("Application", "ReportGen")
            .WriteTo.Console(
                new ExpressionTemplate(
                    // Include trace and span ids when present.
                    "ReportGen - [{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"
                )
            )
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/healthz");
        });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">Service Container.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();
        services.AddHealthChecks();

        services.AddHostedService<ReportGenerationRequestedServiceBusQueueListener>();
        services.AddTransient<AzureStorageService>();
        services.AddTransient<ReportGenerationService>();

        services
            .AddControllers()
            // Add JSON options for REST endpoints.
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        ConfigureOptions(Configuration, services);

        Stimulsoft.Base.StiLicense.Key = "<<SET_STIMULSOFT_KEY_HERE>>";
    }

    private static void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
    {
        var serviceBusOptions = new ServiceBusOptions();
        var storageOptions = new StorageOptions();

        configuration.Bind(ServiceBusOptions.Section, serviceBusOptions);
        configuration.Bind(StorageOptions.Section, storageOptions);

        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.Section));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Section));
    }
}
