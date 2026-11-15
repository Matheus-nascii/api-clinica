using System.Text;
using ApiClinica.Data;
using ApiClinica.Interfaces;
using ApiClinica.Mappers;
using ApiClinica.Models;
using ApiClinica.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
// A chave não fica no repositório: sem ela a API não inicia e não emite tokens inseguros.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Configure Jwt:Key com pelo menos 32 caracteres por variável de ambiente ou User Secrets.");

builder.Services.AddControllers();
// Cada requisição recebe um DbContext próprio, conectado ao arquivo SQLite configurado.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// Mappers e services são Scoped porque trabalham com o DbContext da requisição atual.
builder.Services.AddScoped<IPacienteMapper, PacienteMapper>();
builder.Services.AddScoped<IMedicoMapper, MedicoMapper>();
builder.Services.AddScoped<IConsultaMapper, ConsultaMapper>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // Todo Bearer token é conferido antes de chegar a ações marcadas com [Authorize].
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // Recusa tokens alterados ou assinados com outra chave.
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true, // Aceita somente tokens emitidos por esta API.
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, // Impede o uso de token destinado a outra aplicação.
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true, // Recusa token expirado.
        ClockSkew = TimeSpan.Zero // Não concede minutos extras após a expiração.
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
// Nesta ordem, o middleware primeiro identifica o usuário pelo JWT e depois verifica permissões.
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Cria ou atualiza as tabelas do SQLite conforme as migrations, sem recriar o banco.
    await db.Database.MigrateAsync();
    // Só cria o Admin se as credenciais vierem da configuração segura e o email ainda não existir.
    await scope.ServiceProvider.GetRequiredService<IAuthService>().InitializeAdminAsync();
}

app.MapControllers();
app.Run();
