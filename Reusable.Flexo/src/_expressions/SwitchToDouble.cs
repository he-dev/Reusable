using System.Collections.Generic;

namespace Reusable.Flexo
{
    /// <summary>
    /// Provides a mapping expression from bool to double.
    /// </summary>
    public class SwitchToDouble : Switch
    {
        public SwitchToDouble(string name) : base(name)
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
                    When = Constant.Null,
                    Body = Double.Zero
                }
            };
            Default = new Throw("SwitchValueOutOfRange")
            {
                //Exception = $"ValueOutOfRange",
                Message = $"{nameof(SwitchToDouble)} value must be either 'true', 'false' or 'null'."
            };
        }

        public SwitchToDouble() : this(nameof(SwitchToDouble)) { }
    }
}