using System;
using Reusable.Collections;

namespace Reusable.Commander
{
    public class CommandInfo : IEquatable<CommandInfo>
    {
        public CommandInfo(ArgumentName name, Type commandType)
        {
            CommandType = commandType;
            Name = name;
            ParameterType = commandType.GetCommandParameterType();

            //ValidateParameterPropertyNames(typeof(TParameter));
        }

        [AutoEqualityProperty]
        public ArgumentName Name { get; }

        public Type CommandType { get; }

        public Type ParameterType { get; }

        public string RegistrationKey => $"Commands.{Name.Primary}";

        public override int GetHashCode() => AutoEquality<CommandInfo>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as CommandInfo);

        public bool Equals(CommandInfo other) => AutoEquality<CommandInfo>.Comparer.Equals(this, other);
    }
}