using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Restaurant.Order.ApiGateway.UnitTest;

public class Tests
{
    [Test]
    public void WebApplication_HostCanBeBuild()
    {
        var sut = Program.CreateWebApplication();
        sut.Host.UseDefaultServiceProvider((context, options) =>
        {
            // In the ASP.NET Core stack, gRPC services, by default, are created with a scoped lifetime.
            // (https://docs.microsoft.com/en-us/aspnet/core/grpc/migration?view=aspnetcore-5.0)
            // It seams that the service builder can not discover that and throw an error if you use scoped registration
            // set ValidateScopes to false then
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });
        var app = sut.Build();
        // Will throw an exception if there is any dependency missing

        // Hint: If you get an "http client is not registered exception" and you registered one you additional registered the service. this is not necessary.
        // (https://github.com/dotnet/extensions/issues/1079)

        // Dependency injection errors in generic registered services (likes grpc( services) are not detected
        // You have to add here every grpc service manually

        // Configs will also not be checked, they are always there but contains only null value
    }
}