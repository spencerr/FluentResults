﻿using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace FluentResults
{
    public abstract class ResultBase
    {
        /// <summary>
        /// Is true if Reasons contains at least one error
        /// </summary>
        public bool IsFailed => Reasons.OfType<IError>().Any();

        /// <summary>
        /// Is true if Reasons contains no errors
        /// </summary>
        public bool IsSuccess => !IsFailed;

        /// <summary>
        /// Get all reasons (errors and successes)
        /// </summary>
        public List<IReason> Reasons { get; }

        /// <summary>
        /// Get all errors
        /// </summary>
        public List<IError> Errors => Reasons.OfType<IError>().ToList();

        /// <summary>
        /// Get all successes
        /// </summary>
        public List<ISuccess> Successes => Reasons.OfType<ISuccess>().ToList();

        protected ResultBase()
        {
            Reasons = new List<IReason>();
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type
        /// </summary>
        public bool HasError<TError>() where TError : IError
        {
            return HasError<TError>(error => true);
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type and with a specific condition
        /// </summary>
        public bool HasError<TError>(Func<TError, bool> predicate) where TError : IError
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return ResultHelper.HasError(Errors, predicate);
        }

        /// <summary>
        /// Check if the result object contains an error with a specific condition
        /// </summary>
        public bool HasError(Func<IError, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return ResultHelper.HasError(Errors, predicate);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type
        /// </summary>
        public bool HasSuccess<TSuccess>() where TSuccess : ISuccess
        {
            return HasSuccess<TSuccess>(success => true);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type and with a specific condition
        /// </summary>
        public bool HasSuccess<TSuccess>(Func<TSuccess, bool> predicate) where TSuccess : ISuccess
        {
            return ResultHelper.HasSuccess(Successes, predicate);
        }

        /// <summary>
        /// Check if the result object contains a success with a specific condition
        /// </summary>
        public bool HasSuccess(Func<Success, bool> predicate)
        {
            return ResultHelper.HasSuccess(Successes, predicate);
        }
    }

    public abstract class ResultBase<TResult> : ResultBase
        where TResult : ResultBase<TResult>
    {
        /// <summary>
        /// Add a reason (success or error)
        /// </summary>
        public TResult WithReason(IReason reason)
        {
            Reasons.Add(reason);
            return (TResult)this;
        }

        /// <summary>
        /// Add multiple reasons (success or error)
        /// </summary>
        public TResult WithReasons(IEnumerable<IReason> reasons)
        {
            Reasons.AddRange(reasons);
            return (TResult)this;
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError(string errorMessage)
        {
            return WithError(new Error(errorMessage));
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError(IError error)
        {
            return WithReason(error);
        }

        /// <summary>
        /// Add multiple errors
        /// </summary>
        public TResult WithErrors(IEnumerable<IError> errors)
        {
            return WithReasons(errors);
        }

        /// <summary>
        /// Add multiple errors
        /// </summary>
        public TResult WithErrors(IEnumerable<string> errors)
        {
            return WithReasons(errors.Select(errorMessage => new Error(errorMessage)));
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError<TError>()
            where TError : IError, new()
        {
            return WithError(new TError());
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess(string successMessage)
        {
            return WithSuccess(new Success(successMessage));
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess(Success success)
        {
            return WithReason(success);
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess<TSuccess>()
            where TSuccess : Success, new()
        {
            return WithSuccess(new TSuccess());
        }

        /// <summary>
        /// Log the result. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log()
        {
            return Log(string.Empty);
        }

        /// <summary>
        /// Log the result with a specific logger context
        /// </summary>
        public TResult Log(string context)
        {
            var logger = Result.Settings.Logger;

            logger.Log(context, this);

            return (TResult)this;
        }

        /// <summary>
        /// Log the result with a typed context. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log<TContext>()
        {
            var logger = Result.Settings.Logger;

            logger.Log<TContext>(this);

            return (TResult)this;
        }


        public override string ToString()
        {
            var reasonsString = Reasons.Any()
                ? $", Reasons='{ReasonFormat.ReasonsToString(Reasons)}'"
                : string.Empty;

            return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
        }
    }
}