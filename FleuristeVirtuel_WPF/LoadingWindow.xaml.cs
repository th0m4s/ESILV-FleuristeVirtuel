using FleuristeVirtuel_API;
using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FleuristeVirtuel_WPF
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : GUIWindow
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public static List<T> SelectMultipleRecords<T>(DbConnection conn, string command, params DbParam[] parameters) where T : DbRecord, new()
        {
            List<T> result = new();
            LoadingWindow window = new();
            Thread thread = new(() => {
                result = conn.SelectMultipleRecords<T>(command, parameters);
                window.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(window.Close));
            });
            thread.Start();
            window.ShowDialog();
            return result;
        }
    }
}
