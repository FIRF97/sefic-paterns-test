namespace Sefic.Partners.Services.Helper;

public static class TaxNumberHelper
{
    public static string Normalize(string? taxNumber)
    {
        if (string.IsNullOrEmpty(taxNumber))
            return string.Empty;

        return taxNumber
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("-", "")
            .Trim();
    }

    public static bool Validate(string? rawTaxNumber, out string normalized, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(rawTaxNumber))
        {
            normalized = string.Empty;
            errorMessage = "TaxNumber é obrigatório.";
            return false;
        }

        normalized = Normalize(rawTaxNumber);

        if (normalized.Length != 9 || !normalized.All(char.IsAsciiDigit))
        {
            errorMessage = "TaxNumber inválido. Deve conter exatamente nove dígitos ASCII após remover espaços, pontos e hífenes.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
