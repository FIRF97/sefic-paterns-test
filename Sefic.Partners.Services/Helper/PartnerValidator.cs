using System.ComponentModel.DataAnnotations;
using Sefic.Partners.Shared.Enums;

namespace Sefic.Partners.Services.Helper;

public static class PartnerValidator
{
    public static bool ValidateName(string? name, out string trimmedName, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            trimmedName = string.Empty;
            errorMessage = "Name é obrigatório.";
            return false;
        }

        trimmedName = name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 100)
        {
            errorMessage = "Name deve ter entre 2 e 100 caracteres após Trim.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static bool ValidateEmail(string? email, out string? normalizedEmail, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            normalizedEmail = null;
            errorMessage = null;
            return true;
        }

        normalizedEmail = email.Trim();
        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(normalizedEmail) || !normalizedEmail.Contains('@') || !normalizedEmail.Contains('.'))
        {
            errorMessage = "Formato de e-mail inválido.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static bool ValidateCreditLimit(decimal creditLimit, out string? errorMessage)
    {
        if (creditLimit < 0m || creditLimit > 1_000_000m)
        {
            errorMessage = "CreditLimit deve estar entre 0 e 1 000 000.";
            return false;
        }

        int scale = (decimal.GetBits(creditLimit)[3] >> 16) & 0x7F;
        if (scale > 2 || decimal.Round(creditLimit, 2) != creditLimit)
        {
            errorMessage = "CreditLimit não pode ter mais de duas casas decimais. Rejeite precisão excessiva em vez de arredondar silenciosamente.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static bool ValidateVersion(string? version, out byte[] versionBytes, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            versionBytes = Array.Empty<byte>();
            errorMessage = "Version é obrigatória.";
            return false;
        }

        try
        {
            versionBytes = Convert.FromBase64String(version.Trim());
            if (versionBytes.Length != 8)
            {
                errorMessage = "Version malformada. O token de versão deve representar exatamente 8 bytes em Base64.";
                return false;
            }
        }
        catch (FormatException)
        {
            versionBytes = Array.Empty<byte>();
            errorMessage = "Version malformada. O valor fornecido não é uma string Base64 válida.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static bool ValidatePagination(int page, int pageSize, out string? errorMessage)
    {
        if (page < 1)
        {
            errorMessage = "Page deve ser >= 1.";
            return false;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            errorMessage = "PageSize deve estar entre 1 e 100.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
