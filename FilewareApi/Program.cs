using System.Text.Json;
using FilewareApi.Models;
using FilewareApi.Services.FileManagerService;

namespace FilewareApi;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddTransient<IFileManagerService, FileManagerService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader());
        });
        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseCors("CorsPolicy");

        app.MapControllers();

        app.Run();
    }
}