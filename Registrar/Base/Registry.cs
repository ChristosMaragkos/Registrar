using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Registrar.Base
{
    /// <summary>
    /// I made this class for the sole purpose of having a non-generic entry point
    /// for the static Register method, so you don't have to specify the type parameter T
    /// when calling Register.
    /// Is it petty? Yes. Do I care? Not really.
    /// It just makes the API a bit cleaner to use, and brings it closer to Minecraft's
    /// <c>Registry.register(Registry.BLOCK, id, block)</c> style.
    /// <para></para>
    /// ...which is probably not a good thing to aspire to,
    /// both because Minecraft was written in another language
    /// and because Minecraft's code reads like a Unabomber manifesto.
    /// </summary>
    public static class Registry
    {
        public static T Register<T>(Registry<T> registry, Identifier identifier, T content) where T : class
        {
            return Registry<T>.Register(registry, identifier, content);
        }
        
        public static T Register<T>(Registry<T> registry, string identifier, T content) where T : class
        {
            return Registry<T>.Register(registry, Identifier.Parse(identifier), content);
        }
    }
    
    /// <summary>
    /// Abstract base class for registries that map unique identifiers to content of type T.
    /// Supports registration by <see cref="Identifier"/> as well as retrieval by <see cref="Identifier"/> 
    /// or raw numerical ID. Implements <see cref="IEnumerable{T}"/>
    /// to allow enumeration of stored entries.
    /// <para></para>
    /// Registries can be frozen to prevent further modifications,
    /// ensuring thread-safe read operations without locks.
    /// Once frozen, any attempt to register new entries will throw an exception.
    /// </summary>
    /// <typeparam name="T">The type of content this Registry
    /// instance stores. Must be a reference type.</typeparam>
    public abstract class Registry<T> : IEnumerable<T>, IFreezableRegistry where T : class
    {
        private readonly object _lock = new object();
        private int _nextRawId;
        private readonly Dictionary<Identifier, T> _entriesByIdentifier = new Dictionary<Identifier, T>();
        private readonly Dictionary<int, T> _entriesByRawId = new Dictionary<int, T>();
        // New reverse lookup dictionaries
        private readonly Dictionary<T, Identifier> _identifierByValue; // one-to-one enforced
        private readonly Dictionary<T, int> _rawIdByValue;

        private volatile bool _frozen;

        public bool IsFrozen => _frozen;

        protected Registry(IEqualityComparer<T>? comparer = null)
        {
            var valueComparer = comparer ?? EqualityComparer<T>.Default;
            _identifierByValue = new Dictionary<T, Identifier>(valueComparer);
            _rawIdByValue = new Dictionary<T, int>(valueComparer);
        }
        /// <summary>
        /// Registers a new entry in the registry with the specified identifier and content.
        /// If the identifier already exists, the existing content is returned.
        /// </summary>
        /// <param name="registry">The registry instance where the entry will be registered.</param>
        /// <param name="identifier">The unique identifier for the content.</param>
        /// <param name="content">The content to register.</param>
        /// <returns>The registered content, either newly added or already existing.</returns>
        protected internal static T Register(Registry<T> registry, Identifier identifier, T content)
        {
            // Fast path: prevent entering lock if already frozen and known to reject
            if (registry._frozen)
                throw new InvalidOperationException("Registry is already frozen");
            lock (registry._lock)
            {
                if (registry._frozen)
                    throw new InvalidOperationException("Registry is already frozen");
                if (registry._entriesByIdentifier.TryGetValue(identifier, out var registeredValue))
                    return registeredValue; // id already present, return existing value
                // Enforce uniqueness of value -> identifier
                if (registry._identifierByValue.TryGetValue(content, out var existingId))
                {
                    // If same identifier, treat as idempotent (shouldn't happen since earlier check). If different, throw.
                    return !existingId.Equals(identifier) 
                        ? throw new InvalidOperationException($"Value already registered under identifier '{existingId}'. Duplicate registration with '{identifier}' is not allowed.") : content;
                }

                var rawId = registry._nextRawId;
                registry._nextRawId++;

                registry._entriesByIdentifier[identifier] = content;
                registry._entriesByRawId[rawId] = content;
                registry._identifierByValue[content] = identifier;
                registry._rawIdByValue[content] = rawId;
                return content;
            }
        }

        /// <summary>
        /// Retrieves content from the registry by its identifier (thread-safe).
        /// If the identifier is not found, the fallback value is returned.
        /// </summary>
        /// <param name="identifier">The identifier of the content to retrieve.</param>
        /// <returns>The content associated with the identifier, or the fallback value if not found.</returns>
        public T? Get(Identifier identifier)
        {
            if (_frozen)
                return _entriesByIdentifier.TryGetValue(identifier, out var v) ? v : GetValueOnFail();
            lock (_lock)
            {
                return _entriesByIdentifier.TryGetValue(identifier, out var registeredValue)
                    ? registeredValue
                    : GetValueOnFail();
            }
        }

        /// <summary>
        /// Retrieves content from the registry by its raw numerical ID (thread-safe).
        /// If the raw ID is not found, the fallback value is returned.
        /// </summary>
        /// <param name="rawId">The raw numerical ID of the content to retrieve.</param>
        /// <returns>The content associated with the raw ID, or the fallback value if not found.</returns>
        public T? GetByRawId(int rawId)
        {
            if (_frozen)
                return _entriesByRawId.TryGetValue(rawId, out var v) ? v : GetValueOnFail();
            lock (_lock)
            {
                return _entriesByRawId.TryGetValue(rawId, out var registeredValue)
                    ? registeredValue
                    : GetValueOnFail();
            }
        }

        /// <summary>
        /// Checks if the registry contains an entry with the specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier to check for.</param>
        /// <returns>True if the identifier exists in the registry; otherwise, false.</returns>
        public bool ContainsId(Identifier identifier)
        {
            if (_frozen) return _entriesByIdentifier.ContainsKey(identifier);
            lock (_lock) { return _entriesByIdentifier.ContainsKey(identifier); }
        }

        /// <summary>
        /// Checks if the registry contains an entry with the specified raw numerical ID.
        /// </summary>
        /// <param name="rawId">The raw numerical ID to check for.</param>
        /// <returns>True if the raw ID exists in the registry; otherwise, false.</returns>
        public bool ContainsRawId(int rawId)
        {
            if (_frozen) return _entriesByRawId.ContainsKey(rawId);
            lock (_lock) { return _entriesByRawId.ContainsKey(rawId); }
        }

        /// <summary>
        /// Retrieves the identifier associated with the specified content.
        /// If the content is not found, the fallback identifier is returned.
        /// </summary>
        /// <param name="value">The content to search for.</param>
        /// <returns>The identifier associated with the content, or the fallback
        /// identifier if not found.</returns>
        public Identifier? GetId(T value)
        {
            if (_frozen)
                return _identifierByValue.TryGetValue(value, out var id) ? id : GetIdentifierOnFail();
            lock (_lock)
            {
                return _identifierByValue.TryGetValue(value, out var id)
                    ? id
                    : GetIdentifierOnFail();
            }
        }

        /// <summary>
        /// Retrieves the raw numerical ID associated with the specified content.
        /// If the content is not found, the fallback raw ID is returned.
        /// </summary>
        /// <param name="value">The content to search for.</param>
        /// <returns>The raw numerical ID associated with the content, or the fallback
        /// raw ID if not found.</returns>
        public int? GetRawId(T value)
        {
            if (_frozen)
                return _rawIdByValue.TryGetValue(value, out var raw) 
                    ? raw 
                    : GetRawIdOnFail();
            lock (_lock)
            {
                return _rawIdByValue.TryGetValue(value, out var raw)
                    ? raw
                    : GetRawIdOnFail();
            }
        }
        
        /// <summary>
        /// Fetches a random entry from the registry.
        /// Uses the provided Random instance to select an entry.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> to derive the index from.</param>
        /// <returns></returns>
        public T GetRandom(Random random)
        {
            if (_frozen)
            {
                return _entriesByRawId.Count == 0
                    ? throw new InvalidOperationException("Cannot retrieve a random entry from an empty registry.")
                    : _entriesByRawId[random.Next(_entriesByRawId.Count)];
            }
            lock (_lock)
            {
                return _entriesByRawId.Count == 0 ?
                    throw new InvalidOperationException(
                        "Cannot retrieve a random entry from an empty registry.")
                    : _entriesByRawId[random.Next(_entriesByRawId.Count)];
            }
        }

        /// <summary>
        /// Returns the total number of registered entries (thread-safe).
        /// </summary>
        public int Count
        {
            get
            {
                if (_frozen) return _entriesByIdentifier.Count;
                lock (_lock) return _entriesByIdentifier.Count;
            }
        }

        /// <summary>
        /// Converts the registry's entries to a list of content.
        /// </summary>
        /// <returns>A list of all content stored in the registry.</returns>
        public List<T> ToList()
        {
            if (_frozen) return _entriesByIdentifier.Values.ToList();
            lock (_lock)
            {
                return _entriesByIdentifier.Values.ToList();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the content in the registry.
        /// </summary>
        /// <returns>An enumerator for the content in the registry.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            List<T> snapshot;
            if (_frozen)
            {
                snapshot = _entriesByIdentifier.Values.ToList();
            }
            else
            {
                lock (_lock)
                {
                    snapshot = _entriesByIdentifier.Values.ToList();
                }
            }
            return snapshot.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the content in the registry.
        /// </summary>
        /// <returns>An enumerator for the content in the registry.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Retrieves the fallback value when a lookup fails.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <returns>The fallback value.</returns>
        protected abstract T? GetValueOnFail();

        /// <summary>
        /// Retrieves the fallback identifier when a lookup fails.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <returns>The fallback identifier.</returns>
        protected abstract Identifier? GetIdentifierOnFail();

        /// <summary>
        /// Retrieves the fallback raw numerical ID when a lookup fails.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <returns>The fallback raw numerical ID.</returns>
        protected abstract int? GetRawIdOnFail();

        /// <summary>
        /// Freezes the registry, preventing further modifications.
        /// </summary>
        public void Freeze()
        {
            if (_frozen) return; // idempotent
            lock (_lock)
            {
                _frozen = true;
            }
        }
    }

    /// <summary>
    /// Interface for registries that support freezing.
    /// Freezing a registry prevents any further modifications,
    /// such as adding or removing entries.
    /// </summary>
    public interface IFreezableRegistry
    {
        /// <summary>
        /// Freezes the registry, preventing further modifications.
        /// </summary>
        void Freeze();

        /// <summary>
        /// Indicates whether the registry is frozen.
        /// </summary>
        bool IsFrozen { get; }
    }
}