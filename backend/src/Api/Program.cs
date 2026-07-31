using MesaSitec.Infraestructura.Data;
using MesaSitec.Infraestructura.Data.Semilla;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
