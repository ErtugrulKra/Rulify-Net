using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rulify.Net;

namespace Rulify.Net.Examples
{
    /// <summary>
    /// Example usage of Rulify.Net
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Rulify.Net Example Usage");
            Console.WriteLine("=======================");

            // Basic usage example
            await BasicExample();

            Console.WriteLine();

            // Async rules example
            await AsyncRulesExample();

            Console.WriteLine();

            // Collection evaluation example
            await CollectionEvaluationExample();

            Console.WriteLine();

            // Event monitoring example
            await EventMonitoringExample();
        }

        private static async Task BasicExample()
        {
            Console.WriteLine("1. Basic Usage Example:");
            Console.WriteLine("----------------------");

            // Create a rule engine for string validation
            var engine = new RuleEngine<string>();

            // Add validation rules
            engine.AddRule(new DelegateRule<string>(
                "length-check",
                "Length Check",
                context => context.Length >= 5 ? RuleResult.Success() : RuleResult.Failure("String must be at least 5 characters long"),
                "Validates minimum string length",
                priority: 1
            ));

            engine.AddRule(new DelegateRule<string>(
                "uppercase-check",
                "Uppercase Check",
                context => context.Any(char.IsUpper) ? RuleResult.Success() : RuleResult.Failure("String must contain at least one uppercase letter"),
                "Validates presence of uppercase letters",
                priority: 2
            ));

            // Test with different strings
            var testStrings = new[] { "hello", "Hello", "HELLO WORLD", "hi" };

            foreach (var testString in testStrings)
            {
                Console.WriteLine($"\nTesting: '{testString}'");
                var results = engine.Evaluate(testString);

                foreach (var result in results)
                {
                    var status = result.IsSuccess ? "✓ PASS" : "✗ FAIL";
                    Console.WriteLine($"  {status} - {result.ErrorMessage ?? "Success"}");
                }
            }
        }

        private static async Task AsyncRulesExample()
        {
            Console.WriteLine("2. Async Rules Example:");
            Console.WriteLine("-----------------------");

            var engine = new RuleEngine<string>();

            // Add an async rule that simulates database validation
            engine.AddRule(new DelegateRule<string>(
                "database-check",
                "Database Check",
                context => RuleResult.Success(), // Sync fallback
                async context =>
                {
                    // Simulate async database operation
                    await Task.Delay(100);
                    
                    // Simulate database validation
                    var isValid = context.Length > 3 && !context.Contains("invalid");
                    return isValid 
                        ? RuleResult.Success(new Dictionary<string, object> { { "databaseValidated", true } })
                        : RuleResult.Failure("Database validation failed");
                },
                "Simulates async database validation",
                priority: 1
            ));

            var testString = "validuser";
            Console.WriteLine($"\nTesting async rule with: '{testString}'");
            
            var results = await engine.EvaluateAsync(testString);
            
            foreach (var result in results)
            {
                var status = result.IsSuccess ? "✓ PASS" : "✗ FAIL";
                Console.WriteLine($"  {status} - {result.ErrorMessage ?? "Success"}");
                if (result.Data.ContainsKey("databaseValidated"))
                {
                    Console.WriteLine($"  Database validated: {result.Data["databaseValidated"]}");
                }
            }
        }

        private static async Task CollectionEvaluationExample()
        {
            Console.WriteLine("3. Collection Evaluation Example:");
            Console.WriteLine("---------------------------------");

            var engine = new RuleEngine<int>();

            // Add rules for number validation
            engine.AddRule(new DelegateRule<int>(
                "positive-check",
                "Positive Check",
                context => context > 0 ? RuleResult.Success() : RuleResult.Failure("Number must be positive"),
                "Validates positive numbers",
                priority: 1
            ));

            engine.AddRule(new DelegateRule<int>(
                "range-check",
                "Range Check",
                context => context >= 1 && context <= 100 ? RuleResult.Success() : RuleResult.Failure("Number must be between 1 and 100"),
                "Validates number range",
                priority: 2
            ));

            var numbers = new[] { -5, 0, 25, 50, 150 };

            Console.WriteLine("\nTesting collection of numbers:");
            var results = engine.EvaluateCollection(numbers);

            foreach (var kvp in results)
            {
                Console.WriteLine($"\nNumber: {kvp.Key}");
                foreach (var result in kvp.Value)
                {
                    var status = result.IsSuccess ? "✓ PASS" : "✗ FAIL";
                    Console.WriteLine($"  {status} - {result.ErrorMessage ?? "Success"}");
                }
            }
        }

        private static async Task EventMonitoringExample()
        {
            Console.WriteLine("4. Event Monitoring Example:");
            Console.WriteLine("-----------------------------");

            var engine = new RuleEngine<string>();

            // Subscribe to events
            engine.RuleEvaluating += (sender, args) =>
            {
                Console.WriteLine($"  → Starting evaluation of rule: {args.Rule.Name}");
            };

            engine.RuleEvaluated += (sender, args) =>
            {
                var status = args.Result?.IsSuccess == true ? "SUCCESS" : "FAILED";
                var time = args.Result?.ExecutionTime.TotalMilliseconds ?? 0;
                Console.WriteLine($"  ← Rule '{args.Rule.Name}' completed: {status} (took {time:F2}ms)");
            };

            // Add some rules
            engine.AddRule(new DelegateRule<string>(
                "quick-check",
                "Quick Check",
                context => RuleResult.Success(),
                "A quick validation rule",
                priority: 1
            ));

            engine.AddRule(new DelegateRule<string>(
                "slow-check",
                "Slow Check",
                context =>
                {
                    // Simulate slow operation
                    System.Threading.Thread.Sleep(50);
                    return RuleResult.Success();
                },
                "A slower validation rule",
                priority: 2
            ));

            Console.WriteLine("\nTesting with event monitoring:");
            var results = engine.Evaluate("test");

            Console.WriteLine($"\nTotal rules executed: {results.Count()}");
        }
    }
}
