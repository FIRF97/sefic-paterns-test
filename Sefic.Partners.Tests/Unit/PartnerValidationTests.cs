using Sefic.Partners.Services.Helper;
using Xunit;

namespace Sefic.Partners.Tests.Unit;

public class PartnerValidationTests
{
    [Fact]
    public void ValidateName_ValidNameWithWhitespace_ShouldTrimAndPass()
    {
        var isValid = PartnerValidator.ValidateName(" Ana Silva ", out var trimmed, out var error);
        Assert.True(isValid);
        Assert.Equal("Ana Silva", trimmed);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")] // Menos de 2 caracteres
    public void ValidateName_TooShortOrEmpty_ShouldFail(string raw)
    {
        var isValid = PartnerValidator.ValidateName(raw, out _, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateName_Exceeds100Characters_ShouldFail()
    {
        var longName = new string('A', 101);
        var isValid = PartnerValidator.ValidateName(longName, out _, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    public void ValidateEmail_EmptyOrWhitespace_ShouldReturnNull(string? raw, string? expected)
    {
        var isValid = PartnerValidator.ValidateEmail(raw, out var normalized, out var error);
        Assert.True(isValid);
        Assert.Equal(expected, normalized);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateEmail_ValidEmail_ShouldPass()
    {
        var isValid = PartnerValidator.ValidateEmail("  ana@example.com ", out var normalized, out var error);
        Assert.True(isValid);
        Assert.Equal("ana@example.com", normalized);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("ana@")]
    [InlineData("@dominio.com")]
    public void ValidateEmail_InvalidFormat_ShouldFail(string raw)
    {
        var isValid = PartnerValidator.ValidateEmail(raw, out _, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.50)]
    [InlineData(1000000.00)]
    public void ValidateCreditLimit_ValidValues_ShouldPass(decimal creditLimit)
    {
        var isValid = PartnerValidator.ValidateCreditLimit(creditLimit, out var error);
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000000.01)]
    public void ValidateCreditLimit_OutOfRange_ShouldFail(decimal creditLimit)
    {
        var isValid = PartnerValidator.ValidateCreditLimit(creditLimit, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(10.999)]
    [InlineData(10.1234)]
    [InlineData(0.001)]
    public void ValidateCreditLimit_ExcessivePrecision_ShouldFail(decimal creditLimit)
    {
        var isValid = PartnerValidator.ValidateCreditLimit(creditLimit, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("duas casas decimais", error);
    }

    [Fact]
    public void ValidateVersion_Valid8ByteBase64_ShouldPass()
    {
        // 8 bytes em Base64
        var validBase64 = Convert.ToBase64String(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 });
        var isValid = PartnerValidator.ValidateVersion(validBase64, out var bytes, out var error);
        Assert.True(isValid);
        Assert.Equal(8, bytes.Length);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("not-base64!")]
    [InlineData("AAAA")] // Apenas 3 bytes (menos de 8)
    public void ValidateVersion_MalformedOrInvalid_ShouldFail(string? rawVersion)
    {
        var isValid = PartnerValidator.ValidateVersion(rawVersion, out _, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(0, 20)] // Page = 0
    [InlineData(-1, 20)] // Page < 0
    [InlineData(1, 0)]  // PageSize = 0
    [InlineData(1, 101)] // PageSize > 100
    public void ValidatePagination_InvalidPageOrPageSize_ShouldFail(int page, int pageSize)
    {
        var isValid = PartnerValidator.ValidatePagination(page, pageSize, out var error);
        Assert.False(isValid);
        Assert.NotNull(error);
    }
}
