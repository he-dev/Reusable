using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Provides a mapping expression from bool to double.
    /// </summary>
    public class ToDouble : Switch
    {
        public ToDouble() : base(default, nameof(ToDouble))
        {
            Cases = new List<SwitchCase>
            {
                new SwitchCase
                {
                    When = Constant.True,
                    Body = Double.One
                },
                new SwitchCase
                {
                    When = Constant.False,
                    Body = Double.Zero
                },
                new SwitchCase
                {
                    When = Expression<bool>.Create("CheckIfDouble", ctx => This(ctx).Invoke(ctx).Value is double),
                    Body = Expression<double>.Create("PassDouble", ctx => This(ctx).Invoke(ctx).Value<double>()),
                },
                new SwitchCase
                {
                    When = null,
                    Body = Double.Zero
                }
            };

//            Default = new Throw
//            {
//                Name = "SwitchValueOutOfRange",
//                Message = Constant.FromValue("Message", $"{nameof(ToDouble)} value must be either 'true', 'false' or 'null'.")
//            };
        }
    }
}