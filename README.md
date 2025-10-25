# Rulify.Net

A powerful and flexible rule engine for .NET applications. Rulify.Net provides a comprehensive solution for implementing business rules, validation logic, and decision-making processes in your applications.

[![NuGet](https://img.shields.io/nuget/v/Rulify.Net.svg)](https://www.nuget.org/packages/Rulify.Net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-7.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/7.0)

## Features

- **Dynamic Rule Definition**: Create rules using lambda expressions and delegates
- **Priority-Based Execution**: Execute rules in order of priority
- **Async Support**: Full support for asynchronous rule evaluation
- **Collection Evaluation**: Evaluate rules against collections of data
- **Event Hooks**: Monitor rule execution with events
- **Thread-Safe**: Safe for concurrent operations
- **Dependency Injection Ready**: Easy integration with DI containers

## Installation

```bash
dotnet add package Rulify.Net
```

## Quick Start

### Basic Usage

```csharp
using Rulify.Net;

// Create a rule engine
var engine = new RuleEngine<string>();

// Add a rule
engine.AddRule(new DelegateRule<string>(
    "length-check", 
    "Length Check", 
    context => context.Length > 5 ? RuleResult.Success() : RuleResult.Failure("Too short")
));

// Evaluate a context
var results = engine.Evaluate("Hello World");
foreach (var result in results)
{
    Console.WriteLine($"Rule: {result.IsSuccess}, Message: {result.ErrorMessage}");
}
```

### Async Rules

```csharp
var asyncRule = new DelegateRule<string>(
    "async-check",
    "Async Check",
    context => RuleResult.Success(), // Sync fallback
    async context => 
    {
        await Task.Delay(100); // Simulate async work
        return RuleResult.Success();
    }
);

engine.AddRule(asyncRule);
var results = await engine.EvaluateAsync("test");
```

### Collection Evaluation

```csharp
var contexts = new[] { "short", "medium length", "very long context" };
var results = engine.EvaluateCollection(contexts);

foreach (var kvp in results)
{
    Console.WriteLine($"Context: {kvp.Key}");
    foreach (var result in kvp.Value)
    {
        Console.WriteLine($"  Result: {result.IsSuccess}");
    }
}
```

### Event Monitoring

```csharp
engine.RuleEvaluating += (sender, args) =>
{
    Console.WriteLine($"Evaluating rule: {args.Rule.Name}");
};

engine.RuleEvaluated += (sender, args) =>
{
    Console.WriteLine($"Rule {args.Rule.Name} completed: {args.Result?.IsSuccess}");
};
```

## API Reference

### IRuleEngine<TContext>

Main interface for the rule engine.

- `AddRule(IRule<TContext> rule)`: Add a rule to the engine
- `RemoveRule(string ruleId)`: Remove a rule by ID
- `GetRules()`: Get all rules ordered by priority
- `Evaluate(TContext context)`: Evaluate all rules against a context
- `EvaluateAsync(TContext context)`: Evaluate all rules asynchronously
- `EvaluateCollection(IEnumerable<TContext> contexts)`: Evaluate against multiple contexts
- `ClearRules()`: Remove all rules

### IRule<TContext>

Interface for individual rules.

- `Id`: Unique identifier
- `Name`: Human-readable name
- `Description`: Rule description
- `Priority`: Execution priority (higher = first)
- `IsAsync`: Whether the rule supports async evaluation
- `Evaluate(TContext context)`: Synchronous evaluation
- `EvaluateAsync(TContext context)`: Asynchronous evaluation

### RuleResult

Result of rule evaluation.

- `IsSuccess`: Whether the rule passed
- `ErrorMessage`: Error message if failed
- `Data`: Additional data returned by the rule
- `ExecutionTime`: Time taken to execute the rule

## Advanced Usage

### Custom Rule Implementation

```csharp
public class CustomRule : RuleBase<MyContext>
{
    public CustomRule() : base("custom-id", "Custom Rule", "Description", priority: 10)
    {
    }

    public override RuleResult Evaluate(MyContext context)
    {
        // Custom logic here
        return context.IsValid ? RuleResult.Success() : RuleResult.Failure("Invalid context");
    }
}
```

### Priority-Based Execution

Rules are executed in order of priority (highest first). Rules with the same priority are executed in the order they were added.

```csharp
engine.AddRule(new DelegateRule<string>("rule1", "Rule 1", context => RuleResult.Success(), priority: 1));
engine.AddRule(new DelegateRule<string>("rule2", "Rule 2", context => RuleResult.Success(), priority: 10));
// Rule 2 will execute before Rule 1
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

**Ertugrul Kara**
- GitHub: [@ErtugrulKra](https://github.com/ErtugrulKra)
- Website: [ertugrulkara.com](https://www.ertugrulkara.com)
- Email: ertugrulkra@gmail.com

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
