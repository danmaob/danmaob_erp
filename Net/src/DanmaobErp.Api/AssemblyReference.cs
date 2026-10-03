using System.Reflection;

namespace DanmaobErp.Api;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
