namespace Reusable.IOnymous
{
    public static class UriStringExtensions
    {
        public static bool IsIOnymous(this UriString uri) => SoftString.Comparer.Equals(uri.Scheme, ResourceProvider.DefaultScheme);

        /// <summary>
        /// Gets the Universal Naming Convention (UNC) path for Windows.
        /// </summary>
        public static string ToUnc(this UriString uri)
        {
            return
                uri.Authority
                    ? $"//{(string)uri.Authority}/{((string)uri.Path.Decoded).TrimStart('/')}"
                    : (string)uri.Path.Decoded;
        }
    }
}