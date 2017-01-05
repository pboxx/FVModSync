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
			string actual = input.GetInternalName();

			Assert.AreEqual(expected, actual);
		}

        [TestCase("a,b", 2)]
        [TestCase("a,b,c,d", 4)]
        public void RecordLengthShouldReturnNumberOfParts(string record, int expectedLength)
        {
            int actualLength = record.RecordLength();

            Assert.AreEqual(expectedLength, actualLength);
        }
	}
}