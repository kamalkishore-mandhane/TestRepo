using System.Collections.Generic;
using System.Windows.Input;

namespace SafeShare.WPFUI.Utils
{
    public static class NavigationCommandsManager
    {
        private static List<KeyGesture> BackKeyGestures = new List<KeyGesture>();
        private static List<KeyGesture> ForwardKeyGestures = new List<KeyGesture>();
        private static List<KeyGesture> RefreshKeyGestures = new List<KeyGesture>();

        public static void RemoveBrowseBackKeyGestures()
        {
            // preserve all key gestures for BrowseBack command
            foreach (var gesture in NavigationCommands.BrowseBack.InputGestures)
            {
                if (gesture is KeyGesture keyGesture)
                {
                    BackKeyGestures.Add(keyGesture);
                }
            }

            // remove all key gestures for BrowseBack command
            foreach (var keyGesture in BackKeyGestures)
            {
                NavigationCommands.BrowseBack.InputGestures.Remove(keyGesture);
            }
        }

        public static void ResetBrowseBackKeyGestures()
        {
            // reset removed key gestures for BrowseBack command
            if (BackKeyGestures != null && BackKeyGestures.Count > 0)
            {
                NavigationCommands.BrowseBack.InputGestures.AddRange(BackKeyGestures);
            }

            // clear preserved key gestures
            BackKeyGestures.Clear();
        }

        public static void RemoveBrowseForwardKeyGestures()
        {
            // preserve all key gestures for BrowseForward command
            foreach (var gesture in NavigationCommands.BrowseForward.InputGestures)
            {
                if (gesture is KeyGesture keyGesture)
                {
                    ForwardKeyGestures.Add(keyGesture);
                }
            }

            // remove all key gestures for BrowseForward command
            foreach (var keyGesture in ForwardKeyGestures)
            {
                NavigationCommands.BrowseForward.InputGestures.Remove(keyGesture);
            }
        }

        public static void ResetBrowseForwardKeyGestures()
        {
            // reset removed key gestures for BrowseForward command
            if (ForwardKeyGestures != null && ForwardKeyGestures.Count > 0)
            {
                NavigationCommands.BrowseForward.InputGestures.AddRange(ForwardKeyGestures);
            }

            // clear preserved key gestures
            ForwardKeyGestures.Clear();
        }

        public static void RemoveBrowseRefreshKeyGestures()
        {
            // preserve all key gestures for Refresh command
            foreach (var gesture in NavigationCommands.Refresh.InputGestures)
            {
                if (gesture is KeyGesture keyGesture)
                {
                    RefreshKeyGestures.Add(keyGesture);
                }
            }

            // remove all key gestures for Refresh command
            foreach (var keyGesture in RefreshKeyGestures)
            {
                NavigationCommands.Refresh.InputGestures.Remove(keyGesture);
            }
        }

        public static void ResetBrowseRefreshKeyGestures()
        {
            // reset removed key gestures for Refresh command
            if (RefreshKeyGestures != null && RefreshKeyGestures.Count > 0)
            {
                NavigationCommands.Refresh.InputGestures.AddRange(RefreshKeyGestures);
            }

            // clear preserved key gestures
            RefreshKeyGestures.Clear();
        }
    }
}