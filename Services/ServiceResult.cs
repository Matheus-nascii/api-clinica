namespace ApiClinica.Services;

// O controller usa estes tipos para escolher o status HTTP sem o service depender de MVC.
// Categorias de erro independentes de HTTP, traduzidas pelos controllers em status codes.
public enum ServiceErrorType { None, Validation, NotFound, Conflict, Unauthorized }

// Envelope padronizado para um resultado de caso de uso, com valor ou erros de dominio.
public sealed class ServiceResult<T>
{
    // Nenhum erro representa sucesso; Value contém o DTO/bool que será usado na resposta.
    public bool Success => ErrorType == ServiceErrorType.None;
    public T? Value { get; init; }
    public ServiceErrorType ErrorType { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static ServiceResult<T> Ok(T value) => new() { Value = value };
    public static ServiceResult<T> Fail(ServiceErrorType errorType, params string[] errors) => new()
    {
        ErrorType = errorType,
        Errors = errors
    };
}
