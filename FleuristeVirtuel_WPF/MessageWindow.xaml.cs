﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FleuristeVirtuel_WPF
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : GUIWindow
    {
        MessageResult lastResult = MessageResult.Continue;

        public MessageWindow(string message, string title, bool canContinue = true, bool canCancel = false, bool canRetry = false)
        {
            InitializeComponent();

            MessageContent.Text = message;
            Title = title;

            int count = 0;

            if (canCancel)
            {
                ButtonCancel.Visibility = Visibility.Visible;
                count++;
            }

            if (canRetry)
            {
                ButtonRetry.Visibility = Visibility.Visible;
                count++;
            }

            if (canContinue || count == 0)
            {
                ButtonContinue.Visibility = Visibility.Visible;
                count++;
            }

            ButtonsGrid.Columns = count;
        }

        public MessageResult ShowMessage()
        {
            ShowDialog();
            return lastResult;
        }

        public static MessageResult Show(string message, string title, bool canContinue = true, bool canCancel = false, bool canRetry = false)
        {
            return new MessageWindow(message, title, canContinue, canCancel, canRetry).ShowMessage();
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public enum MessageResult
        {
            Continue, Cancel, Retry
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Cancel;
            Close();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Continue;
            Close();
        }

        private void ButtonRetry_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Retry;
            Close();
        }

        private void GUIWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
        }
    }
}
