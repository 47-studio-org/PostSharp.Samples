﻿using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace PostSharp.Samples.AutoRetry
{
    /// <summary>
    /// Aspect that, when applied to a method, causes invocations of this method to be retried if the method ends with specified exceptions.
    /// </summary>
    [PSerializable]
    [LinesOfCodeAvoided(5)]
    public sealed class AutoRetryAttribute : MethodInterceptionAspect
    {
        /// <summary>
        /// Initializes a new <see cref="AutoRetryAttribute"/> with default values.
        /// </summary>
        public AutoRetryAttribute()
        {
            // Set the default values for properties.
            this.MaxRetries = 5;
            this.Delay = 3;
            this.HandledExceptions = new [] { typeof(WebException), typeof(DataException) };
        }

        /// <summary>
        /// Gets or sets the maximum number of retries. The default value is 5.
        /// </summary>
        public int MaxRetries { get; set; }


        /// <summary>
        /// Gets or sets the delay before retrying, in seconds. The default value is 3.
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// Gets or sets the type of exceptions that cause the method invocation to be retried. The default value is <see cref="WebException"/> and <see cref="DataException"/>.
        /// </summary>
        public Type[] HandledExceptions { get; set; }


        /// <summary>
        /// Method invoked <i>instead</i> of the original method.
        /// </summary>
        /// <param name="args">Method invocation context.</param>
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            for (var i = 0; ; i++)
            {
                try
                {
                    // Invoke the intercepted method.
                    args.Proceed();

                    // If we get here, it means the execution was successful.
                    return;
                }
                catch (Exception e)
                {
                    // The intercepted method threw an exception. Figure out if we can retry the method.

                    if (i < this.MaxRetries && (this.HandledExceptions == null || this.HandledExceptions.Any(type => type.IsInstanceOfType(e))))
                    {
                        // Yes, we can retry. Write some message and wait a bit.

                        Console.WriteLine("Method failed with exception {0}. Sleeping {1} s and retrying. This was our {2}th attempt.",
                            e.GetType().Namespace, this.Delay, i+1);

                        Thread.Sleep(TimeSpan.FromSeconds(this.Delay));

                        // Continue to the next iteration.
                    }
                    else
                    {
                        // No, we cannot retry. Retry the exception.
                        throw;
                    }
                }
            }
        }
    }
}
