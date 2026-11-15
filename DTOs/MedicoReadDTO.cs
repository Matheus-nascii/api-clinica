namespace ApiClinica.DTOs;

// Contrato da resposta publica de medico, separado da entidade persistida pelo EF Core.
public class MedicoReadDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CRM { get; set; } = string.Empty;
}
