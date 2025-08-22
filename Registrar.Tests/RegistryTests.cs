using Registrar.Base;
using Registrar.Implementation;

namespace Registrar.Tests;

public class RegistryTests
{
    [Fact]
    public void SimpleRegistry_GetValueOnFail_ReturnsNull()
    {
        var registry = new SimpleRegistry<string>();
        Assert.Null(registry.Get(Identifier.FromNamespaceAndPath("whyareyou", "readingthis")));
    }

    [Fact]
    public void SimpleRegistry_GetIdentifierOnFail_ReturnsNull()
    {
        var registry = new SimpleRegistry<string>();
        Assert.Null(registry.GetId("Why are you reading this?"));
    }

    [Fact]
    public void SimpleRegistry_GetRawIdOnFail_ReturnsNull()
    {
        var registry = new SimpleRegistry<string>();
        Assert.Null(registry.GetRawId("Again, why are you reading this?"));
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetValueOnFail_ReturnsDefaultValue()
    {
        const string defaultValue = "default";
        var registry =
            new SimpleDefaultedRegistry<string>(defaultValue, 
                Identifier.FromNamespaceAndPath("namespace", "path"));
        var identifier = Identifier.FromNamespaceAndPath("not-the", "default");
        Assert.Equal(defaultValue, registry.Get(identifier));
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetIdentifierOnFail_ReturnsDefaultIdentifier()
    {
        const string defaultValue = "default";
        var defaultIdentifier = Identifier.FromNamespaceAndPath("namespace", "path");
        var registry = new SimpleDefaultedRegistry<string>(defaultValue, defaultIdentifier);
        Registry.Register(registry, defaultIdentifier, defaultValue);
        Assert.Equal(defaultIdentifier, registry.GetId(defaultValue));
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetRawIdOnFail_ReturnsNullIfDefaultNotRegistered()
    {
        var registry =
            new SimpleDefaultedRegistry<string>("default", Identifier.FromNamespaceAndPath("namespace", "path"));
        Assert.Null(registry.GetRawId("default"));
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetRawIdOnFail_ReturnsRawIdIfDefaultRegistered()
    {
        const string defaultValue = "default";
        var defaultIdentifier = Identifier.FromNamespaceAndPath("namespace", "path");
        var registry = new SimpleDefaultedRegistry<string>(defaultValue, defaultIdentifier);
        SimpleRegistry<string>.Register(registry, defaultIdentifier, defaultValue);
        Assert.Equal(0, registry.GetRawId(defaultValue));
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetDefaultValue_ReturnsDefaultValue()
    {
        const string defaultValue = "default";
        var registry =
            new SimpleDefaultedRegistry<string>(defaultValue, Identifier.FromNamespaceAndPath("namespace", "path"));
        Assert.Equal(defaultValue, registry.GetDefaultValue());
    }

    [Fact]
    public void SimpleDefaultedRegistry_GetDefaultIdentifier_ReturnsDefaultIdentifier()
    {
        var defaultIdentifier = Identifier.FromNamespaceAndPath("namespace", "path");
        var registry = new SimpleDefaultedRegistry<string>("default", defaultIdentifier);
        Assert.Equal(defaultIdentifier, registry.GetDefaultIdentifier());
    }
}