using System;

namespace Rulify.Net
{
    /// <summary>
    /// Provides data for rule evaluation events
    /// </summary>
    /// <typeparam name="TContext">The type of context being evaluated</typeparam>
    public class RuleEvaluationEventArgs<TContext> : EventArgs
    {
        /// <summary>
        /// Gets the rule being evaluated
        /// </summary>
        public IRule<TContext> Rule { get; }

        /// <summary>
        /// Gets the context being evaluated against
        /// </summary>
        public TContext Context { get; }

        /// <summary>
        /// Gets the result of the evaluation (null if evaluation hasn't completed yet)
        /// </summary>
        public RuleResult? Result { get; }

        /// <summary>
        /// Gets the timestamp when the evaluation started
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationEventArgs class
        /// </summary>
        /// <param name="rule">The rule being evaluated</param>
        /// <param name="context">The context being evaluated against</param>
        /// <param name="result">The result of the evaluation (optional)</param>
        public RuleEvaluationEventArgs(IRule<TContext> rule, TContext context, RuleResult? result = null)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Result = result;
            Timestamp = DateTime.UtcNow;
        }
    }
}
