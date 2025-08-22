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
    /// Registry.register(Registry.BLOCK, id, block) style.
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
    }
    
    /// <summary>
    /// Abstract base class for registries that map unique identifiers to content of type T.
    /// Supports registration by <see cref="Identifier"/> as well as retrieval by <see cref="Identifier"/> 
    /// or raw numerical ID. Implements <see cref="IEnumerable{T}"/>
    /// to allow enumeration of stored entries.
    /// </summary>
    /// <typeparam name="T">The type of content this Registry
    /// instance stores. Must be a reference type.</typeparam>
    public abstract class Registry<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// Tracks the next available raw numerical ID for registration.
        /// </summary>
        private int _nextRawId;

        /// <summary>
        /// Maps <see cref="Identifier"/> keys to their corresponding content.
        /// </summary>
        private readonly Dictionary<Identifier, T> _entriesByIdentifier 
            = new Dictionary<Identifier, T>();

        /// <summary>
        /// Maps raw numerical IDs to their corresponding content.
        /// </summary>
        private protected readonly Dictionary<int, T> EntriesByRawId 
            = new Dictionary<int, T>();
        
        /// <summary>
        /// Registers a new entry in the registry with the specified identifier and content.
        /// If the identifier already exists, the existing content is returned.
        /// </summary>
        /// <param name="registry">The registry instance where the entry will be registered.</param>
        /// <param name="identifier">The unique identifier for the content.</param>
        /// <param name="content">The content to register.</param>
        /// <returns>The registered content, either newly added or already existing.</returns>
        public static T Register(Registry<T> registry, Identifier identifier, T content)
        {
            if (registry._entriesByIdentifier.TryGetValue(identifier, 
                    out var registeredValue)) return registeredValue;

            registry._entriesByIdentifier[identifier] = content;
            registry.EntriesByRawId[registry._nextRawId] = content;
            registry._nextRawId++;
            return content;
        }

        /// <summary>
        /// Retrieves content from the registry by its identifier.
        /// If the identifier is not found, the fallback value is returned.
        /// </summary>
        /// <param name="identifier">The identifier of the content to retrieve.</param>
        /// <returns>The content associated with the identifier, or the fallback value if not found.</returns>
        public T? Get(Identifier identifier)
        {
            return _entriesByIdentifier.TryGetValue(identifier, out var registeredValue)
                ? registeredValue
                : GetValueOnFail();
        }

        /// <summary>
        /// Retrieves content from the registry by its raw numerical ID.
        /// If the raw ID is not found, the fallback value is returned.
        /// </summary>
        /// <param name="rawId">The raw numerical ID of the content to retrieve.</param>
        /// <returns>The content associated with the raw ID, or the fallback value if not found.</returns>
        public T? GetByRawId(int rawId)
        {
            return EntriesByRawId.TryGetValue(rawId, out var registeredValue)
                ? registeredValue
                : GetValueOnFail();
        }

        /// <summary>
        /// Checks if the registry contains an entry with the specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier to check for.</param>
        /// <returns>True if the identifier exists in the registry; otherwise, false.</returns>
        public bool ContainsId(Identifier identifier)
        {
            return _entriesByIdentifier.ContainsKey(identifier);
        }

        /// <summary>
        /// Checks if the registry contains an entry with the specified raw numerical ID.
        /// </summary>
        /// <param name="rawId">The raw numerical ID to check for.</param>
        /// <returns>True if the raw ID exists in the registry; otherwise, false.</returns>
        public bool ContainsRawId(int rawId)
        {
            return EntriesByRawId.ContainsKey(rawId);
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
            return !_entriesByIdentifier.ContainsValue(value)
                ? GetIdentifierOnFail()
                : _entriesByIdentifier.First(kv => kv.Value == value).Key;
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
            return !EntriesByRawId.ContainsValue(value)
                ? GetRawIdOnFail()
                : EntriesByRawId.First(kv => kv.Value == value).Key;
        }
        
        /// <summary>
        /// Fetches a random entry from the registry.
        /// Uses the provided Random instance to select an entry.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> to derive the index from.</param>
        /// <returns></returns>
        public T GetRandom(Random random)
        {
            return EntriesByRawId[random.Next(EntriesByRawId.Count)];
        }

        /// <summary>
        /// Converts the registry's entries to a list of content.
        /// </summary>
        /// <returns>A list of all content stored in the registry.</returns>
        public List<T> ToList()
        {
            return _entriesByIdentifier.Values.ToList();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the content in the registry.
        /// </summary>
        /// <returns>An enumerator for the content in the registry.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _entriesByIdentifier.Values.GetEnumerator();
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
    }
}