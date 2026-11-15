using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Models;
using ApiClinica.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiClinica.Services;

// Implementa cadastro, login, hash de senha, inicializacao opcional do Admin e emissao de JWT.
public class AuthService(AppDbContext context, IConfiguration configuration, IPasswordHasher<Usuario> passwordHasher) : IAuthService
{
    public async Task<ServiceResult<AuthResponseDTO>> RegisterAsync(RegisterDTO dto)
    {
        // Estas condições protegem contra dados vazios/inválidos mesmo antes da gravação no SQLite.
        if (!Validador.EmailValido(dto.Email)) return ServiceResult<AuthResponseDTO>.Fail(ServiceErrorType.Validation, "Email em formato inválido.");
        if (string.IsNullOrWhiteSpace(dto.Nome)) return ServiceResult<AuthResponseDTO>.Fail(ServiceErrorType.Validation, "Nome não pode ser vazio.");
        if (dto.Senha.Length < 8) return ServiceResult<AuthResponseDTO>.Fail(ServiceErrorType.Validation, "A senha deve ter pelo menos 8 caracteres.");

        // Normalizar evita contas duplicadas que diferem apenas por espaços ou letras maiúsculas.
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await context.Usuarios.AnyAsync(u => u.Email == email)) return ServiceResult<AuthResponseDTO>.Fail(ServiceErrorType.Conflict, "Já existe um usuário cadastrado com este email.");

        // O papel é fixado no servidor; o cliente nunca consegue se cadastrar como Admin.
        var usuario = new Usuario { Nome = dto.Nome.Trim(), Email = email, Role = Roles.Usuario };
        // Apenas o hash é persistido: a senha original não é salva nem retornada.
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, dto.Senha);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return ServiceResult<AuthResponseDTO>.Ok(GerarToken(usuario));
    }

    public async Task<ServiceResult<AuthResponseDTO>> LoginAsync(LoginDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await context.Usuarios.SingleOrDefaultAsync(u => u.Email == email);
        // A mesma resposta para email ausente ou senha incorreta evita revelar quais emails existem.
        if (usuario is null || passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha) == PasswordVerificationResult.Failed)
            return ServiceResult<AuthResponseDTO>.Fail(ServiceErrorType.Unauthorized, "Email ou senha inválidos.");
        return ServiceResult<AuthResponseDTO>.Ok(GerarToken(usuario));
    }

    public async Task InitializeAdminAsync()
    {
        var email = configuration["Admin:Email"];
        var senha = configuration["Admin:Senha"];
        // Sem configuração de Admin, não cria conta administrativa automaticamente.
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(senha)) return;
        // Se somente um valor foi configurado, interrompe para não criar um Admin inseguro.
        if (!Validador.EmailValido(email) || string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            throw new InvalidOperationException("Admin:Email deve ser válido e Admin:Senha deve ter pelo menos 8 caracteres.");

        email = email!.Trim().ToLowerInvariant();
        // A consulta evita duplicar o Admin a cada inicialização da API.
        if (await context.Usuarios.AnyAsync(u => u.Email == email)) return;
        var admin = new Usuario { Nome = "Administrador", Email = email, Role = Roles.Admin };
        admin.SenhaHash = passwordHasher.HashPassword(admin, senha);
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();
    }

    private AuthResponseDTO GerarToken(Usuario usuario)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32) throw new InvalidOperationException("Configure Jwt:Key com pelo menos 32 caracteres por variável de ambiente ou User Secrets.");
        // Usa 60 minutos como padrão caso a duração não seja configurada.
        var expirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);
        // Estas claims são lidas pelo middleware: NameIdentifier identifica o usuário e Role libera Admin.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role)
        };
        // HMAC SHA-256 assina o conteúdo; qualquer alteração invalida a assinatura na próxima requisição.
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"], claims: claims,
            expires: expires, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        return new AuthResponseDTO { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpiraEm = expires, Role = usuario.Role };
    }
}
