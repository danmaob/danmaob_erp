using System.ComponentModel.DataAnnotations;
using DanmaobErp.Application.Localization;

namespace DanmaobErp.Api.Tests.Errors.TestSupport;

public sealed class ErrorsTestInput
{
    [Required(ErrorMessage = MessageKeys.Validation.Required)]
    [MaxLength(5, ErrorMessage = MessageKeys.Validation.MaxLength)]
    public string? Name { get; set; }

    public int Quantity { get; set; }
}
