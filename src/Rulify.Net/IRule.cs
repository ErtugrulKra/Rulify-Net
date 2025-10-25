using System;
using System.Threading.Tasks;

namespace Rulify.Net
{
    /// <summary>
    /// Represents a rule that can be evaluated against a context
    /// </summary>
    /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
    public interface IRule<TContext>
    {
        /// <summary>
        /// Gets the unique identifier of the rule
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the rule
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the priority of the rule (higher number = higher priority)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets whether this rule is asynchronous
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Evaluates the rule against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>The result of the rule evaluation</returns>
        RuleResult Evaluate(TContext context);

        /// <summary>
        /// Evaluates the rule asynchronously against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        Task<RuleResult> EvaluateAsync(TContext context);
    }
}
