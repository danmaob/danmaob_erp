using DanmaobErp.Api.Errors;
using DanmaobErp.Application.Errors;
using DanmaobErp.Application.Tenancy;

namespace DanmaobErp.Api.Tests.Errors;

public sealed class ApiExceptionMapperTests
{
    [Fact]
    public void Map_ValidationFailed_Returns422()
    {
        var error = ApiExceptionMapper.Map(new ValidationFailedException(new Dictionary<string, IReadOnlyList<string>>()));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(ApiErrorCodes.ValidationFailed, error.Code);
    }

    [Fact]
    public void Map_TenantNotResolved_Returns401()
    {
        var error = ApiExceptionMapper.Map(new TenantNotResolvedException());
        Assert.Equal(401, error.StatusCode);
        Assert.Equal(ApiErrorCodes.TenantNotResolved, error.Code);
    }

    [Fact]
    public void Map_WrappedTenantNotResolved_Returns401()
    {
        var error = ApiExceptionMapper.Map(new InvalidOperationException("Wrapped", new TenantNotResolvedException()));
        Assert.Equal(401, error.StatusCode);
        Assert.Equal(ApiErrorCodes.TenantNotResolved, error.Code);
    }

    [Fact]
    public void Map_CrossTenantWrite_Returns403()
    {
        var error = ApiExceptionMapper.Map(new CrossTenantWriteException("Customer"));
        Assert.Equal(403, error.StatusCode);
        Assert.Equal(ApiErrorCodes.CrossTenantWrite, error.Code);
    }

    [Fact]
    public void Map_UnknownException_Returns500()
    {
        var error = ApiExceptionMapper.Map(new InvalidOperationException("Internal"));
        Assert.Equal(500, error.StatusCode);
        Assert.Equal(ApiErrorCodes.InternalError, error.Code);
    }

    [Theory]
    [InlineData(404, "not_found")]
    [InlineData(405, "method_not_allowed")]
    [InlineData(415, "validation_failed")]
    [InlineData(503, "internal_error")]
    public void MapStatusCode_ReturnsExpectedCode(int statusCode, string expectedCode)
    {
        var error = ApiExceptionMapper.MapStatusCode(statusCode);
        Assert.Equal(statusCode, error.StatusCode);
        Assert.Equal(expectedCode, error.Code);
    }
}
