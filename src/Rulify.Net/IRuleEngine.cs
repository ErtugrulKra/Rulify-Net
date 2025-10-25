using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rulify.Net
{
    /// <summary>
    /// Represents a rule engine that can evaluate rules against contexts
    /// </summary>
    /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
    public interface IRuleEngine<TContext> where TContext : notnull
    {
        /// <summary>
        /// Adds a rule to the engine
        /// </summary>
        /// <param name="rule">The rule to add</param>
        void AddRule(IRule<TContext> rule);

        /// <summary>
        /// Removes a rule from the engine
        /// </summary>
        /// <param name="ruleId">The ID of the rule to remove</param>
        /// <returns>True if the rule was removed, false otherwise</returns>
        bool RemoveRule(string ruleId);

        /// <summary>
        /// Gets all rules in the engine
        /// </summary>
        /// <returns>A collection of all rules</returns>
        IEnumerable<IRule<TContext>> GetRules();

        /// <summary>
        /// Evaluates all rules against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A collection of rule results</returns>
        IEnumerable<RuleResult> Evaluate(TContext context);

        /// <summary>
        /// Evaluates all rules asynchronously against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        Task<IEnumerable<RuleResult>> EvaluateAsync(TContext context);

        /// <summary>
        /// Evaluates rules against a collection of contexts
        /// </summary>
        /// <param name="contexts">The contexts to evaluate against</param>
        /// <returns>A dictionary mapping contexts to their rule results</returns>
        Dictionary<TContext, IEnumerable<RuleResult>> EvaluateCollection(IEnumerable<TContext> contexts);

        /// <summary>
        /// Evaluates rules asynchronously against a collection of contexts
        /// </summary>
        /// <param name="contexts">The contexts to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        Task<Dictionary<TContext, IEnumerable<RuleResult>>> EvaluateCollectionAsync(IEnumerable<TContext> contexts);

        /// <summary>
        /// Clears all rules from the engine
        /// </summary>
        void ClearRules();
    }
}
