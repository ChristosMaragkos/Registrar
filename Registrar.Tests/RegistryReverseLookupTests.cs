using Registrar.Base;
using Registrar.Implementation;

namespace Registrar.Tests;

public class RegistryReverseLookupTests
{
    [Fact]
    public void DuplicateIdentifierRegistration_ReturnsExistingValue()
    {
        var registry = new SimpleRegistry<string>();
        var id = Identifier.FromNamespaceAndPath("demo", "one");
        var first = Registry.Register(registry, id, "Value1");
        var second = Registry.Register(registry, id, "Value2"); // different instance, same id -> should return existing
        Assert.Equal("Value1", first);
        Assert.Equal("Value1", second);
        Assert.Equal(id, registry.GetId("Value1"));
        Assert.Equal(0, registry.GetRawId("Value1"));
        Assert.Equal(1, registry.Count); // registration with same id didn't add new
    }

    [Fact]
    public void DuplicateValueDifferentIdentifier_Throws()
    {
        var registry = new SimpleRegistry<string>();
        var id1 = Identifier.FromNamespaceAndPath("demo", "one");
        var id2 = Identifier.FromNamespaceAndPath("demo", "two");
        Registry.Register(registry, id1, "SameValue");
        var ex = Assert.Throws<InvalidOperationException>(() => Registry.Register(registry, id2, "SameValue"));
        Assert.Contains("already registered", ex.Message);
    }

    [Fact]
    public void ReverseLookup_Succeeds()
    {
        var registry = new SimpleRegistry<string>();
        var id = Identifier.FromNamespaceAndPath("demo", "thing");
        Registry.Register(registry, id, "ThingValue");
        Assert.Equal(id, registry.GetId("ThingValue"));
        Assert.Equal(0, registry.GetRawId("ThingValue"));
        Assert.Equal("ThingValue", registry.GetByRawId(0));
    }

    [Fact]
    public async Task ConcurrentRegistration_NoExceptions_AndCountMatches()
    {
        var registry = new SimpleRegistry<string>();
        var tasks = new List<Task>();
        const int total = 200;
        for (var i = 0; i < total; i++)
        {
            var localI = i; // capture
            tasks.Add(Task.Run(() =>
            {
                var id = Identifier.FromNamespaceAndPath("ns", $"path/{localI}");
                Registry.Register(registry, id, $"Value-{localI}");
            }));
        }
        await Task.WhenAll(tasks);
        Assert.Equal(total, registry.Count);
        // Validate reverse lookups for a sample set
        foreach (var check in new[] {0, 42, 199})
        {
            var value = $"Value-{check}";
            var id = Identifier.FromNamespaceAndPath("ns", $"path/{check}");
            Assert.Equal(id, registry.GetId(value));
            var raw = registry.GetRawId(value);
            Assert.NotNull(raw);
            Assert.Equal(value, registry.GetByRawId(raw.Value));
        }
        // Ensure all raw IDs are unique and map back consistently
        var allValues = registry.ToList();
        var rawIds = new HashSet<int>();
        foreach (var raw 
                 in allValues.Select(v => registry.GetRawId(v)))
        {
            Assert.NotNull(raw);
            Assert.True(rawIds.Add(raw.Value));
        }
        Assert.Equal(total, rawIds.Count);
    }
}