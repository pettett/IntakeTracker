using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace nGantt;

public static class UIHelper
{
    public static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null)
            return null;

        if (parentObject is T parent)
            return parent;
        else
            return FindVisualParent<T>(parentObject);
    }
}
