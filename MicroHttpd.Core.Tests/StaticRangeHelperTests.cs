using MicroHttpd.Core.Content;
using System;
using System.Linq;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class StaticRangeHelperTests
    {
		[Theory]
		[InlineData("bytes=199-1023", 199, 1023)]
		[InlineData("bytes=992-", 992, long.MinValue)]
		[InlineData("bytes = 199 - 1023", 199, 1023)]
		public void CanParseValidSingleRange(
			string headerField,
			long expectedRangeFrom,
			long expectedRangeTo)
		{
			Assert.Equal(
				StaticRangeHelper.GetRequestedRanges(headerField)[0],
				new StaticRangeRequest(expectedRangeFrom, expectedRangeTo)
				);
		}

		[Fact]
		public void CanParseValidMultiRange()
		{
			Assert.True(
				StaticRangeHelper.GetRequestedRanges("bytes=11-22, 22-33, 44-55")
					.SequenceEqual(
					new StaticRangeRequest[]
					{
						new StaticRangeRequest(11, 22),
						new StaticRangeRequest(22, 33),
						new StaticRangeRequest(44, 55)
					}));
		}

		[Theory]
		[InlineData("asdSd")]
		[InlineData("bytes=19912312")]
		[InlineData("bytes=199.5-")]
		[InlineData("bytes=299-199.5")]
		public void ThrowsExceptionOnInvalidRanges(string headerField)
		{
			Assert.Throws<ArgumentException>(delegate
			{
				StaticRangeHelper.GetRequestedRanges(headerField);
			});
		}
    }
}
