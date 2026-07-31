using System.Text;
using System.Text.Json;
using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Api.Errores;
using MesaSitec.Infraestructura.Autenticacion;
using MesaSitec.Infraestructura.Data;
using MesaSitec.Infraestructura.Data.Semilla;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Authentication:JwtBearer:SecretKey"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"]
    ?? builder.Configuration["Authentication:JwtBearer:Issuer"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"]
    ?? builder.Configuration["Authentication:JwtBearer:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JWT_SECRET no está configurado o tiene menos de 32 caracteres. " +
        "Defínalo como variable de entorno JWT_SECRET.");
}

builder.Services.AddSingleton<IGeneradorTokens>(_ =>
    new GeneradorTokenJwt(jwtSecret, jwtIssuer ?? "mesasitec", jwtAudience ?? "mesasitec-client"));
builder.Services.AddSingleton<IVerificadorPassword, VerificadorPasswordBcrypt>();
builder.Services.AddScoped<IAutenticacionDatos, AutenticacionDatos>();
builder.Services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => JsonNamingPolicy.CamelCase.ConvertName(e.Key),
                e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

        return new ObjectResult(new
        {
            type = "https://mesasitec.local/errores/validacion",
            title = "Error de validación",
            status = StatusCodes.Status422UnprocessableEntity,
            detail = "Los campos enviados no son válidos.",
            codigo = "VALIDACION",
            errores
        })
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity,
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT obtenido en POST /api/v1/auth/login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddExceptionHandler<ErrorHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://mesasitec.local/errores/no-autenticado",
                        title = "No autenticado",
                        status = 401,
                        detail = "Token ausente, inválido o expirado.",
                        codigo = "NO_AUTENTICADO"
                    },
                    options: null,
                    contentType: "application/problem+json");
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
    options.AddPolicy("PermitirFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirFrontend");
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();

    await db.Database.MigrateAsync();
    await SeedData.SembrarAsync(db);

    var tenants = await db.Tenants.CountAsync();
    var usuarios = await db.Usuarios.CountAsync();
    var categorias = await db.Categorias.CountAsync();
    var solicitudes = await db.Solicitudes.CountAsync();

    app.Logger.LogInformation(
        "Base de datos lista: {Tenants} tenants, {Usuarios} usuarios, {Categorias} categorias, {Solicitudes} solicitudes",
        tenants, usuarios, categorias, solicitudes);
}

app.Run();
