using Registrar.Base;

namespace Registrar.Implementation
{
    /// <summary>
    /// A simple registry implementation that maps unique identifiers to content of type T.
    /// Provides basic functionality for retrieving values, identifiers, and raw IDs, with
    /// fallback methods returning null when lookups fail.
    /// </summary>
    /// <typeparam name="T">The type of content stored in the registry. Must be a reference type.</typeparam>
    public class SimpleRegistry<T> : Registry<T> where T : class
    {
        /// <summary>
        /// Retrieves the fallback value when a lookup fails.
        /// In this implementation, the fallback value is always null.
        /// </summary>
        /// <returns>Null, indicating no fallback value is available.</returns>
        protected override T? GetValueOnFail()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the fallback identifier when a lookup fails.
        /// In this implementation, the fallback identifier is always null.
        /// </summary>
        /// <returns>Null, indicating no fallback identifier is available.</returns>
        protected override Identifier? GetIdentifierOnFail()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the fallback raw numerical ID when a lookup fails.
        /// In this implementation, the fallback raw ID is always null.
        /// </summary>
        /// <returns>Null, indicating no fallback raw ID is available.</returns>
        protected override int? GetRawIdOnFail()
        {
            return null;
        }
    }

    /// <summary>
    /// A specialized registry implementation that provides a default value to return
    /// when lookups fail. The default value is specified during construction and is
    /// used as the fallback for value, identifier, and raw ID lookups.
    /// </summary>
    /// <typeparam name="T">The type of content stored in the registry. Must be a reference type.</typeparam>
    public sealed class SimpleDefaultedRegistry<T> : SimpleRegistry<T> where T : class
    {
        private readonly T _defaultValue;

        private readonly Identifier _defaultIdentifier;
        
        private readonly int _defaultRawId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDefaultedRegistry{T}"/> class
        /// with the specified default value.
        /// </summary>
        /// <param name="defaultValue">The default value to use as a fallback for lookups.</param>
        /// <param name="defaultIdentifier">The default identifier to use as a fallback for lookups.</param>
        /// <param name="defaultRawId">The default raw numerical ID to use as a fallback for lookups.</param>
        public SimpleDefaultedRegistry(T defaultValue, Identifier defaultIdentifier, int defaultRawId = -1)
        {
            _defaultValue = defaultValue;
            _defaultIdentifier = defaultIdentifier;
            _defaultRawId = defaultRawId;
        }

        protected override int? GetRawIdOnFail()
        {
            return _defaultRawId;
        }
        
        protected override T GetValueOnFail()
        {
            return _defaultValue;
        }
        
        protected override Identifier? GetIdentifierOnFail()
        {
            return _defaultIdentifier;
        }
        
        /// <summary>
        /// Fetches the default value set for this registry.
        /// </summary>
        /// <returns></returns>
        public T GetDefaultValue() => _defaultValue;
        
        /// <summary>
        /// Fetches the Identifier associated with the default value.
        /// </summary>
        /// <returns></returns>
        public Identifier GetDefaultIdentifier() => _defaultIdentifier;
    }
}