namespace ApiClinica.Models;

// Entidade persistida na tabela Medicos; representa o profissional que realiza consultas.
public class Medico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CRM { get; set; } = string.Empty;

    // Navegação: um médico pode ter várias consultas
    public List<Consulta> Consultas { get; set; } = new();
}
