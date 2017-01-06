namespace FVModSync.Extensions
{
    using FVModSync.Configuration;
    using System.Linq;

	/// <summary>
	/// Extensions for the <see cref="string"/> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Removes all tabs from the specified <paramref name="value"/> and
		/// returns the new string.
		/// </summary>
		public static string RemoveTabs(this string value)
		{
			return value.Replace("\t", "");
		}

		/// <summary>
		/// Returns the first entry from a comma-separated value.
		/// </summary>
		public static string FirstEntryFromRecord(this string record)
		{
			return record.Split(',').First();
		}

        /// <summary>
        /// Gets the number of entries of a comma-separated value.
        /// </summary>
        public static int RecordLength(this string record)
        {
            return record.Split(',').Length;
        }

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
			string internalName = "\\" + string.Join("\\", pathParts);

			return internalName;
		}

        public static string GetTargetGameFile(this string internalName)
        {
            string targetFile = Config.GameFilePrefix + internalName;
            return targetFile;
        }

        public static string WrapDelimited(this string input)
        {   
            if (input.Contains(','))
            {
                string wrapped = "\"" + input + "\"";
                return wrapped;
            }
            return input;
        }
	}
}