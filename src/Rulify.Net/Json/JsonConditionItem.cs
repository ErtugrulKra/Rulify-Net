namespace Rulify.Net.Json
{
    /// <summary>
    /// Represents a single condition item
    /// </summary>
    public class JsonConditionItem
    {
        /// <summary>
        /// Gets or sets the variable name
        /// </summary>
        public string? Var { get; set; }

        /// <summary>
        /// Gets or sets the comparison operation
        /// </summary>
        public string? Op { get; set; }

        /// <summary>
        /// Gets or sets the comparison value
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets additional dynamic properties for comparison operators
        /// </summary>
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }
}

