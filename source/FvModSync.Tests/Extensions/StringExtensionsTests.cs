namespace FvModSync.Tests.Extensions
{
	using FVModSync.Extensions;
	using NUnit.Framework;

	[TestFixture]
	public class StringExtensionsTests
	{
		[TestCase(@"parent\folder\subfolder", @"\subfolder")]
		[TestCase(@"mods\mod_name\cfg\normal\game.csv", @"\cfg\normal\game.csv")]
		public void GetRelevantPathShouldSkipFirstTwoPathPartAndReturnRemainingString(string input, string expected)
		{
			string actual = input.GetRelevantPath();

			Assert.AreEqual(expected, actual);
		}
	}
}