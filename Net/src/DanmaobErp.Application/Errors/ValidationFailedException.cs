namespace DanmaobErp.Application.Errors;

public sealed class ValidationFailedException : Exception
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; }

    public ValidationFailedException(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        : base("One or more validation rules failed.")
    {
        Errors = errors;
    }
}
