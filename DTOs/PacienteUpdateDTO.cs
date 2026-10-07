namespace ApiClinica.DTOs;

// PATCH: todos os campos são opcionais (null = não alterar)
public class PacienteUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNasc { get; set; }

    // Existe só para podermos RECUSAR a alteração com uma mensagem clara.
    // Regra: CPF não pode ser alterado no PATCH.
    public string? Cpf { get; set; }
}
