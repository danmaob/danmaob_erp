using DanmaobErp.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

RequiredConfiguration.EnsurePresent(builder.Configuration, RequiredConfigurationKeys.All);
var allowedOrigins = CorsOriginsParser.Parse(builder.Configuration[RequiredConfigurationKeys.CorsAllowedOrigins]);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
              .WithHeaders("Authorization", "Content-Type");
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
