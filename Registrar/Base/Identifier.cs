using System;
using System.Text.RegularExpressions;
namespace Registrar.Base
{
    /// <summary>
    /// The Identifier struct represents a unique
    /// identifier composed of a namespace and a path.
    /// It can be used to uniquely identify
    /// resources, objects, or files within a given context.
    /// The format for an identifier is "namespace:path",
    /// where the namespace and path adhere to specific character rules.
    /// The namespace must consist of lowercase letters,
    /// digits, underscores, hyphens, and periods.
    /// The path must consist of lowercase letters,
    /// digits, underscores, hyphens, periods, and slashes.
    /// Examples of valid identifiers include
    /// "example_namespace:some/path/to/resource and "my-mod:items/sword".
    /// Invalid identifiers, such as those with uppercase letters or special characters,
    /// will result in exceptions when attempting to create them.
    /// </summary>
    public readonly struct Identifier : IEquatable<Identifier>
    {
        
        #if DEBUG && NET8_0
        public string GetNamespace_Debug() => Namespace;
        public string GetPath_Debug() => Path;
        #endif
        
        private static readonly Regex NamespaceRegex = new Regex("^[a-z0-9_\\-.]+$");
        private static readonly Regex PathRegex = new Regex("^[a-z0-9_\\-./]+$");

        private string Namespace { get; }
        private string Path { get; }

        private readonly int _hash;
        
        public string GetNamespace() => Namespace;
        public string GetPath() => Path;

        private Identifier(string @namespace, string path)
        {
            if (!IsValidNamespace(@namespace))
                throw new ArgumentException("Invalid namespace format", nameof(@namespace));
            if (!IsValidPath(path))
                throw new ArgumentException("Invalid path format", nameof(path));

            Namespace = @namespace;
            Path = path;
            _hash = HashCode.Combine(Namespace, Path);
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
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>The parsed Identifier.</returns>
        public static Identifier TryParse(string input)
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
        
        public override string ToString()
        {
            return $"{Namespace}:{Path}";
        }

        // Equality members. Do not touch.
        public bool Equals(Identifier other)
        {
            if (_hash != other._hash) return false;
            return Namespace == other.Namespace && Path == other.Path;
        }

        public override bool Equals(object? obj)
        {
            return obj is Identifier other && Equals(other);
        }

        public override int GetHashCode() => _hash;
        
        public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
        public static bool operator !=(Identifier left, Identifier right) => !left.Equals(right);
    }
}