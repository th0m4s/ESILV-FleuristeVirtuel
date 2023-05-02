using FleuristeVirtuel_API;
using FleuristeVirtuel_API.Utils;
using FleuristeVirtuel_API.Types;
using System;
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
using System.Reflection;
using System.Collections;

namespace FleuristeVirtuel_WPF
{
    /// <summary>
    /// Interaction logic for ExportDataWindow.xaml
    /// </summary>
    public partial class ExportDataWindow : GUIWindow
    {
        List<DbRecord> data;
        Type originalType;

        private ExportDataWindow(List<DbRecord> data, Type originalType)
        {
            this.data = data;
            this.originalType = originalType;
            InitializeComponent();
        }

        public static void Open<T>(IEnumerable data) where T : DbRecord
        {
            ExportDataWindow window = new(data.Cast<DbRecord>().ToList(), typeof(T));
            window.ShowDialog();
        }

        private void CancelExport_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmExport_Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)FileType_ComboBox.SelectedItem;
            string? type = cbi.Content.ToString()?.ToLower();
            if (type == null) return;

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "exported_data";

            switch(type)
            {
                case "xml":
                    dialog.DefaultExt = ".xml";
                    dialog.Filter = "Documents XML (.xml)|*.xml";
                    break;
                case "json":
                    dialog.DefaultExt = ".json";
                    dialog.Filter = "Documents JSON (.json)|*.json";
                    break;
                default:
                    MessageWindow.Show("Type de fichier invalide ! " + type, "Impossible d'exporter le fichier");
                    return;
            }

            if (!(dialog.ShowDialog() ?? false)) return;
            string fileName = dialog.FileName;

            try
            {
                switch (type)
                {
                    case "xml":
                        data.ExportToXml(originalType, fileName);
                        break;
                    case "json":
                        data.ExportToJson(originalType, fileName);
                        break;
                }

                MessageWindow.Show("Les données ont été exportées !", "Exportation réussie");
            } catch(Exception ex)
            {
                MessageWindow.Show("Une erreur est survenue lors de l'export : " + ex, "Erreur d'exportation");
            }

            Close();
        }
    }
}
