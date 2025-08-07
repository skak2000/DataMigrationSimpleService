using Microsoft.EntityFrameworkCore;
using SimpleService;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession();

        builder.Services.AddDbContext<SimpleDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 104857600; // 100 MB
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseStaticFiles();

        app.UseSession();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}