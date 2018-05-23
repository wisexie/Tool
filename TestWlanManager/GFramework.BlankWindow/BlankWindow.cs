﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TestWlanManager.GFramework.BlankWindow.Behaviours;
using TestWlanManager.GFramework.BlankWindow.Native;

namespace TestWlanManager.GFramework.BlankWindow
{
    [TemplatePart(Name = PART_TITLEBAR, Type = typeof(UIElement))]
    public class BlankWindow : Window
    {
        private const string PART_TITLEBAR = "PART_TitleBar";

        public double TitleBarHeight
        {
            get { return (double)GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }

        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register("TitleBarHeight", typeof(double), typeof(BlankWindow), new PropertyMetadata(30d));

        public bool IgnoreTaskbarOnMaximize
        {
            get { return (bool)GetValue(IgnoreTaskbarOnMaximizeProperty); }
            set { SetValue(IgnoreTaskbarOnMaximizeProperty, value); }
        }

        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty =
          DependencyProperty.Register("IgnoreTaskbarOnMaximize", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false));

        public Brush GlowBrush
        {
            get { return (Brush)GetValue(GlowBrushProperty); }
            set { SetValue(GlowBrushProperty, value); }
        }

        public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register("GlowBrush", typeof(Brush), typeof(BlankWindow), new PropertyMetadata(null));

        public Brush NonActiveGlowBrush
        {
            get { return (Brush)GetValue(NonActiveGlowBrushProperty); }
            set { SetValue(NonActiveGlowBrushProperty, value); }
        }

        public static readonly DependencyProperty NonActiveGlowBrushProperty = DependencyProperty.Register("NonActiveGlowBrush", typeof(Brush), typeof(BlankWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(153, 153, 153)))); // #999999

        public bool HasMaximized
        {
            get { return (bool)GetValue(HasMaximizedProperty); }
            set { SetValue(HasMaximizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasMaximized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasMaximizedProperty =
            DependencyProperty.Register("HasMaximized", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false));

        static BlankWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlankWindow), new FrameworkPropertyMetadata(typeof(BlankWindow)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnMaximizedChanged();

            AddHandler(MouseDownEvent, new MouseButtonEventHandler(TitleBarMouseDown), true);
        }

        protected void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            var draggable = GetDraggable((DependencyObject)e.Source) || GetDraggable((DependencyObject)e.OriginalSource);
            if (!draggable)
            {
                return;
            }
            // if UseNoneWindowStyle = true no movement, no maximize please
            if (e.ChangedButton == MouseButton.Left)
            {
                var mPoint = Mouse.GetPosition(this);

                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                UnsafeNativeMethods.ReleaseCapture();
                var wpfPoint = PointToScreen(mPoint);
                var x = Convert.ToInt16(wpfPoint.X);
                var y = Convert.ToInt16(wpfPoint.Y);
                var lParam = (int)(uint)x | (y << 16);
                UnsafeNativeMethods.SendMessage(windowHandle, Constants.WM_NCLBUTTONDOWN, Constants.HT_CAPTION, lParam);
                var canResize = ResizeMode == ResizeMode.CanResizeWithGrip || ResizeMode == ResizeMode.CanResize;
                // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
                //var isMouseOnTitlebar = mPoint.Y <= TitleBarHeight && TitleBarHeight > 0;
                //if (e.ClickCount == 2 && canResize && isMouseOnTitlebar)
                if (e.ClickCount == 2 && canResize)
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        Microsoft.Windows.Shell.SystemCommands.RestoreWindow(this);
                    }
                    else
                    {
                        Microsoft.Windows.Shell.SystemCommands.MaximizeWindow(this);
                    }
                }
            }
        }

        internal T GetPart<T>(string name) where T : DependencyObject
        {
            return GetTemplateChild(name) as T;
        }

        /// <summary>
        /// Gets the window placement settings (can be overwritten).
        /// </summary>
        public virtual IWindowPlacementSettings GetWindowPlacementSettings()
        {
            return WindowPlacementSettings ?? new WindowApplicationSettings(this);
        }

        public IWindowPlacementSettings WindowPlacementSettings { get; set; }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            OnMaximizedChanged();
        }

        protected virtual void OnMaximizedChanged()
        {
            HasMaximized = WindowState == WindowState.Maximized;
        }

        public static bool GetDraggable(DependencyObject obj)
        {
            return (bool)obj.GetValue(DraggableProperty);
        }

        public static void SetDraggable(DependencyObject obj, bool value)
        {
            obj.SetValue(DraggableProperty, value);
        }

        // Using a DependencyProperty as the backing store for Draggable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggableProperty =
            DependencyProperty.RegisterAttached("Draggable", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false,
                (o,arg)=>
                {
                    var w = Window.GetWindow(o);
                    if (w == null)
                        return;
                    if (w as BlankWindow == null)
                    {
                        w.AddHandler(MouseDownEvent, new MouseButtonEventHandler((sender, e) => {
                            if (e.ChangedButton == MouseButton.Left)
                            {
                                var mPoint = Mouse.GetPosition(w);

                                IntPtr windowHandle = new WindowInteropHelper(w).Handle;
                                UnsafeNativeMethods.ReleaseCapture();
                                var wpfPoint = w.PointToScreen(mPoint);
                                var x = Convert.ToInt16(wpfPoint.X);
                                var y = Convert.ToInt16(wpfPoint.Y);
                                var lParam = (int)(uint)x | (y << 16);
                                UnsafeNativeMethods.SendMessage(windowHandle, Constants.WM_NCLBUTTONDOWN, Constants.HT_CAPTION, lParam);
                                var canResize = w.ResizeMode == ResizeMode.CanResizeWithGrip || w.ResizeMode == ResizeMode.CanResize;
                                // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
                                //var isMouseOnTitlebar = mPoint.Y <= TitleBarHeight && TitleBarHeight > 0;
                                //if (e.ClickCount == 2 && canResize && isMouseOnTitlebar)
                                if (e.ClickCount == 2 && canResize)
                                {
                                    if (w.WindowState == WindowState.Maximized)
                                    {
                                        Microsoft.Windows.Shell.SystemCommands.RestoreWindow(w);
                                    }
                                    else
                                    {
                                        Microsoft.Windows.Shell.SystemCommands.MaximizeWindow(w);
                                    }
                                }
                            }

                        }), false);
                    }
                }));

        public static bool GetMinimize(DependencyObject obj)
        {
            return (bool)obj.GetValue(MinimizeProperty);
        }

        public static void SetMinimize(DependencyObject obj, bool value)
        {
            obj.SetValue(MinimizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for Minimized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimizeProperty =
            DependencyProperty.RegisterAttached("Minimize", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false,
                (o, args) =>
                {
                    o.ApplyBehavior<WindowMinimizedBehavior>();
                }));



        public static bool GetMaximize(DependencyObject obj)
        {
            return (bool)obj.GetValue(MaximizeProperty);
        }

        public static void SetMaximize(DependencyObject obj, bool value)
        {
            obj.SetValue(MaximizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for Maximized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximizeProperty =
            DependencyProperty.RegisterAttached("Maximize", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false, (o, args) =>
            {
                o.ApplyBehavior<WindowMaximizedBehavior>();
            }));

        public static bool GetCloseable(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseableProperty);
        }

        public static void SetCloseable(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseableProperty, value);
        }

        // Using a DependencyProperty as the backing store for Closed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseableProperty =
            DependencyProperty.RegisterAttached("Closeable", typeof(bool), typeof(BlankWindow), new PropertyMetadata(false,
                (o, args) =>
                {
                    o.ApplyBehavior<WindowClosedBehavior>();
                }));



    }
}
