using System.Text.Json.Serialization;

namespace Sefic.Partners.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartnerType
{
    CUSTOMER,
    SUPPLIER,
    BOTH
}
