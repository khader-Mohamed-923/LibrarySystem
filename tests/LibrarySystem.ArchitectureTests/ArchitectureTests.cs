using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace LibrarySystem.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly ApiAssembly = Assembly.Load("LibrarySystem.API");
    private static readonly Assembly ServicesAssembly = Assembly.Load("LibrarySystem.Services");
    private static readonly Assembly DataAssembly = Assembly.Load("LibrarySystem.Data");

    [Fact]
    public void Services_MustNot_DependOn_API()
    {
        var result = Types.InAssembly(ServicesAssembly)
            .Should()
            .NotHaveDependencyOn("LibrarySystem.API")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Services layer must not reference the API layer — dependency flows API → Services, not the reverse");
    }

    [Fact]
    public void Data_MustNot_DependOn_Services()
    {
        var result = Types.InAssembly(DataAssembly)
            .Should()
            .NotHaveDependencyOn("LibrarySystem.Services")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Data layer must not reference Services — dependency flows Services → Data");
    }

    [Fact]
    public void Data_MustNot_DependOn_API()
    {
        var result = Types.InAssembly(DataAssembly)
            .Should()
            .NotHaveDependencyOn("LibrarySystem.API")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Data layer must not reference API layer");
    }

    [Fact]
    public void Controllers_MustNot_DirectlyReference_LibraryDbContext()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .NotHaveDependencyOn("LibrarySystem.Data.LibraryDbContext")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Controllers must never reference LibraryDbContext directly — all DB access goes through service interfaces");
    }

    [Fact]
    public void Controllers_MustNot_DependOn_RepositoryInterfaces_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .NotHaveDependencyOn("LibrarySystem.Data.Repositories")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Controllers must not inject repositories directly — they must only depend on service interfaces");
    }

    [Fact]
    public void ServiceClasses_MustImplement_IServiceInterface()
    {
        var serviceTypes = ServicesAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract);

        foreach (var type in serviceTypes)
        {
            var hasMatchingInterface = type.GetInterfaces()
                .Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Service"));

            hasMatchingInterface.ShouldBeTrue($"Service class '{type.Name}' must implement an interface starting with 'I' and ending with 'Service'");
        }
    }

    [Fact]
    public void RepositoryClasses_MustImplement_IRepositoryInterface()
    {
        var repoTypes = DataAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Repository") && !t.IsInterface && !t.IsAbstract);

        foreach (var type in repoTypes)
        {
            var hasMatchingInterface = type.GetInterfaces()
                .Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Repository"));

            hasMatchingInterface.ShouldBeTrue($"Repository class '{type.Name}' must implement an interface starting with 'I' and ending with 'Repository'");
        }
    }

    [Fact]
    public void Controllers_MustReside_InControllersNamespace()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .ResideInNamespace("LibrarySystem.API.Controllers")
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("All MVC controllers must live in the LibrarySystem.API.Controllers namespace");
    }

    [Fact]
    public void NoClass_InAnyProject_ShouldHaveManagerInName()
    {
        foreach (var assembly in new[] { ApiAssembly, ServicesAssembly, DataAssembly })
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveNameMatching(".*Manager.*")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue($"Assembly '{assembly.GetName().Name}' contains a class with 'Manager' in its name — use 'Service', 'Handler', or 'Coordinator' instead");
        }
    }

    [Fact]
    public void CustomExceptions_MustInheritFromException()
    {
        foreach (var assembly in new[] { ApiAssembly, ServicesAssembly, DataAssembly })
        {
            var result = Types.InAssembly(assembly)
                .That().HaveNameEndingWith("Exception")
                .Should()
                .Inherit(typeof(Exception))
                .GetResult();

            result.IsSuccessful.ShouldBeTrue($"All custom exception classes in '{assembly.GetName().Name}' must inherit from Exception");
        }
    }

    [Fact]
    public void ServiceClasses_MustNotBeStatic()
    {
        var result = Types.InAssembly(ServicesAssembly)
            .That().HaveNameEndingWith("Service")
            .ShouldNot()
            .BeStatic()
            .GetResult();
            
        result.IsSuccessful.ShouldBeTrue("Service classes must not be static — they need to be instantiable for dependency injection");
    }

    [Fact]
    public void EntityNavigationProperties_MustBeInitialized_ToAvoidNullReferenceExceptions()
    {
        var entityTypes = DataAssembly.GetTypes()
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Entities") &&
                        !t.IsInterface && !t.IsAbstract);

        foreach (var type in entityTypes)
        {
            var collectionProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition()
                     .IsAssignableTo(typeof(IEnumerable<>)));

            foreach (var prop in collectionProps)
            {
                var instance = Activator.CreateInstance(type);
                if (instance == null) continue;

                var value = prop.GetValue(instance);
                value.ShouldNotBeNull($"Navigation property '{type.Name}.{prop.Name}' is null by default. Initialize it: public ICollection<X> {prop.Name} = new List<X>();");
            }
        }
    }
}
