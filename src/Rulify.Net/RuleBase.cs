using System;
using System.Threading.Tasks;

namespace Rulify.Net
{
    /// <summary>
    /// Represents a base rule implementation
    /// </summary>
    /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
    public abstract class RuleBase<TContext> : IRule<TContext>
    {
        /// <summary>
        /// Gets the unique identifier of the rule
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the rule
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the priority of the rule (higher number = higher priority)
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Gets whether this rule is asynchronous
        /// </summary>
        public virtual bool IsAsync => false;

        /// <summary>
        /// Initializes a new instance of the RuleBase class
        /// </summary>
        /// <param name="id">The unique identifier of the rule</param>
        /// <param name="name">The name of the rule</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="priority">The priority of the rule</param>
        protected RuleBase(string id, string name, string description = "", int priority = 0)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? "";
            Priority = priority;
        }

        /// <summary>
        /// Evaluates the rule against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>The result of the rule evaluation</returns>
        public abstract RuleResult Evaluate(TContext context);

        /// <summary>
        /// Evaluates the rule asynchronously against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        public virtual async Task<RuleResult> EvaluateAsync(TContext context)
        {
            // Default implementation runs the synchronous version
            return await Task.Run(() => Evaluate(context));
        }
    }
}
