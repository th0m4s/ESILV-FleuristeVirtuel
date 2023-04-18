using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FleuristeVirtuel_WPF
{
    public partial class GUIWindow : Window
    {
        public Grid? WindowRoot { get; private set; }
        public Grid? LayoutRoot { get; private set; }
        public Grid? HeaderBar { get; private set; }
        public Grid? MinimizeButton { get; private set; }
        public Grid? MaximizeButton { get; private set; }
        public Grid? CloseButton { get; private set; }
        public Border? WindowBorder { get; private set; }

        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)GetTemplateChild(childName);
        }

        public override void OnApplyTemplate()
        {
            WindowRoot = GetRequiredTemplateChild<Grid>("WindowRoot");
            LayoutRoot = GetRequiredTemplateChild<Grid>("LayoutRoot");

            HeaderBar = GetRequiredTemplateChild<Grid>("HeaderBar");
            MinimizeButton = GetRequiredTemplateChild<Grid>("MinimizeButton");
            MaximizeButton = GetRequiredTemplateChild<Grid>("MaximizeButton");
            CloseButton = GetRequiredTemplateChild<Grid>("CloseButton");

            WindowBorder = GetRequiredTemplateChild<Border>("WindowBorder");

            MinimizeButton.MouseDown += MinimizeButton_MouseDown;
            MaximizeButton.MouseDown += MaximizeButton_MouseDown;
            CloseButton.MouseDown += CloseButton_MouseDown;

            HeaderBar.MouseDown += HeaderBar_MouseDown;
            StateChanged += GUIWindow_StateChanged;


            base.OnApplyTemplate();
        }

        private void GUIWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowBorder != null)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowBorder.BorderThickness = new Thickness(6);
                }
                else
                {
                    WindowBorder.BorderThickness = new Thickness(0);
                }
            }
        }

        private void HeaderBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && ResizeMode == ResizeMode.CanResize)
            {
                ToggleWindowState();
                return;
            }

            if (WindowState != WindowState.Maximized)
            {
                DragMove();
            }
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void MaximizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleWindowState();
        }

        private void ToggleWindowState()
        {
            if (ResizeMode == ResizeMode.CanResize)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
