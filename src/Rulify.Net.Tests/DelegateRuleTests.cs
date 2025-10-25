using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Rulify.Net;

namespace Rulify.Net.Tests
{
    public class DelegateRuleTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRule()
        {
            // Arrange
            var id = "test-rule";
            var name = "Test Rule";
            var description = "A test rule";
            var priority = 5;
            var evaluator = new Func<string, RuleResult>(context => RuleResult.Success());

            // Act
            var rule = new DelegateRule<string>(id, name, evaluator, description, priority);

            // Assert
            Assert.Equal(id, rule.Id);
            Assert.Equal(name, rule.Name);
            Assert.Equal(description, rule.Description);
            Assert.Equal(priority, rule.Priority);
            Assert.False(rule.IsAsync);
        }

        [Fact]
        public void Constructor_WithNullId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var evaluator = new Func<string, RuleResult>(context => RuleResult.Success());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DelegateRule<string>(null!, "Test Rule", evaluator));
        }

        [Fact]
        public void Constructor_WithNullName_ShouldThrowArgumentNullException()
        {
            // Arrange
            var evaluator = new Func<string, RuleResult>(context => RuleResult.Success());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DelegateRule<string>("test-rule", null!, evaluator));
        }

        [Fact]
        public void Constructor_WithNullEvaluator_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DelegateRule<string>("test-rule", "Test Rule", null!));
        }

        [Fact]
        public void Evaluate_WithSuccessfulRule_ShouldReturnSuccess()
        {
            // Arrange
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            var context = "test context";

            // Act
            var result = rule.Evaluate(context);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Evaluate_WithFailedRule_ShouldReturnFailure()
        {
            // Arrange
            var errorMessage = "Validation failed";
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Failure(errorMessage));
            var context = "test context";

            // Act
            var result = rule.Evaluate(context);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMessage, result.ErrorMessage);
        }

        [Fact]
        public void Evaluate_WithException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => throw new InvalidOperationException("Test exception"));
            var context = "test context";

            // Act
            var result = rule.Evaluate(context);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task EvaluateAsync_WithAsyncRule_ShouldUseAsyncEvaluator()
        {
            // Arrange
            var asyncEvaluator = new Func<string, Task<RuleResult>>(async context =>
            {
                await Task.Delay(10);
                return RuleResult.Success();
            });
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success(), asyncEvaluator);
            var context = "test context";

            // Act
            var result = await rule.EvaluateAsync(context);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(rule.IsAsync);
        }

        [Fact]
        public async Task EvaluateAsync_WithoutAsyncEvaluator_ShouldFallBackToSync()
        {
            // Arrange
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            var context = "test context";

            // Act
            var result = await rule.EvaluateAsync(context);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(rule.IsAsync);
        }
    }
}
