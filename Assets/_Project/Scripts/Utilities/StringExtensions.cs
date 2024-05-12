public static class StringExtensions {
    /// <summary>
    /// Computes the FNV-1a hash for the input string. 
    /// The FNV-1a hash is a non-cryptographic hash function known for its speed and good distribution properties.
    /// Useful for creating Dictionary keys instead of using strings.
    /// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    /// </summary>
    /// <param name="str">The input string to hash.</param>
    /// <returns>An integer representing the FNV-1a hash of the input string.</returns>
    public static int ComputeFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (char c in str) {
            hash = (hash ^ c) * 16777619;
        }
        return unchecked((int)hash);
    }
}