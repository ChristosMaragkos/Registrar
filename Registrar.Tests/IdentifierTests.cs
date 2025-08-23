using Registrar.Base;

namespace Registrar.Tests;

public class IdentifierTests
{
    [Fact]
    public void FromNamespaceAndPath_ValidInputs_CreatesIdentifier()
    {
        var identifier = Identifier.FromNamespaceAndPath("valid-namespace", "valid/path");

        Assert.Equal("valid-namespace", identifier.GetNamespace());
        Assert.Equal("valid/path", identifier.GetPath());
    }

    [Fact]
    public void FromNamespaceAndPath_InvalidNamespace_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Identifier.FromNamespaceAndPath("INVALID_NAMESPACE", "valid/path"));
    }

    [Fact]
    public void FromNamespaceAndPath_InvalidPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Identifier.FromNamespaceAndPath("valid-namespace", "INVALID_PATH"));
    }
    
    [Fact]
    public void TryParse_ValidInput_CreatesIdentifier()
    {
        var identifier = Identifier.TryParse("valid-namespace:valid/path");
        
        Assert.Equal("valid-namespace", identifier.GetNamespace());
        Assert.Equal("valid/path", identifier.GetPath());
    }
    
    [Fact]
    public void TryParse_InvalidNamespace_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Identifier.TryParse("INVALID_NAMESPACE:valid/path"));
    }

    [Fact]
    public void TryParse_InvalidPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Identifier.TryParse("valid-namespace:INVALID_PATH"));
    }

    [Fact]
    public void Equals_SameIdentifiers_ReturnsTrue()
    {
        var identifier1 = Identifier.FromNamespaceAndPath("namespace", "path");
        var identifier2 = Identifier.FromNamespaceAndPath("namespace", "path");

        Assert.True(identifier1.Equals(identifier2));
    }

    [Fact]
    public void Equals_DifferentIdentifiers_ReturnsFalse()
    {
        var identifier1 = Identifier.FromNamespaceAndPath("namespace1", "path1");
        var identifier2 = Identifier.FromNamespaceAndPath("namespace2", "path2");

        Assert.False(identifier1.Equals(identifier2));
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var identifier = Identifier.FromNamespaceAndPath("namespace", "path");

        Assert.Equal("namespace:path", identifier.ToString());
    }

    [Fact]
    public void GetHashCode_SameIdentifiers_ReturnsSameHash()
    {
        var identifier1 = Identifier.FromNamespaceAndPath("namespace", "path");
        var identifier2 = Identifier.FromNamespaceAndPath("namespace", "path");

        Assert.Equal(identifier1.GetHashCode(), identifier2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentIdentifiers_ReturnsDifferentHash()
    {
        var identifier1 = Identifier.FromNamespaceAndPath("namespace1", "path1");
        var identifier2 = Identifier.FromNamespaceAndPath("namespace2", "path2");

        Assert.NotEqual(identifier1.GetHashCode(), identifier2.GetHashCode());
    }
}