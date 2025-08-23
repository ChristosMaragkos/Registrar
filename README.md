# Registrar

A tiny, zero-dependency (other than the BCL) library that provides a simple, namespaced identifier type and in-memory registries for mapping those identifiers to arbitrary reference-type objects. Targets **.NET 8.0** and **.NET Standard 2.1** for broad compatibility.

> Think of it as a lightweight, strongly-typed key space for plugins, modding data, content packs, or any scenario where you want stable string IDs like `my-mod:items/sword` instead of ad‑hoc dictionaries sprinkled through your code.

## Features
- `Identifier` struct with validated `namespace:path` format
- Simple registration API: `Registry.Register(registry, id, value)`
- Raw integer IDs auto-assigned in registration order
- Defaulted registry variant (`SimpleDefaultedRegistry<T>`) for fallback lookups
- Enumeration support (`foreach` over values)
- Thread-safe registration & lookups via coarse-grained lock (see Issues for planned refinements)
- Multi-targeted build (net8.0 + netstandard2.1)
- Unit test coverage for core behaviors

## Quick Start
```csharp
using Registrar.Base;
using Registrar.Implementation;

// Create a simple registry of some reference type (string for demo)
var registry = new SimpleRegistry<string>();

// Create identifiers
var swordId = Identifier.FromNamespaceAndPath("demo", "items/iron_sword");
var pickId  = Identifier.FromNamespaceAndPath("demo", "items/iron_pickaxe");

// Register
Registry.Register(registry, swordId, "Sword");
Registry.Register(registry, pickId,  "Pickaxe");

// Retrieve
var sword = registry.Get(swordId);          // "Sword"
var firstRaw = registry.GetByRawId(0);      // "Sword" (registration order)

// Reverse lookups (O(n) – see limitations)
var idOfSword = registry.GetId("Sword");    // swordId
var rawOfPick = registry.GetRawId("Pickaxe"); // 1

// Random (ensure non-empty first)
var rng = new Random();
var randomItem = registry.GetRandom(rng);
```

### Defaulted Registry
```csharp
var defaultId = Identifier.FromNamespaceAndPath("base", "items/missing");
var defaulted = new SimpleDefaultedRegistry<string>("<missing>", defaultId);

// Unregistered lookup returns the configured default
var missing = defaulted.Get(Identifier.FromNamespaceAndPath("demo", "items/not_there")); // "<missing>"
```

## The Identifier Format
```
namespace:path
```
- Namespace: lowercase letters, digits, `_ - .`
- Path: lowercase letters, digits, `_ - . /`
- Example: `example_namespace:some/path/to/resource`

Creation helpers:
- `Identifier.FromNamespaceAndPath(ns, path)` – validates and throws on failure.
- `Identifier.TryParse("ns:path")` – CURRENTLY THROWS on invalid input (see Issue #1).

## Testing
Run the existing unit tests:
```bash
dotnet test
```
All current tests (including concurrency) should pass.

## Contributing
1. Fork & branch (`feat/your-feature`)
2. Add/adjust tests
3. Follow standard .NET naming conventions
4. Open a PR referencing the related issue(s)

## License
Licensed under the MIT License. See [LICENSE.md](./LICENSE.md) for details.

## Disclaimer
The library is intentionally small; prioritize clarity and explicit contracts over premature optimization. Feedback and contributions welcome.

---
Feel free to open an issue for additional enhancements or clarifications.
