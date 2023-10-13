using System;
using System.Windows.Automation.Peers;

namespace SBkUpUI.WPFUI.Controls
{
    class FakeRibbonTwoLineTextAutomationPeer : FrameworkElementAutomationPeer
    {
        ///
        public FakeRibbonTwoLineTextAutomationPeer(FakeRibbonTwoLineText owner) : base(owner)
        { }

        ///
        protected override string GetClassNameCore()
        {
            return Owner.GetType().Name;
        }

        ///
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Text;
        }

        protected override bool IsControlElementCore()
        {
            // Return true if TextBlock is not part of the style
            return ((FakeRibbonTwoLineText)Owner).TemplatedParent == null;
        }

        /// <summary>
        ///   Returns name for automation clients to display
        /// </summary>
        protected override string GetNameCore()
        {
            string name = base.GetNameCore();
            if (String.IsNullOrEmpty(name))
            {
                name = ((FakeRibbonTwoLineText)Owner).Text;
            }

            return name;
        }

    }
}
