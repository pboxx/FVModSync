namespace FVModSync.Extensions
{
    using System.Linq;

	/// <summary>
	/// Extensions for the <see cref="string"/> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Drops the first two path parts from the specified <paramref name="path"/>
		/// and returns the remaining relative path.
		/// <remarks>The path format specifies the format
		/// <c>\{mods subfolder}\{name_of_mod}\{original/relevant_path}</c>
		/// so the relevant path is the part of the path that resembles the relative path
		/// to the original game file.
		/// </remarks>
		/// </summary>
		public static string GetInternalName(this string path)
		{
			string[] pathParts = path.Split('\\').Skip(2).ToArray();
			string internalName = string.Join("\\", pathParts);

			return internalName;
		}

        public static string GetInternalScriptName(this string path)
        {
            string[] pathParts = path.Split('\\').Skip(1).ToArray();
            string internalName = string.Join("\\", pathParts);

            return internalName;
        }

        public static string CleanupLuaOperators(this string input)
        {
            string[] doNotUseMe = { "-", ".", "*", "<", ">", "+", "=" };
            string clean = input;

            foreach (string doNotUse in doNotUseMe) {
                clean = clean.Replace(doNotUse, "_");
            }

            return clean;
        }

        public static string WrapDelimited(this string input)
        {
            if (input.Contains(',') || input.Contains("\"\""))
            {
                string wrapped = "\"" + input + "\"";
                return wrapped;
            }
            return input;
        }
	}
}