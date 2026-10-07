using API.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using API;
using API.Nswag;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);

var options = new DataOptions().UseSQLite(builder.Configuration["DB"] ?? "Data Source=dev.db");
builder.Services.AddSingleton(new DataOptions<SatinRoadDatabase>(options));
builder.Services.AddScoped<SatinRoadDatabase>();
builder.Services.AddSingleton<IRandomProvider, SystemRandomProvider>();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyCustomExceptionHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddOpenApiDocument(settings =>
{
    settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor());
    
    settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer", new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from /Auth/Login"
    }));
    settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

builder.Services.AddCors();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SatinRoadDatabase>();
    SatinRoadSeed.EnsureSeeded(db);
}

app.UseExceptionHandler();
app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseOpenApi();
app.UseSwaggerUi();
app.UseAuthentication(); // NEW: who are you?
app.UseAuthorization();  // NEW: are you allowed?
app.MapControllers();
app.Run();

namespace API
{
    public class MyCustomExceptionHandler : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized, // NEW: wrong login
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = exception.Message
            });

            return default;
        }
    }
}