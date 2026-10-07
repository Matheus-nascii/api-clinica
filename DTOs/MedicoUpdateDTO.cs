namespace ApiClinica.DTOs;

// PATCH: null = não alterar
public class MedicoUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? CRM { get; set; }
}
