using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobErp.Api.Tests.Errors.TestSupport;

public static class TestApi
{
    public static WebApplicationFactory<Program> Create()
    {
        var baseFactory = new WebApplicationFactory<Program>();
        return baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors:AllowedOrigins", "https://app.example.com");
            builder.UseSetting("ConnectionStrings:Erp", "Server=localhost;Database=DanmaobErpTests;TrustServerCertificate=True");
            builder.ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(ErrorsTestController).Assembly));
        });
    }
}
