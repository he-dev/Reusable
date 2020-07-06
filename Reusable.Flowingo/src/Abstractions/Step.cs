using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Flowingo.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.Flowingo.Abstractions
{
    public interface IStep<T> : INode<IStep<T>>
    {
        bool Enabled { get; set; }

        object Tag { get; }

        Task ExecuteAsync(T context);
    }

    public abstract class Step<T> : IStep<T>
    {
        protected Step()
        {
            Tag = this.CreateTag();
            Logger = ServiceContext.Resolve<ILoggerFactory>().CreateLogger(GetType().ToPrettyString());
        }

        public IStep<T>? Prev { get; set; }

        public IStep<T>? Next { get; set; }

        public bool Enabled { get; set; } = true;

        public object Tag { get; set; }

        protected ILogger? Logger { get; }

        public virtual async Task ExecuteAsync(T context)
        {
            using (Logger?.BeginScope("ExecuteStep", new { Tag }))
            {
                try
                {
                    Validator.ValidateObject(this, new ValidationContext(this), true);
                    if (await ExecuteBody(context) == Flow.Break)
                    {
                        Logger?.Log(Telemetry.Collect.Application().Logic().Decision("Do not execute the next step.").Because($"{Flow.Break}"));
                        return;
                    }
                    else
                    {
                        Logger?.Log(Telemetry.Collect.Application().Logic().Decision("Execute the next step.").Because($"{Flow.Continue}"));
                    }
                }
                catch (Exception e)
                {
                    Logger?.Scope().Exceptions.Push(e);
                }
            }

            await ExecuteNextAsync(context);
        }

        protected abstract Task<Flow> ExecuteBody(T context);

        protected async Task ExecuteNextAsync(T context)
        {
            if (this.EnumerateNextWithoutSelf<IStep<T>>().FirstOrDefault(s => !(s is Continue<T>) && s.Enabled) is {} next)
            {
                await next.ExecuteAsync(context);
            }
        }
    }

    public class Continue<T> : Step<T>
    {
        public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);

        protected override Task<Flow> ExecuteBody(T context) => Flow.Continue.ToTask();
    }

    public class Break<T> : Step<T>
    {
        public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);

        protected override Task<Flow> ExecuteBody(T context) => Flow.Break.ToTask();
    }

    public static class StepHelper
    {
        public static string CreateTag<T>(this IStep<T> step)
        {
            return Regex.Replace(step.GetType().ToPrettyString(), @"(\<?[A-Z])", m =>
            {
                if (m.Index == 0)
                {
                    return m.Value;
                }

                return $"-{m.Value.Trim('<')}";
            }).ToLower();
        }
    }
}