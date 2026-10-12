using Sefic.Partners.Services.Helper;
using Xunit;

namespace Sefic.Partners.Tests.Unit;

public class TaxNumberHelperTests
{
    [Theory]
    [InlineData("001.234.567", "001234567")]
    [InlineData("001 234 567", "001234567")]
    [InlineData("001-234-567", "001234567")]
    [InlineData("  001.234-567 ", "001234567")]
    public void Normalize_ShouldRemoveSeparatorsAndPreserveLeadingZeros(string raw, string expected)
    {
        var normalized = TaxNumberHelper.Normalize(raw);
        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void Validate_ValidNif_ShouldReturnTrueAndNormalizedValue()
    {
        var isValid = TaxNumberHelper.Validate("001.234.567", out var normalized, out var error);
        Assert.True(isValid);
        Assert.Equal("001234567", normalized);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("12345678A")]   // Letras
    [InlineData("123.456.78A")] // Letras com pontos
    [InlineData("12345678")]    // 8 dígitos (menos de 9)
    [InlineData("1234567890")]  // 10 dígitos (mais de 9)
    [InlineData("")]            // Vazio
    [InlineData("   ")]         // Espaços
    [InlineData(null)]          // Nulo
    public void Validate_InvalidNif_ShouldReturnFalseWithError(string? raw)
    {
        var isValid = TaxNumberHelper.Validate(raw, out _, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }
}
