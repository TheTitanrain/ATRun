using System;
using System.IO;
using Xunit;

namespace ATRun.Tests
{
    public class ShellHelperTests
    {
        // ── GetFileDescription ────────────────────────────────────────────────

        [Fact]
        public void GetFileDescription_NonExistentPath_ReturnsFallbackFilename()
        {
            // FileVersionInfo.GetVersionInfo throws FileNotFoundException → catch block
            // returns Path.GetFileNameWithoutExtension as fallback.
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_NoSuchApp.exe");

            string result = ShellHelper.GetFileDescription(path);

            Assert.Equal(Path.GetFileNameWithoutExtension(path), result);
        }

        [Fact]
        public void GetFileDescription_FileWithNoVersionInfo_ReturnsFallbackFilename()
        {
            // A real file (empty bytes) has no version info → FileDescription is
            // null/empty/whitespace → returns Path.GetFileNameWithoutExtension as fallback.
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_Dummy.exe");
            File.WriteAllBytes(path, Array.Empty<byte>());
            try
            {
                string result = ShellHelper.GetFileDescription(path);

                Assert.Equal(Path.GetFileNameWithoutExtension(path), result);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
