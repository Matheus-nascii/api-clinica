using System.Text.RegularExpressions;

namespace ApiClinica.Validations;

// Regras de validação reaproveitadas pelos controllers
public static class Validador
{
    // Ex.: nome@dominio.com
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    // Formato exigido: (47) 98888-7777  (também aceita fixo: (47) 3333-4444)
    // Obs.: NÃO usamos o atributo [Phone] do C#, conforme o enunciado.
    private static readonly Regex TelefoneRegex =
        new(@"^\(\d{2}\) \d{4,5}-\d{4}$", RegexOptions.Compiled);

    public static bool EmailValido(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    public static bool TelefoneValido(string? telefone) =>
        !string.IsNullOrWhiteSpace(telefone) && TelefoneRegex.IsMatch(telefone.Trim());

    public static bool DataNascValida(DateTime data) => data.Date <= DateTime.Today;

    public static string SomenteNumeros(string? texto) =>
        new string((texto ?? string.Empty).Where(char.IsDigit).ToArray());

    // Validação numérica do CPF (dígitos verificadores)
    public static bool CpfValido(string? cpf)
    {
        var numeros = SomenteNumeros(cpf);

        if (numeros.Length != 11) return false;
        if (numeros.All(c => c == numeros[0])) return false; // 000.000.000-00, 111..., etc.

        int[] digitos = numeros.Select(c => c - '0').ToArray();

        // 1º dígito verificador: pesos 10..2 sobre os 9 primeiros dígitos
        int soma = 0;
        for (int i = 0; i < 9; i++) soma += digitos[i] * (10 - i);
        int resto = soma * 10 % 11;
        if (resto == 10) resto = 0;
        if (resto != digitos[9]) return false;

        // 2º dígito verificador: pesos 11..2 sobre os 10 primeiros dígitos
        soma = 0;
        for (int i = 0; i < 10; i++) soma += digitos[i] * (11 - i);
        resto = soma * 10 % 11;
        if (resto == 10) resto = 0;
        return resto == digitos[10];
    }

    // 52998224725 -> 529.982.247-25
    public static string FormatarCpf(string cpf) =>
        cpf.Length == 11
            ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
            : cpf;
}
