using System.Text.Json.Serialization;

namespace Sefic.Partners.Shared.Models;

public class Response<T>
{
    public Response()
    {
        Retorno = new Retorno();
    }

    public Response(T data, Retorno? retorno = null)
    {
        Data = data;
        Retorno = retorno ?? new Retorno();
    }

    [JsonPropertyOrder(10)]
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    [JsonPropertyOrder(11)]
    [JsonPropertyName("retorno")]
    public Retorno Retorno { get; set; } = new Retorno();
}

public class Retorno
{
    [JsonPropertyName("codigo")]
    public int Codigo { get; set; } = 200;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    public Retorno() { }

    public Retorno(int codigo, string mensagem)
    {
        Codigo = codigo;
        Mensagem = mensagem;
    }
}
