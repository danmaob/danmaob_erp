using System.Reflection;

namespace DanmaobErp.Domain;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
