using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rulify.Net;

namespace Rulify.Net.Tests
{
    public class RuleEngineTests
    {
        [Fact]
        public void Constructor_ShouldCreateEmptyEngine()
        {
            // Act
            var engine = new RuleEngine<string>();

            // Assert
            Assert.Empty(engine.GetRules());
        }

        [Fact]
        public void AddRule_WithValidRule_ShouldAddRule()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());

            // Act
            engine.AddRule(rule);

            // Assert
            var rules = engine.GetRules().ToList();
            Assert.Single(rules);
            Assert.Equal(rule, rules[0]);
        }

        [Fact]
        public void AddRule_WithNullRule_ShouldThrowArgumentNullException()
        {
            // Arrange
            var engine = new RuleEngine<string>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => engine.AddRule(null!));
        }

        [Fact]
        public void AddRule_WithSameId_ShouldReplaceExistingRule()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule1 = new DelegateRule<string>("test-rule", "Test Rule 1", 
                context => RuleResult.Success());
            var rule2 = new DelegateRule<string>("test-rule", "Test Rule 2", 
                context => RuleResult.Failure("Failed"));

            // Act
            engine.AddRule(rule1);
            engine.AddRule(rule2);

            // Assert
            var rules = engine.GetRules().ToList();
            Assert.Single(rules);
            Assert.Equal("Test Rule 2", rules[0].Name);
        }

        [Fact]
        public void RemoveRule_WithExistingRule_ShouldRemoveRule()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);

            // Act
            var result = engine.RemoveRule("test-rule");

            // Assert
            Assert.True(result);
            Assert.Empty(engine.GetRules());
        }

        [Fact]
        public void RemoveRule_WithNonExistingRule_ShouldReturnFalse()
        {
            // Arrange
            var engine = new RuleEngine<string>();

            // Act
            var result = engine.RemoveRule("non-existing");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RemoveRule_WithNullId_ShouldReturnFalse()
        {
            // Arrange
            var engine = new RuleEngine<string>();

            // Act
            var result = engine.RemoveRule(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetRules_ShouldReturnRulesOrderedByPriority()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule1 = new DelegateRule<string>("rule1", "Rule 1", 
                context => RuleResult.Success(), priority: 1);
            var rule2 = new DelegateRule<string>("rule2", "Rule 2", 
                context => RuleResult.Success(), priority: 3);
            var rule3 = new DelegateRule<string>("rule3", "Rule 3", 
                context => RuleResult.Success(), priority: 2);

            engine.AddRule(rule1);
            engine.AddRule(rule2);
            engine.AddRule(rule3);

            // Act
            var rules = engine.GetRules().ToList();

            // Assert
            Assert.Equal(3, rules.Count);
            Assert.Equal("Rule 2", rules[0].Name); // Priority 3
            Assert.Equal("Rule 3", rules[1].Name); // Priority 2
            Assert.Equal("Rule 1", rules[2].Name); // Priority 1
        }

        [Fact]
        public void Evaluate_WithSingleRule_ShouldReturnSingleResult()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);
            var context = "test context";

            // Act
            var results = engine.Evaluate(context).ToList();

            // Assert
            Assert.Single(results);
            Assert.True(results[0].IsSuccess);
        }

        [Fact]
        public void Evaluate_WithMultipleRules_ShouldReturnMultipleResults()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule1 = new DelegateRule<string>("rule1", "Rule 1", 
                context => RuleResult.Success());
            var rule2 = new DelegateRule<string>("rule2", "Rule 2", 
                context => RuleResult.Failure("Failed"));
            engine.AddRule(rule1);
            engine.AddRule(rule2);
            var context = "test context";

            // Act
            var results = engine.Evaluate(context).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.True(results[0].IsSuccess);
            Assert.False(results[1].IsSuccess);
        }

        [Fact]
        public async Task EvaluateAsync_WithAsyncRules_ShouldHandleAsyncRules()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var asyncEvaluator = new Func<string, Task<RuleResult>>(async context =>
            {
                await Task.Delay(10);
                return RuleResult.Success();
            });
            var rule = new DelegateRule<string>("async-rule", "Async Rule", 
                context => RuleResult.Success(), asyncEvaluator);
            engine.AddRule(rule);
            var context = "test context";

            // Act
            var results = (await engine.EvaluateAsync(context)).ToList();

            // Assert
            Assert.Single(results);
            Assert.True(results[0].IsSuccess);
        }

        [Fact]
        public void EvaluateCollection_WithMultipleContexts_ShouldEvaluateAllContexts()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);
            var contexts = new[] { "context1", "context2", "context3" };

            // Act
            var results = engine.EvaluateCollection(contexts);

            // Assert
            Assert.Equal(3, results.Count);
            foreach (var context in contexts)
            {
                Assert.True(results.ContainsKey(context));
                Assert.Single(results[context]);
                Assert.True(results[context].First().IsSuccess);
            }
        }

        [Fact]
        public async Task EvaluateCollectionAsync_WithMultipleContexts_ShouldEvaluateAllContexts()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);
            var contexts = new[] { "context1", "context2" };

            // Act
            var results = await engine.EvaluateCollectionAsync(contexts);

            // Assert
            Assert.Equal(2, results.Count);
            foreach (var context in contexts)
            {
                Assert.True(results.ContainsKey(context));
                Assert.Single(results[context]);
                Assert.True(results[context].First().IsSuccess);
            }
        }

        [Fact]
        public void EvaluateCollection_WithNullContexts_ShouldThrowArgumentNullException()
        {
            // Arrange
            var engine = new RuleEngine<string>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => engine.EvaluateCollection(null!));
        }

        [Fact]
        public void ClearRules_ShouldRemoveAllRules()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule1 = new DelegateRule<string>("rule1", "Rule 1", 
                context => RuleResult.Success());
            var rule2 = new DelegateRule<string>("rule2", "Rule 2", 
                context => RuleResult.Success());
            engine.AddRule(rule1);
            engine.AddRule(rule2);

            // Act
            engine.ClearRules();

            // Assert
            Assert.Empty(engine.GetRules());
        }

        [Fact]
        public void RuleEvaluating_Event_ShouldFireWhenRuleIsEvaluating()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);
            var context = "test context";
            var eventFired = false;
            IRule<string>? eventRule = null;
            string? eventContext = null;

            engine.RuleEvaluating += (sender, args) =>
            {
                eventFired = true;
                eventRule = args.Rule;
                eventContext = args.Context;
            };

            // Act
            engine.Evaluate(context);

            // Assert
            Assert.True(eventFired);
            Assert.Equal(rule, eventRule);
            Assert.Equal(context, eventContext);
        }

        [Fact]
        public void RuleEvaluated_Event_ShouldFireWhenRuleIsEvaluated()
        {
            // Arrange
            var engine = new RuleEngine<string>();
            var rule = new DelegateRule<string>("test-rule", "Test Rule", 
                context => RuleResult.Success());
            engine.AddRule(rule);
            var context = "test context";
            var eventFired = false;
            IRule<string>? eventRule = null;
            string? eventContext = null;
            RuleResult? eventResult = null;

            engine.RuleEvaluated += (sender, args) =>
            {
                eventFired = true;
                eventRule = args.Rule;
                eventContext = args.Context;
                eventResult = args.Result;
            };

            // Act
            engine.Evaluate(context);

            // Assert
            Assert.True(eventFired);
            Assert.Equal(rule, eventRule);
            Assert.Equal(context, eventContext);
            Assert.NotNull(eventResult);
            Assert.True(eventResult.IsSuccess);
        }
    }
}
