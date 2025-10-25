using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rulify.Net
{
    /// <summary>
    /// Represents a rule engine that can evaluate rules against contexts
    /// </summary>
    /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
    public class RuleEngine<TContext> : IRuleEngine<TContext> where TContext : notnull
    {
        private readonly Dictionary<string, IRule<TContext>> _rules;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Occurs when a rule is about to be evaluated
        /// </summary>
        public event EventHandler<RuleEvaluationEventArgs<TContext>>? RuleEvaluating;

        /// <summary>
        /// Occurs when a rule has been evaluated
        /// </summary>
        public event EventHandler<RuleEvaluationEventArgs<TContext>>? RuleEvaluated;

        /// <summary>
        /// Initializes a new instance of the RuleEngine class
        /// </summary>
        public RuleEngine()
        {
            _rules = new Dictionary<string, IRule<TContext>>();
        }

        /// <summary>
        /// Adds a rule to the engine
        /// </summary>
        /// <param name="rule">The rule to add</param>
        public void AddRule(IRule<TContext> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            lock (_lockObject)
            {
                _rules[rule.Id] = rule;
            }
        }

        /// <summary>
        /// Removes a rule from the engine
        /// </summary>
        /// <param name="ruleId">The ID of the rule to remove</param>
        /// <returns>True if the rule was removed, false otherwise</returns>
        public bool RemoveRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                return false;

            lock (_lockObject)
            {
                return _rules.Remove(ruleId);
            }
        }

        /// <summary>
        /// Gets all rules in the engine
        /// </summary>
        /// <returns>A collection of all rules</returns>
        public IEnumerable<IRule<TContext>> GetRules()
        {
            lock (_lockObject)
            {
                return _rules.Values.OrderByDescending(r => r.Priority).ToList();
            }
        }

        /// <summary>
        /// Evaluates all rules against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A collection of rule results</returns>
        public IEnumerable<RuleResult> Evaluate(TContext context)
        {
            var rules = GetRules();
            var results = new List<RuleResult>();

            foreach (var rule in rules)
            {
                OnRuleEvaluating(rule, context);
                
                var result = rule.Evaluate(context);
                results.Add(result);
                
                OnRuleEvaluated(rule, context, result);
            }

            return results;
        }

        /// <summary>
        /// Evaluates all rules asynchronously against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        public async Task<IEnumerable<RuleResult>> EvaluateAsync(TContext context)
        {
            var rules = GetRules();
            var results = new List<RuleResult>();

            foreach (var rule in rules)
            {
                OnRuleEvaluating(rule, context);
                
                RuleResult result;
                if (rule.IsAsync)
                {
                    result = await rule.EvaluateAsync(context);
                }
                else
                {
                    result = rule.Evaluate(context);
                }
                
                results.Add(result);
                OnRuleEvaluated(rule, context, result);
            }

            return results;
        }

        /// <summary>
        /// Evaluates rules against a collection of contexts
        /// </summary>
        /// <param name="contexts">The contexts to evaluate against</param>
        /// <returns>A dictionary mapping contexts to their rule results</returns>
        public Dictionary<TContext, IEnumerable<RuleResult>> EvaluateCollection(IEnumerable<TContext> contexts)
        {
            if (contexts == null)
                throw new ArgumentNullException(nameof(contexts));

            var results = new Dictionary<TContext, IEnumerable<RuleResult>>();

            foreach (var context in contexts)
            {
                results[context] = Evaluate(context);
            }

            return results;
        }

        /// <summary>
        /// Evaluates rules asynchronously against a collection of contexts
        /// </summary>
        /// <param name="contexts">The contexts to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        public async Task<Dictionary<TContext, IEnumerable<RuleResult>>> EvaluateCollectionAsync(IEnumerable<TContext> contexts)
        {
            if (contexts == null)
                throw new ArgumentNullException(nameof(contexts));

            var results = new Dictionary<TContext, IEnumerable<RuleResult>>();

            foreach (var context in contexts)
            {
                results[context] = await EvaluateAsync(context);
            }

            return results;
        }

        /// <summary>
        /// Clears all rules from the engine
        /// </summary>
        public void ClearRules()
        {
            lock (_lockObject)
            {
                _rules.Clear();
            }
        }

        /// <summary>
        /// Raises the RuleEvaluating event
        /// </summary>
        /// <param name="rule">The rule being evaluated</param>
        /// <param name="context">The context being evaluated against</param>
        protected virtual void OnRuleEvaluating(IRule<TContext> rule, TContext context)
        {
            RuleEvaluating?.Invoke(this, new RuleEvaluationEventArgs<TContext>(rule, context));
        }

        /// <summary>
        /// Raises the RuleEvaluated event
        /// </summary>
        /// <param name="rule">The rule that was evaluated</param>
        /// <param name="context">The context that was evaluated against</param>
        /// <param name="result">The result of the evaluation</param>
        protected virtual void OnRuleEvaluated(IRule<TContext> rule, TContext context, RuleResult result)
        {
            RuleEvaluated?.Invoke(this, new RuleEvaluationEventArgs<TContext>(rule, context, result));
        }
    }
}
