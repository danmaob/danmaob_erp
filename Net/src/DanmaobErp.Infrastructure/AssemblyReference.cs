using System.Reflection;

namespace DanmaobErp.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
