using System.Reflection;
using Microsoft.OpenApi.Models;
using StreamApi;
using StreamApi.Scheduler;
using StreamApi.Services;
using Xabe.FFmpeg.Downloader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stream API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var filePath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(filePath);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigins",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

#region Services

builder.Services.AddScoped<IFFmpegDownloader, FFmpegDownloaderBase>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScheduler();

#endregion

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    if (!Directory.Exists(FFmpegDownloaderBase.FFmpegPath))
        Directory.CreateDirectory(FFmpegDownloaderBase.FFmpegPath);

    var files = Directory.GetFiles(FFmpegDownloaderBase.FFmpegPath, "ffmpeg.*");

    if (files.Length == 0)
    {
        var ffmpegDownloader = serviceScope.ServiceProvider.GetService<IFFmpegDownloader>();
        await ffmpegDownloader.GetLatestVersion(FFmpegDownloaderBase.FFmpegPath);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stream API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowSpecificOrigins");

app.MapControllers();

app.Run();
