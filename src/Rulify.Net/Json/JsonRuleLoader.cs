using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Rulify.Net;

namespace Rulify.Net.Json
{
    /// <summary>
    /// Loads and parses JSON rules into rule engine instances
    /// </summary>
    public class JsonRuleLoader
    {
        /// <summary>
        /// Loads rules from a JSON string
        /// </summary>
        /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
        /// <param name="json">The JSON string containing rules</param>
        /// <returns>A collection of IRule instances</returns>
        public static IEnumerable<IRule<Dictionary<string, object>>> LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("rules", out var rulesElement))
                return Enumerable.Empty<IRule<Dictionary<string, object>>>();

            var rules = new List<IRule<Dictionary<string, object>>>();

            foreach (var ruleElement in rulesElement.EnumerateArray())
            {
                var rule = ParseRuleDefinition(ruleElement);
                rules.Add(CreateRuleFromDefinition(rule));
            }

            return rules;
        }

        /// <summary>
        /// Loads rules from a JSON file
        /// </summary>
        /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
        /// <param name="filePath">The path to the JSON file</param>
        /// <returns>A collection of IRule instances</returns>
        public static IEnumerable<IRule<Dictionary<string, object>>> LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"JSON file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }

        /// <summary>
        /// Loads rules into a rule engine from a JSON string
        /// </summary>
        /// <param name="json">The JSON string containing rules</param>
        /// <returns>A configured RuleEngine instance</returns>
        public static RuleEngine<Dictionary<string, object>> LoadEngineFromJson(string json)
        {
            var engine = new RuleEngine<Dictionary<string, object>>();
            var rules = LoadFromJson(json);

            foreach (var rule in rules)
            {
                engine.AddRule(rule);
            }

            return engine;
        }

        /// <summary>
        /// Loads rules into a rule engine from a JSON file
        /// </summary>
        /// <param name="filePath">The path to the JSON file</param>
        /// <returns>A configured RuleEngine instance</returns>
        public static RuleEngine<Dictionary<string, object>> LoadEngineFromFile(string filePath)
        {
            var engine = new RuleEngine<Dictionary<string, object>>();
            var rules = LoadFromFile(filePath);

            foreach (var rule in rules)
            {
                engine.AddRule(rule);
            }

            return engine;
        }

        private static JsonRuleDefinition ParseRuleDefinition(JsonElement element)
        {
            var definition = new JsonRuleDefinition
            {
                Id = element.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                Description = element.TryGetProperty("description", out var descProp) ? descProp.GetString() : null,
                Priority = element.TryGetProperty("priority", out var priorityProp) ? priorityProp.GetInt32() : 0,
                Foreach = element.TryGetProperty("foreach", out var foreachProp) ? foreachProp.GetString() : null
            };

            // Parse conditions
            if (element.TryGetProperty("conditions", out var conditionsElement))
            {
                definition.Conditions = new JsonCondition();

                if (conditionsElement.TryGetProperty("all", out var allElement))
                {
                    definition.Conditions.All = allElement.EnumerateArray()
                        .Select(ParseConditionItem)
                        .ToList();
                }

                if (conditionsElement.TryGetProperty("any", out var anyElement))
                {
                    definition.Conditions.Any = anyElement.EnumerateArray()
                        .Select(ParseConditionItem)
                        .ToList();
                }
            }

            // Parse actions
            if (element.TryGetProperty("actions", out var actionsElement))
            {
                definition.Actions = actionsElement.EnumerateArray()
                    .Select(ParseAction)
                    .ToList();
            }

            return definition;
        }

        private static JsonConditionItem ParseConditionItem(JsonElement element)
        {
            var item = new JsonConditionItem();

            if (element.TryGetProperty("var", out var varProp))
                item.Var = varProp.GetString();

            if (element.TryGetProperty("op", out var opProp))
                item.Op = opProp.GetString();

            if (element.TryGetProperty("value", out var valueProp))
                item.Value = valueProp.GetString();

            // Parse comparison operators as property keys (">", ">=", "<", "<=", "==", "!=")
            item.AdditionalProperties = new Dictionary<string, object>();
            
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == "var" || property.Name == "op" || property.Name == "value")
                    continue;

                // Check if it's a comparison operator
                if (property.Name.Contains(">") || property.Name.Contains("<") || 
                    property.Name == "==" || property.Name == "!=" || property.Name.Contains("="))
                {
                    item.Op = item.Op ?? property.Name;
                    item.Value = item.Value ?? GetValueFromJsonElement(property.Value);
                }
            }

            return item;
        }

        private static JsonAction ParseAction(JsonElement element)
        {
            var action = new JsonAction();

            if (element.TryGetProperty("type", out var typeProp))
                action.Type = typeProp.GetString();

            if (element.TryGetProperty("expression", out var exprProp))
                action.Expression = exprProp.GetString();

            if (element.TryGetProperty("condition", out var condProp))
                action.Condition = condProp.GetString();

            if (element.TryGetProperty("then", out var thenProp))
                action.Then = thenProp.GetString();

            if (element.TryGetProperty("else", out var elseProp))
                action.Else = elseProp.GetString();

            return action;
        }

        private static object? GetValueFromJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }

        private static IRule<Dictionary<string, object>> CreateRuleFromDefinition(JsonRuleDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            var id = definition.Id ?? Guid.NewGuid().ToString();
            var name = definition.Name ?? id;
            var description = definition.Description ?? string.Empty;
            var priority = definition.Priority;

            // If there's a foreach configuration, create a specialized rule
            if (!string.IsNullOrWhiteSpace(definition.Foreach))
            {
                return CreateForeachRule(definition);
            }

            // Create a standard rule with conditions and actions
            return new DelegateRule<Dictionary<string, object>>(
                id,
                name,
                context => EvaluateRule(context, definition),
                description,
                priority
            );
        }

        private static IRule<Dictionary<string, object>> CreateForeachRule(JsonRuleDefinition definition)
        {
            var foreachProperty = definition.Foreach!;

            return new DelegateRule<Dictionary<string, object>>(
                definition.Id ?? Guid.NewGuid().ToString(),
                definition.Name ?? definition.Id ?? "ForeachRule",
                context =>
                {
                    var evaluator = new ExpressionEvaluator(context);
                    var results = new List<RuleResult>();

                    // Check if the foreach property exists in context
                    if (!context.ContainsKey(foreachProperty))
                        return RuleResult.Success(new Dictionary<string, object> { { "skipped", true } });

                    var collection = context[foreachProperty];
                    
                    if (collection is not System.Collections.IEnumerable enumerable)
                        return RuleResult.Success(new Dictionary<string, object> { { "skipped", true } });

                    int index = 0;
                    foreach (var item in enumerable)
                    {
                        // Create a sub-context with the current item
                        var itemContext = new Dictionary<string, object>(context)
                        {
                            ["item"] = item,
                            ["index"] = index
                        };

                        // Execute actions for each item
                        if (definition.Actions != null)
                        {
                            try
                            {
                                var itemEvaluator = new ExpressionEvaluator(itemContext);
                                ExecuteActions(itemEvaluator, definition.Actions);
                                results.Add(RuleResult.Success(new Dictionary<string, object> { { "item", item } }));
                            }
                            catch (Exception ex)
                            {
                                results.Add(RuleResult.Failure($"Foreach iteration failed: {ex.Message}"));
                            }
                        }

                        index++;
                    }

                    return RuleResult.Success(new Dictionary<string, object> { { "iterations", results.Count } });
                },
                definition.Description ?? string.Empty,
                definition.Priority
            );
        }

        private static RuleResult EvaluateRule(Dictionary<string, object> context, JsonRuleDefinition definition)
        {
            try
            {
                var evaluator = new ExpressionEvaluator(context);

                // Check conditions
                if (definition.Conditions != null)
                {
                    if (!EvaluateConditions(evaluator, definition.Conditions))
                    {
                        return RuleResult.Success(new Dictionary<string, object> { { "skipped", true } });
                    }
                }

                // Execute actions
                if (definition.Actions != null)
                {
                    ExecuteActions(evaluator, definition.Actions);
                }

                return RuleResult.Success();
            }
            catch (Exception ex)
            {
                return RuleResult.Failure($"Rule evaluation failed: {ex.Message}");
            }
        }

        private static bool EvaluateConditions(ExpressionEvaluator evaluator, JsonCondition conditions)
        {
            if (conditions.All != null && conditions.All.Any())
            {
                foreach (var condition in conditions.All)
                {
                    if (!EvaluateSingleCondition(evaluator, condition))
                        return false;
                }
            }

            if (conditions.Any != null && conditions.Any.Any())
            {
                var anySuccess = false;
                foreach (var condition in conditions.Any)
                {
                    if (EvaluateSingleCondition(evaluator, condition))
                    {
                        anySuccess = true;
                        break;
                    }
                }
                if (!anySuccess)
                    return false;
            }

            return true;
        }

        private static bool EvaluateSingleCondition(ExpressionEvaluator evaluator, JsonConditionItem item)
        {
            if (item.Var == null)
                return false;

            // Try to get the comparison operator and value from dynamic properties
            // JSON deserializer may place comparison operators like ">" as properties
            var varName = item.Var;
            var varValue = evaluator.Variables.ContainsKey(varName) ? evaluator.Variables[varName] : null;

            // Check various comparison operators
            var comparisonValue = item.Value ?? GetComparisonValueFromProperties(item);
            string? op = item.Op ?? GetComparisonOperatorFromProperties(item);

            if (op != null)
            {
                return EvaluateComparison(varValue, op, comparisonValue);
            }

            return false;
        }

        private static object? GetComparisonValueFromProperties(JsonConditionItem item)
        {
            // Check common comparison operator properties
            if (item.AdditionalProperties != null)
            {
                foreach (var kvp in item.AdditionalProperties)
                {
                    if (kvp.Key.StartsWith(">") || kvp.Key.StartsWith("<") || 
                        kvp.Key == "==" || kvp.Key == "!=" || kvp.Key.Contains("="))
                    {
                        return kvp.Value;
                    }
                }
            }
            return null;
        }

        private static string? GetComparisonOperatorFromProperties(JsonConditionItem item)
        {
            if (item.AdditionalProperties != null)
            {
                foreach (var kvp in item.AdditionalProperties)
                {
                    if (kvp.Key.StartsWith(">") || kvp.Key.StartsWith("<") || 
                        kvp.Key == "==" || kvp.Key == "!=")
                    {
                        return kvp.Key;
                    }
                }
            }
            return null;
        }

        private static bool EvaluateComparison(object? varValue, string op, object? comparisonValue)
        {
            return op switch
            {
                ">" => CompareValues(varValue, comparisonValue) > 0,
                ">=" => CompareValues(varValue, comparisonValue) >= 0,
                "<" => CompareValues(varValue, comparisonValue) < 0,
                "<=" => CompareValues(varValue, comparisonValue) <= 0,
                "==" => CompareValues(varValue, comparisonValue) == 0,
                "!=" => CompareValues(varValue, comparisonValue) != 0,
                _ => false
            };
        }

        private static bool EvaluateComparison(object? varValue, JsonConditionItem item, object? comparisonValue)
        {
            // Try to get operator from reflection
            var properties = typeof(JsonConditionItem).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                if (value != null && !string.IsNullOrEmpty(value.ToString()) && prop.Name != "Var" && prop.Name != "Op")
                {
                    var op = prop.Name.TrimStart('_');
                    return EvaluateComparison(varValue, op, comparisonValue);
                }
            }

            return false;
        }

        private static int CompareValues(object? left, object? right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            if (left is IComparable leftComp && right is IComparable rightComp)
            {
                try
                {
                    return leftComp.CompareTo(right);
                }
                catch
                {
                    // If comparison fails, convert to string
                    return left.ToString()?.CompareTo(right.ToString()) ?? 0;
                }
            }

            return left.ToString()?.CompareTo(right.ToString()) ?? 0;
        }

        private static void ExecuteActions(ExpressionEvaluator evaluator, List<JsonAction> actions)
        {
            if (actions == null || !actions.Any())
                return;

            foreach (var action in actions)
            {
                if (action.Type == "compute" && !string.IsNullOrWhiteSpace(action.Expression))
                {
                    evaluator.EvaluateExpression(action.Expression);
                }
                else if (action.Type == "if" && !string.IsNullOrWhiteSpace(action.Condition))
                {
                    if (evaluator.EvaluateCondition(action.Condition))
                    {
                        if (!string.IsNullOrWhiteSpace(action.Then))
                        {
                            evaluator.EvaluateExpression(action.Then);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(action.Else))
                    {
                        evaluator.EvaluateExpression(action.Else);
                    }
                }
            }
        }
    }
}

