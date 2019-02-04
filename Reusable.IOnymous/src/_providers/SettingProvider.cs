using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider([NotNull] ResourceMetadata metadata)
            : base(new SoftString[] { "setting" }, metadata)
        {
        }
    }
}