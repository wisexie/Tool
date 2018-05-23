using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Controls;
  

namespace DzTree.VideoRecoder.Core.MyBehaviorHelper
{
    public class ControlFocusBehaviorBase : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused", typeof(bool), typeof(ControlFocusBehaviorBase),
            new PropertyMetadata(IsFocusedPropertyChanged));

        private static void IsFocusedPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var p = dependencyObject as TextBox;
            if (p == null) return;
            if ((e.NewValue is bool ? (bool)e.NewValue : false))
            {
                p.Focus();
            }
        }

        public static bool GetIsFocused(Behavior p)
        {
            return p.GetValue(IsFocusedProperty) is bool ? (bool)p.GetValue(IsFocusedProperty) : false;
        }

        public static void SetIsFocused(Behavior p, bool value)
        {
            p.SetValue(IsFocusedProperty, value);
        }
    }  
}
