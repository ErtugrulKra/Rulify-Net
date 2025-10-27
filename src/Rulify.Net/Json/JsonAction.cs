namespace Rulify.Net.Json
{
    /// <summary>
    /// Represents an action in a JSON rule
    /// </summary>
    public class JsonAction
    {
        /// <summary>
        /// Gets or sets the type of action (compute, if, etc.)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the expression to execute
        /// </summary>
        public string? Expression { get; set; }

        /// <summary>
        /// Gets or sets the condition for conditional actions
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Gets or sets the action to execute when condition is true
        /// </summary>
        public string? Then { get; set; }

        /// <summary>
        /// Gets or sets the action to execute when condition is false
        /// </summary>
        public string? Else { get; set; }
    }
}

