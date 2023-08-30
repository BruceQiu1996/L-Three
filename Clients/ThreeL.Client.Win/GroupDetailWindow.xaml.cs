﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for GroupDetailWindow.xaml
    /// </summary>
    public partial class GroupDetailWindow : Window
    {
        public GroupDetailWindow(GroupDetailWindowViewModel groupDetailWindow)
        {
            InitializeComponent();
            DataContext = groupDetailWindow;
        }

        private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
