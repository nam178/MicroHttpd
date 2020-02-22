using System.Text;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class HttpLineBuilderTests
    {
		[Theory]
		[InlineData("\r\n")]
		[InlineData("\n")]
		public void LineEndsWithinFirstBuffer(string newLineChar)
		{
			var builder = new HttpLineBuilder(Encoding.UTF8);
			var line = $"Very Short Line{newLineChar}The Next Line Longer{newLineChar}";
			var lineBuff = System.Text.Encoding.UTF8.GetBytes(line);

			// Do the first line
			int nextLineStartIndex;
			Assert.True(builder.AppendBuffer(lineBuff, 0, 24, out nextLineStartIndex));
			Assert.Equal(
				14 + newLineChar.Length + 1, 
				nextLineStartIndex);
			Assert.Equal("Very Short Line", builder.Result);

			// Do the second line
			builder.Reset();
			Assert.True(builder.AppendBuffer(
				lineBuff, 
				nextLineStartIndex, 
				lineBuff.Length -nextLineStartIndex, 
				out nextLineStartIndex));
			Assert.Equal(
				14 + newLineChar.Length + 1 +
				19 + newLineChar.Length + 1,
				nextLineStartIndex
				);
			Assert.Equal("The Next Line Longer", builder.Result);
		}

		[Theory]
		[InlineData("\r\n")]
		[InlineData("\n")]
		public void LineCanEndOnSecondBuffer(string newLineChar)
		{
			var builder = new HttpLineBuilder(Encoding.UTF8);
			var line = $"Very Short Line{newLineChar}The Next Line Longer{newLineChar}";
			var lineBuff = System.Text.Encoding.UTF8.GetBytes(line);

			// First append - should be false since it is insufficient data to build a line
			Assert.False(builder.AppendBuffer(lineBuff, 0, 8, out _));

			// Second pass, now the line should be built
			int nextLineStartIndex;
			Assert.True(builder.AppendBuffer(lineBuff, 8, 24, out nextLineStartIndex));
			Assert.Equal(
				14 + newLineChar.Length + 1,
				nextLineStartIndex);
			Assert.Equal("Very Short Line", builder.Result);
		}

		[Fact]
		public void WorksWithEmptyLine()
		{
			var builder = new HttpLineBuilder(Encoding.UTF8);
			var line = "\r\nSecondLine\n";
			var lineBuff = System.Text.Encoding.UTF8.GetBytes(line);

			int nextLineStartIndex;
			Assert.True(builder.AppendBuffer(lineBuff, 0, lineBuff.Length, out nextLineStartIndex));
			Assert.Equal(2, nextLineStartIndex);
			Assert.Equal(string.Empty, builder.Result);
		}
    }
}
