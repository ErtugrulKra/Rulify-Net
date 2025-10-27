namespace Rulify.Net.Json
{
    /// <summary>
    /// Represents a condition in a JSON rule
    /// </summary>
    public class JsonCondition
    {
        /// <summary>
        /// Gets or sets the 'all' conditions (AND logic)
        /// </summary>
        public List<JsonConditionItem>? All { get; set; }

        /// <summary>
        /// Gets or sets the 'any' conditions (OR logic)
        /// </summary>
        public List<JsonConditionItem>? Any { get; set; }
    }
}

