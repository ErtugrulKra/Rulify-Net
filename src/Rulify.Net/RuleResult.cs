using System;
using System.Collections.Generic;

namespace Rulify.Net
{
    /// <summary>
    /// Represents the result of a rule evaluation
    /// </summary>
    public class RuleResult
    {
        /// <summary>
        /// Gets whether the rule evaluation was successful
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the error message if the rule evaluation failed
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets additional data returned by the rule
        /// </summary>
        public Dictionary<string, object> Data { get; }

        /// <summary>
        /// Gets the execution time of the rule
        /// </summary>
        public TimeSpan ExecutionTime { get; }

        /// <summary>
        /// Initializes a new instance of the RuleResult class
        /// </summary>
        /// <param name="isSuccess">Whether the rule evaluation was successful</param>
        /// <param name="errorMessage">The error message if the rule evaluation failed</param>
        /// <param name="executionTime">The execution time of the rule</param>
        /// <param name="data">Additional data returned by the rule</param>
        public RuleResult(bool isSuccess, string? errorMessage = null, TimeSpan executionTime = default, Dictionary<string, object>? data = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ExecutionTime = executionTime;
            Data = data ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a successful rule result
        /// </summary>
        /// <param name="data">Additional data returned by the rule</param>
        /// <param name="executionTime">The execution time of the rule</param>
        /// <returns>A successful rule result</returns>
        public static RuleResult Success(Dictionary<string, object>? data = null, TimeSpan executionTime = default)
        {
            return new RuleResult(true, null, executionTime, data);
        }

        /// <summary>
        /// Creates a failed rule result
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="executionTime">The execution time of the rule</param>
        /// <param name="data">Additional data returned by the rule</param>
        /// <returns>A failed rule result</returns>
        public static RuleResult Failure(string errorMessage, TimeSpan executionTime = default, Dictionary<string, object>? data = null)
        {
            return new RuleResult(false, errorMessage, executionTime, data);
        }
    }
}
