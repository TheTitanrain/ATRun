using Xunit;

namespace ATRun.Tests
{
    public class FileConstantsTests
    {
        [Fact]
        public void AutorunRegKey_ExactValue()
        {
            Assert.Equal(@"Software\Microsoft\Windows\CurrentVersion\Run", FileConstants.AutorunRegKey);
        }

        [Theory]
        [InlineData(".exe")]
        [InlineData(".EXE")]
        [InlineData(".Exe")]
        public void SupportedExtensions_ContainsDotExe_CaseInsensitive(string ext)
        {
            Assert.Contains(ext, FileConstants.SupportedExtensions);
        }

        [Theory]
        [InlineData(".lnk")]
        [InlineData(".bat")]
        [InlineData(".cmd")]
        [InlineData("")]
        [InlineData("exe")]
        public void SupportedExtensions_DoesNotContainUnsupportedExtensions(string ext)
        {
            Assert.DoesNotContain(ext, FileConstants.SupportedExtensions);
        }
    }
}
