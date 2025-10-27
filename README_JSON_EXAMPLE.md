# JSON/YAML Rule Import for Rulify.Net

This document explains Rulify.Net's new JSON/YAML rule import feature.

## Features

- ✅ Load rules from JSON format
- ✅ Preserve existing programmatic rule structure
- ✅ Dynamic expression evaluation
- ✅ Conditional (if/then) actions
- ✅ Foreach loops
- ✅ Complex condition structures (all/any)
- ✅ Priority ordering

## Usage

### Basic Usage

```csharp
using Rulify.Net;
using System.Collections.Generic;

// Load rules from JSON
var json = @"{
  ""rules"": [
    {
      ""id"": ""discount_rule"",
      ""name"": ""Discount Rule"",
      ""description"": ""Apply 10% discount for orders over 100"",
      ""conditions"": {
        ""all"": [
          { ""var"": ""total"", "">"": 100 }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""final_total = total * 0.9""
        }
      ]
    }
  ]
}";

// Create engine
var engine = JsonRuleLoader.LoadEngineFromJson(json);

// Create context
var context = new Dictionary<string, object>
{
    ["total"] = 150
};

// Evaluate rules
var results = engine.Evaluate(context);

// Display result
foreach (var result in results)
{
    Console.WriteLine($"Rule: {result.IsSuccess}");
    Console.WriteLine($"Execution time: {result.ExecutionTime.TotalMilliseconds}ms");
}
```

### Loading from File

```csharp
// Load from JSON file
var engine = JsonRuleLoader.LoadEngineFromFile("rules.json");

// Or get just the rules and add them to your own engine
var rules = JsonRuleLoader.LoadFromFile("rules.json");
var myEngine = new RuleEngine<Dictionary<string, object>>();
foreach (var rule in rules)
{
    myEngine.AddRule(rule);
}
```

## JSON Format

### Basic Structure

```json
{
  "rules": [
    {
      "id": "unique_id",
      "name": "Rule Name",
      "description": "Rule description",
      "priority": 1,
      "conditions": { ... },
      "actions": [ ... ],
      "foreach": "property_name"
    }
  ]
}
```

### Conditions

**All (AND Logic):**
```json
"conditions": {
  "all": [
    { "var": "age", ">=": 18 },
    { "var": "balance", ">": 0 }
  ]
}
```

**Any (OR Logic):**
```json
"conditions": {
  "any": [
    { "var": "isVip", "==": true },
    { "var": "specialDiscount", "==": true }
  ]
}
```

### Actions

**Compute:**
```json
"actions": [
  {
    "type": "compute",
    "expression": "total = (price * quantity) + tax"
  }
]
```

**If/Then (Conditional Action):**
```json
"actions": [
  {
    "type": "if",
    "condition": "amount > 1000",
    "then": "discount = 0.15",
    "else": "discount = 0.10"
  }
]
```

### Foreach (Loop)

```json
{
  "id": "process_items",
  "foreach": "items",
  "actions": [
    {
      "type": "if",
      "condition": "item.price > 100",
      "then": "item.discounted_price = item.price * 0.9"
    }
  ]
}
```

## Complete Example: Travel Reservation System

```json
{
  "rules": [
    {
      "id": "hotel_price",
      "description": "Weekday/weekend hotel price difference",
      "conditions": {
        "all": [
          { "var": "stay_days", ">=": 1 }
        ]
      },
      "actions": [
        {
          "type": "compute",
          "expression": "hotel_total = (weekdays * base_rate_weekday) + (weekends * base_rate_weekend)"
        },
        {
          "type": "if",
          "condition": "stay_days >= 7 and start_day == 'Monday'",
          "then": "hotel_total *= 0.9"
        }
      ]
    },
    {
      "id": "child_discounts",
      "description": "Age-based discount for children",
      "foreach": "children",
      "actions": [
        {
          "type": "if",
          "condition": "item <= 4",
          "then": "child_price = 0"
        },
        {
          "type": "if",
          "condition": "item >= 5 and item <= 12",
          "then": "child_price = base_rate_weekday * 0.5"
        },
        {
          "type": "if",
          "condition": "item > 12",
          "then": "child_price = base_rate_weekday"
        }
      ]
    },
    {
      "id": "flight_discount",
      "description": "Bulk purchase flight discount",
      "conditions": {
        "any": [
          { "var": "adults", ">": 0 }
        ]
      },
      "actions": [
        {
          "type": "compute",
          "expression": "flight_total = (adults * flight_price_adult)"
        },
        {
          "type": "if",
          "condition": "adults >= 5",
          "then": "flight_total *= 0.95"
        }
      ]
    }
  ]
}
```

### Usage:

```csharp
var json = File.ReadAllText("travel_rules.json");
var engine = JsonRuleLoader.LoadEngineFromJson(json);

var booking = new Dictionary<string, object>
{
    ["stay_days"] = 7,
    ["weekdays"] = 5,
    ["weekends"] = 2,
    ["base_rate_weekday"] = 100m,
    ["base_rate_weekend"] = 150m,
    ["start_day"] = "Monday",
    ["adults"] = 3,
    ["flight_price_adult"] = 500m,
    ["children"] = new[] { 3, 8, 14 }
};

var results = engine.Evaluate(booking);

// Display results
foreach (var result in results)
{
    Console.WriteLine($"Rule: {result.IsSuccess}");
}

// Final values
Console.WriteLine($"Hotel Total: {booking.GetValueOrDefault("hotel_total")}");
Console.WriteLine($"Flight Total: {booking.GetValueOrDefault("flight_total")}");
```

## Supported Operators

### Comparison Operators
- `>`: Greater than
- `>=`: Greater than or equal
- `<`: Less than
- `<=`: Less than or equal
- `==`: Equal
- `!=`: Not equal

### Arithmetic Operators
- `+`: Addition
- `-`: Subtraction
- `*`: Multiplication
- `/`: Division

### Logical Operators
- `and`: AND (&&)
- `or`: OR (||)

## Compatibility with Programmatic Usage

JSON rules are fully compatible with the existing programmatic rule structure:

```csharp
// Load from JSON
var engine = JsonRuleLoader.LoadEngineFromJson(jsonString);

// Add programmatic rule
engine.AddRule(new DelegateRule<Dictionary<string, object>>(
    "custom_rule",
    "Custom Rule",
    context => RuleResult.Success()
));

// Evaluate both types of rules
var results = engine.Evaluate(context);
```

## Important Notes

1. **Type Safety**: JSON rules are evaluated dynamically, so type safety is not checked at compile-time.

2. **Context**: JSON rules use `Dictionary<string, object>` context.

3. **Expression Evaluator**: Dynamic expressions are evaluated by a simple expression evaluator. For complex mathematical operations, Roslyn or similar library can be used.

4. **Error Management**: Invalid expressions or missing variables may cause rules to fail. Check the results.

## YAML Support

To add YAML support, you'll need to add a YAML parser library (e.g., YamlDotNet). The existing JSON structure can be easily adapted to YAML.

## Tests

The project is tested with 37 tests, with additional tests added for the new JSON loader feature:

```bash
dotnet test
```

## Example Files

- `JsonRulesExample.cs`: Comprehensive JSON usage example
- `JsonRuleLoaderTests.cs`: JSON loader tests
- `README_JSON_EXAMPLE.md`: This document

