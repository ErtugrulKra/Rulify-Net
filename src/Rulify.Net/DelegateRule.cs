using System;
using System.Threading.Tasks;

namespace Rulify.Net
{
    /// <summary>
    /// Represents a rule that uses a delegate for evaluation
    /// </summary>
    /// <typeparam name="TContext">The type of context to evaluate against</typeparam>
    public class DelegateRule<TContext> : RuleBase<TContext>
    {
        private readonly Func<TContext, RuleResult> _evaluator;
        private readonly Func<TContext, Task<RuleResult>>? _asyncEvaluator;

        /// <summary>
        /// Gets whether this rule is asynchronous
        /// </summary>
        public override bool IsAsync => _asyncEvaluator != null;

        /// <summary>
        /// Initializes a new instance of the DelegateRule class
        /// </summary>
        /// <param name="id">The unique identifier of the rule</param>
        /// <param name="name">The name of the rule</param>
        /// <param name="evaluator">The delegate that evaluates the rule</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="priority">The priority of the rule</param>
        public DelegateRule(string id, string name, Func<TContext, RuleResult> evaluator, string description = "", int priority = 0)
            : base(id, name, description, priority)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        /// <summary>
        /// Initializes a new instance of the DelegateRule class with async support
        /// </summary>
        /// <param name="id">The unique identifier of the rule</param>
        /// <param name="name">The name of the rule</param>
        /// <param name="evaluator">The delegate that evaluates the rule synchronously</param>
        /// <param name="asyncEvaluator">The delegate that evaluates the rule asynchronously</param>
        /// <param name="description">The description of the rule</param>
        /// <param name="priority">The priority of the rule</param>
        public DelegateRule(string id, string name, Func<TContext, RuleResult> evaluator, Func<TContext, Task<RuleResult>> asyncEvaluator, string description = "", int priority = 0)
            : base(id, name, description, priority)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _asyncEvaluator = asyncEvaluator ?? throw new ArgumentNullException(nameof(asyncEvaluator));
        }

        /// <summary>
        /// Evaluates the rule against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>The result of the rule evaluation</returns>
        public override RuleResult Evaluate(TContext context)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                var result = _evaluator(context);
                var executionTime = DateTime.UtcNow - startTime;
                
                // Update execution time in the result
                return new RuleResult(result.IsSuccess, result.ErrorMessage, executionTime, result.Data);
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                return RuleResult.Failure($"Rule evaluation failed: {ex.Message}", executionTime);
            }
        }

        /// <summary>
        /// Evaluates the rule asynchronously against the provided context
        /// </summary>
        /// <param name="context">The context to evaluate against</param>
        /// <returns>A task that represents the asynchronous evaluation</returns>
        public override async Task<RuleResult> EvaluateAsync(TContext context)
        {
            if (_asyncEvaluator != null)
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    var result = await _asyncEvaluator(context);
                    var executionTime = DateTime.UtcNow - startTime;
                    
                    // Update execution time in the result
                    return new RuleResult(result.IsSuccess, result.ErrorMessage, executionTime, result.Data);
                }
                catch (Exception ex)
                {
                    var executionTime = DateTime.UtcNow - startTime;
                    return RuleResult.Failure($"Async rule evaluation failed: {ex.Message}", executionTime);
                }
            }

            // Fall back to synchronous evaluation
            return await base.EvaluateAsync(context);
        }
    }
}
