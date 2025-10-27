using System;
using System.Collections.Generic;
using Xunit;
using Rulify.Net.Json;

namespace Rulify.Net.Tests
{
    public class JsonRuleLoaderTests
    {
        [Fact]
        public void LoadFromJson_ValidJson_ReturnsRules()
        {
            // Arrange
            var json = @"{
              ""rules"": [
                {
                  ""id"": ""test_rule"",
                  ""name"": ""Test Rule"",
                  ""description"": ""A test rule"",
                  ""priority"": 1,
                  ""conditions"": {
                    ""all"": [
                      { ""var"": ""test_var"", "">="": 1 }
                    ]
                  },
                  ""actions"": [
                    {
                      ""type"": ""compute"",
                      ""expression"": ""result = test_var * 2""
                    }
                  ]
                }
              ]
            }";

            // Act
            var rules = JsonRuleLoader.LoadFromJson(json);
            var ruleList = new List<IRule<Dictionary<string, object>>>(rules);

            // Assert
            Assert.Single(ruleList);
            Assert.Equal("test_rule", ruleList[0].Id);
            Assert.Equal("Test Rule", ruleList[0].Name);
            Assert.Equal("A test rule", ruleList[0].Description);
            Assert.Equal(1, ruleList[0].Priority);
        }

        [Fact]
        public void LoadFromJson_MultipleRules_ReturnsAllRules()
        {
            // Arrange
            var json = @"{
              ""rules"": [
                {
                  ""id"": ""rule1"",
                  ""name"": ""Rule 1"",
                  ""description"": ""First rule"",
                  ""priority"": 1
                },
                {
                  ""id"": ""rule2"",
                  ""name"": ""Rule 2"",
                  ""description"": ""Second rule"",
                  ""priority"": 2
                }
              ]
            }";

            // Act
            var rules = JsonRuleLoader.LoadFromJson(json);
            var ruleList = new List<IRule<Dictionary<string, object>>>(rules);

            // Assert
            Assert.Equal(2, ruleList.Count);
        }

        [Fact]
        public void LoadEngineFromJson_ValidJson_CreatesEngine()
        {
            // Arrange
            var json = @"{
              ""rules"": [
                {
                  ""id"": ""hotel_price"",
                  ""name"": ""Hotel Price"",
                  ""description"": ""Calculate hotel price"",
                  ""priority"": 1,
                  ""conditions"": {
                    ""all"": [
                      { ""var"": ""stay_days"", "">="": 1 }
                    ]
                  },
                  ""actions"": [
                    {
                      ""type"": ""compute"",
                      ""expression"": ""hotel_total = weekdays * base_rate_weekday""
                    }
                  ]
                }
              ]
            }";

            // Act
            var engine = JsonRuleLoader.LoadEngineFromJson(json);
            var context = new Dictionary<string, object>
            {
                ["stay_days"] = 7,
                ["weekdays"] = 5,
                ["base_rate_weekday"] = 100m
            };

            var results = engine.Evaluate(context);

            // Assert
            Assert.NotNull(engine);
            Assert.Single(engine.GetRules());
        }

        [Fact]
        public void LoadFromJson_EmptyRulesArray_ReturnsEmpty()
        {
            // Arrange
            var json = @"{ ""rules"": [] }";

            // Act
            var rules = JsonRuleLoader.LoadFromJson(json);
            var ruleList = new List<IRule<Dictionary<string, object>>>(rules);

            // Assert
            Assert.Empty(ruleList);
        }

        [Fact]
        public void LoadFromJson_InvalidJson_ThrowsException()
        {
            // Arrange
            var json = "{ invalid json }";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => JsonRuleLoader.LoadFromJson(json));
        }

        [Fact]
        public void ExpressionEvaluator_SimpleArithmetic_EvaluatesCorrectly()
        {
            // Arrange
            var variables = new Dictionary<string, object>
            {
                ["x"] = 10,
                ["y"] = 5
            };
            var evaluator = new ExpressionEvaluator(variables);

            // Act
            var result = evaluator.EvaluateExpression("z = x + y");

            // Assert
            Assert.Equal(15m, result);
            Assert.Equal(15m, variables["z"]);
        }

        [Fact]
        public void ExpressionEvaluator_Conditional_WorksCorrectly()
        {
            // Arrange
            var variables = new Dictionary<string, object>
            {
                ["value"] = 10
            };
            var evaluator = new ExpressionEvaluator(variables);

            // Act
            var result = evaluator.EvaluateCondition("value > 5");

            // Assert
            Assert.True(result);
        }
    }
}

