using System;
using System.Diagnostics.CodeAnalysis;
#if NET8_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
using System.Text.RegularExpressions;
namespace Registrar.Base
{
    /// <summary>
    /// The Identifier struct represents an immutable
    /// identifier composed of a namespace and a path.
    /// It can be used to uniquely identify
    /// resources, objects, or files within a given context.
    /// The format for an identifier is "namespace:path",
    /// where the namespace and path adhere to specific character rules.<para></para>
    /// The namespace must consist of lowercase letters,
    /// digits, underscores, hyphens, and periods.
    /// The path must consist of lowercase letters,
    /// digits, underscores, hyphens, periods, and slashes.
    /// Examples of valid identifiers include
    /// "example_namespace:some/path/to/resource and "my-mod:items/sword".
    /// Invalid identifiers, such as those with uppercase letters or special characters,
    /// will result in exceptions when attempting to create them.
    /// </summary>
    #if NET8_0_OR_GREATER
    [JsonConverter(typeof(IdentifierConverter))]
    #endif
    public readonly struct Identifier : IEquatable<Identifier>
    {
        
        #if DEBUG
        public string GetNamespace_Debug() => Namespace;
        public string GetPath_Debug() => Path;
        #endif
        
        private static readonly Regex NamespaceRegex = new Regex("^[a-z0-9_\\-.]+$");
        private static readonly Regex PathRegex = new Regex("^[a-z0-9_\\-./]+$");

        public string Namespace { get; }
        public string Path { get; }

        private Identifier(string @namespace, string path)
        {
            if (!IsValidNamespace(@namespace))
                throw new ArgumentException("Invalid namespace format", nameof(@namespace));
            if (!IsValidPath(path))
                throw new ArgumentException("Invalid path format", nameof(path));

            Namespace = @namespace;
            Path = path;
        }

        // Namespace and path validation methods.
        
        // --- A valid namespace contains only lowercase letters, digits, underscores, hyphens, and periods.
        private static bool IsValidNamespace(string @namespace)
        {
            return !(string.IsNullOrWhiteSpace(@namespace) ||
                   !NamespaceRegex.IsMatch(@namespace));
        }
        
        // --- A valid path contains only lowercase letters, digits, underscores, hyphens and periods,
        //     as well as slashes to denote file paths.
        private static bool IsValidPath(string path)
        {
            return !(string.IsNullOrWhiteSpace(path) ||
                     !PathRegex.IsMatch(path));
        }

        /// <summary>
        /// Creates an identifier from a namespace and path.
        /// The input strings are validated to ensure they conform to expected formats.
        /// </summary>
        /// <param name="namespace">The namespace the identifier belongs to.</param>
        /// <param name="path">The object or file path the identifier points to</param>
        /// <returns></returns>
        public static Identifier FromNamespaceAndPath(string @namespace, string path)
        {
            return new Identifier(@namespace, path);
        }

        /// <summary>
        /// Attempts to parse a string in the format "namespace:path"
        /// and create an Identifier instance.
        /// The input string is validated to ensure it conforms to the expected format.
        /// Throws on invalid input.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>The parsed Identifier.</returns>
        public static Identifier Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace", nameof(input));

            var parts = input.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Input must be in the format 'namespace:path'");

            var @namespace = parts[0];
            var path = parts[1];

            return new Identifier(@namespace, path);
        }

        /// <summary>
        /// Tries to parse a string in the format "namespace:path" and create a new Identifier instance.
        /// The input string is validated to ensure it conforms to the expected format.
        /// Returns true if parsing was successful, false otherwise.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="result">The resulting Identifier</param>
        /// <returns></returns>
        public static bool TryParse(string input, [NotNullWhen(true)] out Identifier? result)
        {
            result = null;
            
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            var parts = input.Split(':');
            if (parts.Length != 2)
                return false;
            
            var @namespace = parts[0];
            var path = parts[1];
            
            if (!IsValidNamespace(@namespace) || !IsValidPath(path))
                return false;
            
            result = new Identifier(@namespace, path);
            return true;
        }
        
        public override string ToString()
        {
            return $"{Namespace}:{Path}";
        }

        public bool Equals(Identifier other)
        {
            if (GetHashCode() != other.GetHashCode()) return false;
            return Namespace == other.Namespace && Path == other.Path;
        }

        public override bool Equals(object? obj)
        {
            return obj is Identifier other && Equals(other);
        }

        public override int GetHashCode() => HashCode.Combine(Namespace, Path);
        
        public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
        public static bool operator !=(Identifier left, Identifier right) => !left.Equals(right);
    }

#if NET8_0_OR_GREATER
    public class IdentifierConverter : JsonConverter<Identifier>
    {
        public override Identifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str == null)
                return default;
            var parts = str.Split(':');
            if (parts.Length != 2)
                return default;
            return Identifier.TryParse(str, out var id) 
                ? id.Value 
                : default;
        }

        public override void Write(Utf8JsonWriter writer, Identifier value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
#endif
}