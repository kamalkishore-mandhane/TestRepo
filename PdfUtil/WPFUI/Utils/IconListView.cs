using PdfUtil.WPFUI.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PdfUtil.WPFUI.Utils
{
    public class IconListView : ListView
    {
        private bool _isProcessMouseEventByUp = false;
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Line)
            {
                base.OnPreviewMouseDown(e);
                return;
            }

            var checkbox = VisualTreeHelperUtils.FindAncestor<CheckBox>((DependencyObject)e.OriginalSource);
            if (checkbox != null)
            {
                base.OnPreviewMouseDown(e);
                return;
            }

            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem != null)
            {
                var icon = ItemContainerGenerator.ItemFromContainer(listViewItem) as IconItem;
                if (icon != null && SelectedItems.Contains(icon) && Keyboard.Modifiers == ModifierKeys.None)
                {
                    _isProcessMouseEventByUp = true;
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (_isProcessMouseEventByUp)
            {
                var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                    var icon = ItemContainerGenerator.ItemFromContainer(listViewItem) as IconItem;
                    if (icon != null)
                    {
                        SelectedIndex = -1;// clear;
                        SelectedItem = icon;
                    }
                }

                _isProcessMouseEventByUp = false;
            }
        }

        public new void SetSelectedItems(IEnumerable selectedItems)
        {
            base.SetSelectedItems(selectedItems);
        }
    }
}
