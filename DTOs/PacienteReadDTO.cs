namespace ApiClinica.DTOs;

// Dados que a API DEVOLVE ao cliente
public class PacienteReadDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataNasc { get; set; }
    public string Cpf { get; set; } = string.Empty;
}
