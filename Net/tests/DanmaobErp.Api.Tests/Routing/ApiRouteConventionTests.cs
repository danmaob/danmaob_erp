using System.Reflection;
using DanmaobErp.Api.Tests.Errors.TestSupport;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobErp.Api.Tests.Routing;

public sealed class ApiRouteConventionTests
{
    private static List<string> FindUnversionedControllers(Assembly assembly)
    {
        var offenders = new List<string>();
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(ControllerBase).IsAssignableFrom(type) is false || type.IsAbstract is true)
            {
                continue;
            }

            var route = type.GetCustomAttribute<RouteAttribute>();
            if (route is null || route.Template.StartsWith("api/v", StringComparison.Ordinal) is false)
            {
                offenders.Add(type.Name);
            }
        }

        return offenders;
    }

    [Fact]
    public void EveryApiController_UsesVersionedRoute()
    {
        var offenders = FindUnversionedControllers(typeof(Program).Assembly);
        Assert.Empty(offenders);
    }

    [Fact]
    public void RouteCheck_DetectsUnversionedControllers()
    {
        var offenders = FindUnversionedControllers(typeof(ErrorsTestController).Assembly);
        Assert.Contains(offenders, x => x == "ErrorsTestController");
        Assert.DoesNotContain(offenders, x => x == "DocumentationTestController");
    }
}
