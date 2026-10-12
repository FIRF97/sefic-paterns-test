using System.Text.Json.Serialization;

namespace Sefic.Partners.Shared.Models;

public class ErrorResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    public ErrorResponse() { }

    public ErrorResponse(int status, string message, List<string>? errors = null)
    {
        Status = status;
        Message = message;
        Errors = errors;
    }
}
