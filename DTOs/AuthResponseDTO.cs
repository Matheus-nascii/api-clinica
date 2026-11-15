namespace ApiClinica.DTOs;

// Resposta de autenticacao: entrega o JWT, a expiracao em UTC e o papel do usuario, nunca senha/hash.
public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string Role { get; set; } = string.Empty;
}
