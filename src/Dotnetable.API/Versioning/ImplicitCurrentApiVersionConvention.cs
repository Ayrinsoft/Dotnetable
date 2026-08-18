using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Dotnetable.API.Versioning;

/// <summary>
/// Controllers without an explicit <see cref="ApiVersionAttribute"/> or
/// <see cref="ApiVersionNeutralAttribute"/> are published as <see cref="ApiVersions.V1"/>.
/// A later breaking contract is a new controller (or action) with its own
/// <c>[ApiVersion("2.0")]</c> on the same route — v1 stays mapped and keeps working.
/// </summary>
internal sealed class ImplicitCurrentApiVersionConvention : IControllerConvention
{
    public bool Apply(IControllerConventionBuilder controller, ControllerModel controllerModel)
    {
        foreach (var attribute in controllerModel.Attributes)
        {
            if (attribute is ApiVersionAttribute or ApiVersionNeutralAttribute)
                return false;
        }

        controller.HasApiVersion(ApiVersions.V1);
        return true;
    }
}
