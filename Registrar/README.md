# Registrar

A tiny, zero-dependency (other than the BCL) library that provides a simple, namespaced identifier type and in-memory registries for mapping those identifiers to arbitrary reference-type objects. Targets **.NET 8.0** and **.NET Standard 2.1** for broad compatibility.

> Lightweight, strongly-typed key space for plugins, data packs, game content, or any scenario needing stable IDs like `my-mod:items/sword`.

## Features
- `Identifier` struct with validated `namespace:path` format
- Simple registration API: `Registry.Register(registry, id, value)`
- Raw integer IDs auto-assigned in registration order
- Defaulted registry (`SimpleDefaultedRegistry<T>`) with fallback value/id/raw id
- Reverse lookups now O(1) (value -> identifier / rawId) with enforced uniqueness
- Thread-safe registration & lookups (coarse locking) + lock-elided reads after freeze
- Freezing: convert a registry to immutable state (`registry.Freeze()`) to prevent further mutation
- Enumeration support (snapshot enumeration for safety)
- Multi-targeted build (net8.0 + netstandard2.1)
- MIT licensed
- Unit tests including concurrency & freeze behavior

## Identifier Format
```
namespace:path
```
- Namespace: `[a-z0-9_.-]+`
- Path: `[a-z0-9_.\-/]+`
- Examples: `example_namespace:some/path`, `vanilla:items/health_potion`

Creation helpers:
- `Identifier.FromNamespaceAndPath(ns, path)` – validates & throws on failure.
- `Identifier.Parse("ns:path")` – throws on invalid format.
- `Identifier.TryParse(string, out Identifier? id)` – returns `true/false` without throwing.

## Static vs Dynamic Registries
Inspired by Minecraft’s model:
- Static registry: Hard-coded (vanilla/core) values only; frozen after bootstrap; further registrations throw `InvalidOperationException("Registry is already frozen")`.
- Dynamic registry (future extension): Intended to allow late additions (e.g. data packs / mods) and remains unfrozen (or supports controlled reload cycles).

Currently, all registries can be frozen manually by calling `Freeze()`. A recommended pattern is to freeze core registries after loading vanilla + mod content.

### Bootstrap Lifecycle Example
```csharp
public interface IRegistrar { void Register(); }

public static class Items : IRegistrar {
    public static readonly SimpleRegistry<Item> Registry = new();
    public static readonly Item HealthPotion = Registry.Register(
        Identifier.FromNamespaceAndPath("vanilla", "health_potion"), new Item());
    public void Register() { /* static field initializers already executed */ }
}

public static class Bootstrap {
    public static void Initialize(IEnumerable<IRegistrar> registrars) {
        foreach (var r in registrars) r.Register(); // perform all registrations
        Items.Registry.Freeze(); // freeze static registry
    }
}
```
Attempting to register AFTER freezing:
```csharp
Registry.Register(Items.Registry, Identifier.FromNamespaceAndPath("vanilla","late"), new Item());
// => InvalidOperationException("Registry is already frozen")
```

## Reverse Lookup & Uniqueness
Value -> Identifier / RawId lookups are now O(1). A single value may only be registered under one identifier. Attempting to register the SAME value under a different identifier throws. Re-registering an existing identifier returns the already stored value (idempotent for that ID).

## Quick Start
```csharp
var registry = new SimpleRegistry<string>();
var swordId = Identifier.FromNamespaceAndPath("demo", "items/iron_sword");
Registry.Register(registry, swordId, "Sword");
registry.Freeze(); // make immutable
var sword = registry.Get(swordId); // "Sword"
```

### Defaulted Registry
```csharp
var defaultId = Identifier.FromNamespaceAndPath("base", "items/missing");
var defaulted = new SimpleDefaultedRegistry<string>("<missing>", defaultId);
// Not found -> returns default
var missing = defaulted.Get(Identifier.FromNamespaceAndPath("demo","items/not_there")); // "<missing>"
```

## Freezing Details
- `Freeze()` is idempotent.
- After freeze: all read APIs skip locks for performance.
- Registering after freeze throws.
- Concurrency: safe to call `Freeze()` while no registration is in progress (call it after bootstrap phase).

## Error Handling
- Parse / validation methods throw on malformed identifiers (`ArgumentException` / `FormatException`).
- Lookup returns fallback (null or defaulted value) when missing.
- `GetRandom(Random)` throws if registry empty.
- Registration after freeze throws.

## Testing
```bash
dotnet test
```
Covers identifier validation, defaulted behavior, reverse lookup, concurrency, and freezing.

## Versioning & Breaking Changes
Potential roadmap items may introduce breaking changes until 1.0 (e.g., specialized dynamic registries). Uniqueness enforcement & proper `TryParse` pattern already introduced.

## Roadmap (Planned / Ideas)
- Dynamic registry abstraction (data-driven reloadable registries)
- Bulk registration & removal APIs
- Optional multi-value mapping mode (value -> multiple identifiers)
- Benchmark project (BenchmarkDotNet) for lock vs freeze read performance
- Source generator for strongly-typed constants

## Contributing
1. Fork & branch
2. Add/adjust tests
3. Keep public APIs documented
4. Open PR referencing related issue(s)

## License
MIT – see [LICENSE.md](./LICENSE.md)

## Disclaimer
Small by design; clarity over premature optimization. Contributions welcome.
