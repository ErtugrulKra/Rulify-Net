using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rulify.Net;

namespace Rulify.Net.Tests
{
    public class RuleResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            // Arrange & Act
            var result = RuleResult.Success();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.Data);
        }

        [Fact]
        public void Success_WithData_ShouldCreateSuccessfulResultWithData()
        {
            // Arrange
            var data = new Dictionary<string, object> { { "key", "value" } };

            // Act
            var result = RuleResult.Success(data);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            Assert.Single(result.Data);
            Assert.Equal("value", result.Data["key"]);
        }

        [Fact]
        public void Failure_ShouldCreateFailedResult()
        {
            // Arrange
            var errorMessage = "Test error";

            // Act
            var result = RuleResult.Failure(errorMessage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Empty(result.Data);
        }

        [Fact]
        public void Failure_WithData_ShouldCreateFailedResultWithData()
        {
            // Arrange
            var errorMessage = "Test error";
            var data = new Dictionary<string, object> { { "errorCode", 500 } };

            // Act
            var result = RuleResult.Failure(errorMessage, data: data);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Single(result.Data);
            Assert.Equal(500, result.Data["errorCode"]);
        }
    }
}
