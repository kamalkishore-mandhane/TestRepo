using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace SafeShare.WPFUI.Utils
{
    internal class VisualTreeHelperUtils
    {
        public static T FindSelfOrAncestor<T>(DependencyObject dependencyObject)
            where T : class
        {
            if (dependencyObject is T)
            {
                return dependencyObject as T;
            }

            return FindAncestor<T>(dependencyObject);
        }

        /// <summary>
        /// Find Parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class
        {
            DependencyObject target = dependencyObject;
            do
            {
                //// If target is not a Visual or Visual3D, VisualTreeHelper.GetParent() will throw a InvalidOperationException.
                //// https://social.msdn.microsoft.com/Forums/vstudio/en-US/5982cafe-f75b-42b4-99dc-50d3a81b30b0/invalidoperationexception-systemwindowsdocumentshyperlink-is-not-a-visual-or-visual3d?forum=wpf
                //// https://wpf.codeplex.com/workitem/10240
                if (target is Visual || target is System.Windows.Media.Media3D.Visual3D)
                {
                    target = VisualTreeHelper.GetParent(target);
                }
                else
                {
                    return null;
                }
            }
            while (target != null && !(target is T));
            return target as T;
        }

        /// <summary>
        /// Find Child with Depth First Search.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obfuscation(Exclude = true)]
        public static T FindVisualChild<T>(DependencyObject dependencyObject, Predicate<T> predicate = null)
            where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T && (predicate == null || predicate(child as T)))
                {
                    return child as T;
                }
                else
                {
                    T childOfChild = FindVisualChild(child, predicate);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find Child with Breadth First Search.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obfuscation(Exclude = true)]
        public static T FindVisualChildWithBFS<T>(DependencyObject dependencyObject, Predicate<T> predicate = null)
            where T : FrameworkElement
        {
            Queue childrenQueue = new Queue();
            childrenQueue.Enqueue(dependencyObject);

            while (childrenQueue.Count > 0)
            {
                DependencyObject dp = childrenQueue.Dequeue() as DependencyObject;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dp, i);
                    if (child is T && (predicate == null || predicate(child as T)))
                    {
                        return child as T;
                    }
                    else
                    {
                        childrenQueue.Enqueue(child);
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns Visual parent of the given element.
        ///     If includeContentElements is true then the
        ///     logic considers the logical parent of the content
        ///     element as visual parent.
        /// </summary>
        private static DependencyObject GetVisualParent(DependencyObject element, bool includeContentElements)
        {
            if (includeContentElements)
            {
                ContentElement ce = element as ContentElement;
                if (ce != null)
                {
                    return LogicalTreeHelper.GetParent(ce);
                }
            }
            return VisualTreeHelper.GetParent(element);
        }

        /// <summary>
        /// Walks up the visual tree and invalidates Measure till a parent of type PathEndType is found.
        /// </summary>
        /// <typeparam name="PathEndType">Invalidation ends when this type is found in the visual tree</typeparam>
        /// <param name="pathStart">Invalidation starts from this child</param>
        public static void InvalidateMeasureForVisualAncestorPath<PathEndType>(DependencyObject pathStart) where PathEndType : DependencyObject
        {
            InvalidateMeasureForVisualAncestorPath<PathEndType>(pathStart, /*includePathEnd*/ true);
        }

        /// <summary>
        ///     Walks up the visual tree and invalidates Measure till a parent of type PathEndType is found.
        ///     pathStart can be a ContentElement.
        /// </summary>
        public static void InvalidateMeasureForVisualAncestorPath<PathEndType>(DependencyObject pathStart, bool includePathEnd) where PathEndType : DependencyObject
        {
            // Allows element to be ContentElement
            bool includeContentElements = true;
            while (pathStart != null)
            {
                bool isEndType = pathStart is PathEndType;
                if (!includePathEnd && isEndType)
                {
                    return;
                }
                UIElement element = pathStart as UIElement;
                if (element != null)
                {
                    element.InvalidateMeasure();
                }
                if (isEndType)
                {
                    return;
                }
                pathStart = GetVisualParent(pathStart, includeContentElements);
                includeContentElements = false;
            }
        }
    }

    public class KeyboardUtil
    {
        public static bool IsCtrlKeyDown
        {
            get
            {
                return (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control;
            }
        }

        public static bool IsShiftKeyDown
        {
            get
            {
                return (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift;
            }
        }
    }
}