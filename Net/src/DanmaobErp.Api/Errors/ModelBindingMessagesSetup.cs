using DanmaobErp.Application.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace DanmaobErp.Api.Errors;

public sealed class ModelBindingMessagesSetup : IConfigureOptions<MvcOptions>
{
    private readonly IStringLocalizer _localizer;

    public ModelBindingMessagesSetup(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public void Configure(MvcOptions options)
    {
        var provider = options.ModelBindingMessageProvider;
        provider.SetMissingBindRequiredValueAccessor((_) => _localizer[MessageKeys.Validation.Required].Value);
        provider.SetMissingKeyOrValueAccessor(() => _localizer[MessageKeys.Validation.Required].Value);
        provider.SetMissingRequestBodyRequiredValueAccessor(() => _localizer[MessageKeys.Validation.Required].Value);
        provider.SetValueMustNotBeNullAccessor((_) => _localizer[MessageKeys.Validation.Required].Value);
        provider.SetAttemptedValueIsInvalidAccessor((_, _) => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor((_) => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetUnknownValueIsInvalidAccessor((_) => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetValueIsInvalidAccessor((_) => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetValueMustBeANumberAccessor((_) => _localizer[MessageKeys.Validation.InvalidValue].Value);
        provider.SetNonPropertyValueMustBeANumberAccessor(() => _localizer[MessageKeys.Validation.InvalidValue].Value);
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    }
}
