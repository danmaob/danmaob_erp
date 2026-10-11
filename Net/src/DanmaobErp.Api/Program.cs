using DanmaobErp.Api.Configuration;
using DanmaobErp.Api.Documentation;
using DanmaobErp.Api.Errors;
using DanmaobErp.Api.Tenancy;
using DanmaobErp.Application.Tenancy;
using DanmaobErp.Application.Time;
using DanmaobErp.Infrastructure.ExternalServices;
using DanmaobErp.Infrastructure.Localization;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Time;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

RequiredConfiguration.EnsurePresent(builder.Configuration, RequiredConfigurationKeys.All);
var allowedOrigins = CorsOriginsParser.Parse(builder.Configuration[RequiredConfigurationKeys.CorsAllowedOrigins]);

// Add services to the container.

builder.Services.AddMessageLocalization(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddControllers().AddDataAnnotationsLocalization().AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<ApiDocumentInfoTransformer>());
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
builder.Services.AddPersistence(builder.Configuration[RequiredConfigurationKeys.ErpConnectionString]);
builder.Services.AddBusinessClock();
builder.Services.AddExternalServices(builder.Configuration, builder.Environment.ContentRootPath, builder.Environment.IsDevelopment());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create);
builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ModelBindingMessagesSetup>();
builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ProblemDetailsResponseTypesSetup>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomization.Apply);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

app.Services.GetRequiredService<MessageCatalog>();
app.Services.GetRequiredService<IBusinessClock>();
app.UseRequestLocalization(RequestLocalizationSetup.Create(app.Services.GetRequiredService<LocalizationSettings>()));
app.UseExceptionHandler();
app.UseStatusCodePages();

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
