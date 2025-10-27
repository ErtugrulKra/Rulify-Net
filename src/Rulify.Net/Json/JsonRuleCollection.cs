namespace Rulify.Net.Json
{
    /// <summary>
    /// Represents the root JSON structure containing rules
    /// </summary>
    public class JsonRuleCollection
    {
        /// <summary>
        /// Gets or sets the collection of rules
        /// </summary>
        public List<JsonRuleDefinition>? Rules { get; set; }
    }
}

