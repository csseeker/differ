using Differ.Common.Logging;
using FluentAssertions;
using Xunit;

namespace Differ.Tests.Unit.Common;

public class DifferLoggingOptionsTests
{
    [Fact]
    public void GetResolvedLogDirectory_ExpandsEnvironmentVariables()
    {
        var original = Environment.GetEnvironmentVariable("DIFFER_TEST_TEMP");
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("DIFFER_TEST_TEMP", tempPath);

        try
        {
            var options = new DifferLoggingOptions
            {
                LogDirectory = "%DIFFER_TEST_TEMP%"
            };

            var resolved = options.GetResolvedLogDirectory();

            resolved.Should().Be(Path.GetFullPath(tempPath));
        }
        finally
        {
            Environment.SetEnvironmentVariable("DIFFER_TEST_TEMP", original);
        }
    }

    [Fact]
    public void GetLogFilePath_ComposesDirectoryAndFileName()
    {
        var options = new DifferLoggingOptions
        {
            LogDirectory = "logs",
            LogFileName = "test.log"
        };

        var path = options.GetLogFilePath();

        path.Should().EndWith(Path.Combine("logs", "test.log"));
        Path.GetFileName(path).Should().Be("test.log");
    }

    [Fact]
    public void GetResolvedLogDirectory_RejectsEmptyDirectory()
    {
        var options = new DifferLoggingOptions
        {
            LogDirectory = string.Empty
        };

        Action act = () => options.GetResolvedLogDirectory();

        act.Should().Throw<InvalidOperationException>();
    }
}
