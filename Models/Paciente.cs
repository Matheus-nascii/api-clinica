namespace ApiClinica.Models;

public class Paciente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataNasc { get; set; }
    public string Cpf { get; set; } = string.Empty; // salvo apenas com números

    // Navegação: um paciente pode ter várias consultas
    public List<Consulta> Consultas { get; set; } = new();
}
