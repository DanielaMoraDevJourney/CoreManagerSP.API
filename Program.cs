using CoreManagerSP.API.CoreManager.API.Middleware;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Auth;
using CoreManagerSP.API.CoreManager.Application.Interfaces.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using CoreManagerSP.API.CoreManager.Infrastructure.Services.Auth;
using CoreManagerSP.API.CoreManager.Infrastructure.Services.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.Services.Prestamo; 
using CoreManagerSP.API.CoreManager.Infrastructure.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using CoreManagerSP.API.CoreManager.Infrastructure.Services.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Services.Prestamo.Strategies;
using CoreManagerSP.API.CoreManager.API.Mapping;
using CoreManagerSP.API.CoreManager.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// BASE DE DATOS

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CoreManagerDbContext>(options =>
    options.UseSqlServer(connectionString));

// SERVICIOS
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITipoPrestamoService, TipoPrestamoService>();
builder.Services.AddScoped<IEntidadFinancieraService, EntidadFinancieraService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


// Servicios refactorizados
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<ISimulacionService, SimulacionService>();
builder.Services.AddScoped<IAnalisisService, AnalisisService>();
builder.Services.AddScoped<IMejorasService, MejorasService>();

// Registro de las estrategias para IUsuarioMejoraStrategy
builder.Services.AddScoped<IUsuarioMejoraStrategy, IngresoStrategy>();
builder.Services.AddScoped<IUsuarioMejoraStrategy, TarjetaCreditoStrategy>();

builder.Services.AddScoped<ICreditoCalculator, CreditoCalculator>();

// AUTENTICACIÓN JWT
var jwtSection = builder.Configuration.GetRequiredSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT Key missing.");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "CoreManager";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "CoreManagerUsuarios";

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilterAttribute>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// SWAGGER + SEGURIDAD JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CoreManager API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Description = @"JWT Authorization header using the Bearer scheme.  
                      Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// JSON OPTIONS
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

// APP PIPELINE
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirTodo");
app.UseAuthentication();
app.UseMiddleware<TokenActivoMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
