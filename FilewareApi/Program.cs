using FilewareApi.Services.FileManagerService;
using FilewareApi.Services.MessagingService;
using FilewareApi.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace FilewareApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IFileManagerService, FileManagerService>();
        builder.Services.AddScoped<IMessagingService, MessagingService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                bld => bld
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader());
        });
        builder.Services.AddDbContext<FilewareDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = TenGigabytes);
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

    private const long TenGigabytes = 10737418240;
}