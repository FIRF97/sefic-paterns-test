namespace Sefic.Partners.Services.Helper;

public class ServiceResult<T>
{
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ServiceResult<T> Success(T data, string message = "", int statusCode = 200) =>
        new() { StatusCode = statusCode, Data = data, Message = message };

    public static ServiceResult<T> Created(T data, string message = "Registo criado com sucesso.") =>
        new() { StatusCode = 201, Data = data, Message = message };

    public static ServiceResult<T> BadRequest(string message, List<string>? errors = null) =>
        new() { StatusCode = 400, Message = message, Errors = errors };

    public static ServiceResult<T> NotFound(string message = "Registo não encontrado.") =>
        new() { StatusCode = 404, Message = message };

    public static ServiceResult<T> Conflict(string message = "Conflito detetado.") =>
        new() { StatusCode = 409, Message = message };

    public static ServiceResult<T> InternalError(string message = "Erro interno do servidor.") =>
        new() { StatusCode = 500, Message = message };
}
