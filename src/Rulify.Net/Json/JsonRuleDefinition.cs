namespace Rulify.Net.Json
{
    /// <summary>
    /// Represents a rule definition loaded from JSON/YAML
    /// </summary>
    public class JsonRuleDefinition
    {
        /// <summary>
        /// Gets or sets the unique identifier of the rule
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the rule
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the priority of the rule
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the condition structure
        /// </summary>
        public JsonCondition? Conditions { get; set; }

        /// <summary>
        /// Gets or sets the actions to execute
        /// </summary>
        public List<JsonAction>? Actions { get; set; }

        /// <summary>
        /// Gets or sets the foreach configuration for iterating over collections
        /// </summary>
        public string? Foreach { get; set; }
    }
}

