using System.Windows;
using System.Windows.Interactivity;

namespace TestWlanManager.GFramework.BlankWindow.Behaviours
{
    public class StylizedBehaviorCollection : FreezableCollection<Behavior>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StylizedBehaviorCollection();
        }
    }
}