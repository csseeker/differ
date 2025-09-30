using Differ.Core.Models;
using FluentAssertions;

namespace Differ.Tests.Unit.Core;

public class OperationResultTests
{
    [Fact]
    public void Success_WithData_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var data = "test data";

        // Act
        var result = OperationResult<string>.Success(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(data);
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Test error";

        // Act
        var result = OperationResult<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessageAndException_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Test error";
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = OperationResult<string>.Failure(errorMessage, exception);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Exception.Should().Be(exception);
    }

    [Fact]
    public void SuccessWithoutData_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = OperationResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void FailureWithoutData_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Test error";

        // Act
        var result = OperationResult.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Exception.Should().BeNull();
    }
}