using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API;

internal static class StartupHelperExtensions
{
    // Add services to the container
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddScoped<ICourseLibraryRepository, 
            CourseLibraryRepository>();

        builder.Services.AddDbContext<CourseLibraryContext>(options =>
        {
            options.UseSqlite(@"Data Source=library.db");
        });

        builder.Services.AddAutoMapper(
            AppDomain.CurrentDomain.GetAssemblies());

        return builder.Build();
    }

    // Configure the request/response pipelien
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                });
            });
        }
        /*
         env.IsDevelopment() checks if the current environment is set to "Development". This is typically controlled by the ASPNETCORE_ENVIRONMENT environment variable.

       If the environment is set to "Development", app.UseDeveloperExceptionPage() is called. This middleware displays detailed exception information in the browser for development purposes. It's helpful for debugging and should be used only in the development environment.

       If the environment is not "Development", the else block is executed.

       app.UseExceptionHandler() sets up a middleware to handle exceptions globally. It takes a lambda expression that configures the response when an exception occurs.

       In the lambda expression, context.Response.StatusCode is set to 500, indicating an internal server error.

       await context.Response.WriteAsync("An unexpected fault happened. Try again later") writes a simple error message to the response body.

       Other middleware and configuration specific to your application can be added after the exception handling code.
        */
        app.UseAuthorization();

        app.MapControllers(); 
         
        return app; 
    }

    public static async Task ResetDatabaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetService<CourseLibraryContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        } 
    }
}