using Threeyes.Core;

namespace Threeyes.ShowHide
{
    public class ShowOnDebugBuild : ShowAndHide
    {
        protected override void Awake()
        {
            base.Awake();

            Show(PlatformTool.IsDebugBuild);
        }
    }
}
