using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Reflection;

namespace AISAM.API.Infrastructure;

public sealed class EnvironmentAwareControllerFeatureProvider : ControllerFeatureProvider
{
    private readonly bool _includeDevelopmentOnlyControllers;

    public EnvironmentAwareControllerFeatureProvider(bool includeDevelopmentOnlyControllers)
    {
        _includeDevelopmentOnlyControllers = includeDevelopmentOnlyControllers;
    }

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo))
        {
            return false;
        }

        if (_includeDevelopmentOnlyControllers)
        {
            return true;
        }

        return !typeInfo.IsDefined(typeof(DevelopmentOnlyAttribute), inherit: true);
    }
}
