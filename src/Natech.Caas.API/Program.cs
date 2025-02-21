using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Natech.Caas.API;
using Natech.Caas.API.Extensions;
using Natech.Caas.API.Validators;
using Natech.Caas.Core.Services;
using Natech.Caas.Database;
using Natech.Caas.Database.Repository;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        builder.Services.AddCatApi(builder.Configuration);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        Console.WriteLine($"🔌 Connection String: {connectionString}");

        var isIntegrationTesting = builder.Environment.IsEnvironment(Consts.INTEGRATION_TESTING);
        if (isIntegrationTesting)
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        builder.Services.AddScoped<ICatRepository, CatRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<ICatService, CatService>();

        builder.Services.AddTransient<IDownloader, ImageDownloadService>(sp => new ImageDownloadService(Consts.DOWNLOADS_FOLDER));

        builder.Services.AddValidators();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), Consts.DOWNLOADS_FOLDER)),
            RequestPath = $"/{Consts.DOWNLOADS_FOLDER}",
        });

        app.UseRouting();
        app.MapControllers();

        app.UseHttpsRedirection();

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);

        if (!isIntegrationTesting)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }
        }

        app.Run();
    }
}

public partial class Program { }