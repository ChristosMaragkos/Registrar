using Registrar.Base;
using Registrar.Implementation;

namespace Registrar.Tests;

public class RegistryFreezeTests
{
    [Fact]
    public void Freeze_Idempotent()
    {
        var reg = new SimpleRegistry<string>();
        Registry.Register(reg, Identifier.FromNamespaceAndPath("vanilla","a"), "A");
        reg.Freeze();
        reg.Freeze();
        Assert.True(reg.IsFrozen);
        Assert.Equal("A", reg.Get(Identifier.FromNamespaceAndPath("vanilla","a")));
    }

    [Fact]
    public void Register_AfterFreeze_Throws()
    {
        var reg = new SimpleRegistry<string>();
        Registry.Register(reg, Identifier.FromNamespaceAndPath("vanilla","a"), "A");
        reg.Freeze();
        Assert.Throws<InvalidOperationException>(() =>
            Registry.Register(reg, Identifier.FromNamespaceAndPath("vanilla","b"), "B"));
    }

    [Fact]
    public void Reads_Work_AfterFreeze_NoMutation()
    {
        var reg = new SimpleRegistry<string>();
        var id = Identifier.FromNamespaceAndPath("vanilla","a");
        Registry.Register(reg, id, "A");
        reg.Freeze();
        Assert.True(reg.IsFrozen);
        Assert.Equal("A", reg.Get(id));
        Assert.Equal(id, reg.GetId("A"));
        Assert.Equal(0, reg.GetRawId("A"));
        Assert.Single(reg.ToList());
    }

    [Fact]
    public void ReRegister_SameIdentifier_AfterFreeze_Throws()
    {
        var reg = new SimpleRegistry<string>();
        var id = Identifier.FromNamespaceAndPath("vanilla","a");
        Registry.Register(reg, id, "A");
        reg.Freeze();
        Assert.Throws<InvalidOperationException>(() => Registry.Register(reg, id, "A"));
    }

    [Fact]
    public void Freeze_DoesNotChangeExistingValues()
    {
        var reg = new SimpleRegistry<string>();
        var id1 = Identifier.FromNamespaceAndPath("vanilla","a");
        var id2 = Identifier.FromNamespaceAndPath("vanilla","b");
        Registry.Register(reg, id1, "A");
        Registry.Register(reg, id2, "B");
        var before = reg.ToList();
        reg.Freeze();
        var after = reg.ToList();
        Assert.Equal(before.Count, after.Count);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ParallelReadsAfterFreeze_NoExceptions()
    {
        var reg = new SimpleRegistry<string>();
        for (var i = 0; i < 100; i++)
        {
            Registry.Register(reg, Identifier.FromNamespaceAndPath("v", $"id{i}"), $"V{i}");
        }
        reg.Freeze();
        var tasks = new List<Task>();
        for (int t = 0; t < 50; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var id = Identifier.FromNamespaceAndPath("v", $"id{i}");
                    var v = reg.Get(id);
                    Assert.NotNull(v);
                }
            }));
        }
        await Task.WhenAll(tasks);
        Assert.True(reg.IsFrozen);
    }
}

