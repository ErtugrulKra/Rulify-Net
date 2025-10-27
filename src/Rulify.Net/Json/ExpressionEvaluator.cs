using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rulify.Net.Json
{
    /// <summary>
    /// Evaluates dynamic expressions at runtime
    /// </summary>
    public class ExpressionEvaluator
    {
        /// <summary>
        /// Gets the variables available in the expression
        /// </summary>
        public Dictionary<string, object> Variables { get; }

        /// <summary>
        /// Initializes a new instance of the ExpressionEvaluator
        /// </summary>
        /// <param name="variables">The variables available in the expression</param>
        public ExpressionEvaluator(Dictionary<string, object> variables)
        {
            Variables = variables ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Evaluates a boolean expression
        /// </summary>
        public bool EvaluateCondition(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return true;

            return EvaluateBooleanExpression(expression);
        }

        /// <summary>
        /// Evaluates an assignment or computation expression
        /// </summary>
        public object? EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            // Check if it's an assignment
            if (expression.Contains("="))
            {
                var parts = expression.Split('=');
                if (parts.Length != 2)
                    throw new InvalidOperationException($"Invalid assignment expression: {expression}");

                var variableName = parts[0].Trim();
                var valueExpression = parts[1].Trim();
                
                var result = EvaluateArithmeticExpression(valueExpression);
                if (result != null)
                {
                    Variables[variableName] = result;
                }
                return result;
            }

            // Regular computation
            return EvaluateArithmeticExpression(expression);
        }

        private bool EvaluateBooleanExpression(string expression)
        {
            // Handle logical operators
            expression = expression.Trim();

            // Check for 'and' and 'or' operators (case insensitive)
            if (expression.Contains(" and ", StringComparison.OrdinalIgnoreCase) || 
                expression.Contains(" && "))
            {
                var parts = expression.Split(new[] { " and ", " && " }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    parts = expression.Split(new[] { " and ", " && " }, StringSplitOptions.None);
                }
                
                if (parts.Length == 2)
                {
                    return EvaluateBooleanExpression(parts[0].Trim()) && EvaluateBooleanExpression(parts[1].Trim());
                }
            }

            if (expression.Contains(" or ", StringComparison.OrdinalIgnoreCase) || 
                expression.Contains(" || "))
            {
                var parts = expression.Split(new[] { " or ", " || " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    return EvaluateBooleanExpression(parts[0].Trim()) || EvaluateBooleanExpression(parts[1].Trim());
                }
            }

            // Handle comparison operators
            if (expression.Contains(">="))
            {
                var parts = ParseComparison(expression, ">=");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) >= 0;
                }
            }

            if (expression.Contains("<="))
            {
                var parts = ParseComparison(expression, "<=");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) <= 0;
                }
            }

            if (expression.Contains("=="))
            {
                var parts = ParseComparison(expression, "==");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) == 0;
                }
            }

            if (expression.Contains("!="))
            {
                var parts = ParseComparison(expression, "!=");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) != 0;
                }
            }

            if (expression.Contains(">") && !expression.Contains(">="))
            {
                var parts = ParseComparison(expression, ">");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) > 0;
                }
            }

            if (expression.Contains("<") && !expression.Contains("<="))
            {
                var parts = ParseComparison(expression, "<");
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0]);
                    var right = EvaluateArithmeticExpression(parts[1]);
                    return CompareValues(left, right) < 0;
                }
            }

            // Simple boolean value
            if (bool.TryParse(expression, out bool boolValue))
                return boolValue;

            // Try to evaluate as number comparison with 0
            var numericValue = EvaluateArithmeticExpression(expression);
            return ConvertToDecimal(numericValue) != 0;
        }

        private object? EvaluateArithmeticExpression(string expression)
        {
            expression = expression.Trim();

            // Handle parentheses
            if (expression.Contains("("))
            {
                return EvaluateParentheticalExpression(expression);
            }

            // Handle addition and subtraction
            if (expression.Contains("+"))
            {
                var parts = expression.Split('+');
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0].Trim());
                    var right = EvaluateArithmeticExpression(parts[1].Trim());
                    return ConvertToDecimal(left) + ConvertToDecimal(right);
                }
            }

            if (expression.Contains("-") && !expression.StartsWith("-"))
            {
                var index = expression.LastIndexOf('-');
                if (index > 0)
                {
                    var left = EvaluateArithmeticExpression(expression.Substring(0, index).Trim());
                    var right = EvaluateArithmeticExpression(expression.Substring(index + 1).Trim());
                    return ConvertToDecimal(left) - ConvertToDecimal(right);
                }
            }

            // Handle multiplication and division
            if (expression.Contains("*"))
            {
                var parts = expression.Split('*');
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0].Trim());
                    var right = EvaluateArithmeticExpression(parts[1].Trim());
                    return ConvertToDecimal(left) * ConvertToDecimal(right);
                }
            }

            if (expression.Contains("/"))
            {
                var parts = expression.Split('/');
                if (parts.Length == 2)
                {
                    var left = EvaluateArithmeticExpression(parts[0].Trim());
                    var right = EvaluateArithmeticExpression(parts[1].Trim());
                    var rightValue = ConvertToDecimal(right);
                    if (rightValue == 0)
                        throw new DivideByZeroException("Division by zero");
                    return ConvertToDecimal(left) / rightValue;
                }
            }

            // Handle numeric values
            if (decimal.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                return decimalValue;

            if (double.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            if (int.TryParse(expression, out int intValue))
                return intValue;

            // Handle string literals
            if (expression.StartsWith("'") && expression.EndsWith("'"))
                return expression.Substring(1, expression.Length - 2);
            if (expression.StartsWith("\"") && expression.EndsWith("\""))
                return expression.Substring(1, expression.Length - 2);

            // Handle boolean literals
            if (expression.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (expression.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // Handle null
            if (expression.Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            // Handle variable access
            if (Variables.ContainsKey(expression))
                return Variables[expression];

            throw new InvalidOperationException($"Cannot evaluate expression: {expression}");
        }

        private object? EvaluateParentheticalExpression(string expression)
        {
            var startIndex = expression.LastIndexOf('(');
            if (startIndex < 0)
                return EvaluateArithmeticExpression(expression);

            var endIndex = expression.IndexOf(')', startIndex);
            if (endIndex < 0)
                throw new InvalidOperationException($"Unmatched parenthesis in: {expression}");

            var innerExpression = expression.Substring(startIndex + 1, endIndex - startIndex - 1);
            var innerResult = EvaluateArithmeticExpression(innerExpression);

            var newExpression = expression.Substring(0, startIndex) + 
                               ConvertToString(innerResult) + 
                               expression.Substring(endIndex + 1);

            return EvaluateArithmeticExpression(newExpression);
        }

        private string[] ParseComparison(string expression, string op)
        {
            var index = expression.IndexOf(op);
            if (index < 0)
                return new[] { expression };

            return new[]
            {
                expression.Substring(0, index).Trim(),
                expression.Substring(index + op.Length).Trim()
            };
        }

        private int CompareValues(object? left, object? right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            var leftDecimal = ConvertToDecimal(left);
            var rightDecimal = ConvertToDecimal(right);
            return decimal.Compare(leftDecimal, rightDecimal);
        }

        private decimal ConvertToDecimal(object? value)
        {
            if (value == null) return 0;
            if (value is decimal d) return d;
            if (value is double dl) return (decimal)dl;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is float f) return (decimal)f;
            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;
            return 0;
        }

        private string ConvertToString(object? value)
        {
            if (value == null) return "null";
            if (value is bool b) return b ? "true" : "false";
            return value.ToString() ?? "null";
        }
    }
}

