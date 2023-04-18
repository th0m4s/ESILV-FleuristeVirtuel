using FleuristeVirtuel_API;
using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FleuristeVirtuel_WPF
{
    public class DbRecordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GUIWindow
    {
        DbConnection conn;

        public MainWindow()
        {
            conn = new();
            InitializeComponent();
        }

        public void Reload_Magasins()
        {
            List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin");
            foreach(var m in magasins) m.FetchForeignReferences(conn);
            Magasin_DataGrid.ItemsSource = magasins;
        }

        private void Magasin_Add_Click(object sender, RoutedEventArgs e)
        {
            AddEditMagasin addEditWindow = new();
            addEditWindow.ShowDialog();

            TMagasin? new_magasin = addEditWindow.value;
            if(addEditWindow.Submitted && new_magasin != null)
            {
                TAdresse? new_adresse = new_magasin.adresse_localisation;
                if(new_adresse != null)
                {
                    new_adresse.InsertInto("adresse", conn);

                    new_magasin.id_adresse_localisation = new_adresse.id_adresse;
                    new_magasin.InsertInto("magasin", conn);

                    Reload_Magasins();
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabItem? tab = ((TabControl)e.Source).SelectedItem as TabItem;
                if (tab == null) return;

                if (tab.Name == "Magasin_Tab")
                {
                    Reload_Magasins();
                }
            }
        }

        private void Magasin_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Magasins();
        }
        private void Magasin_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_magasin))
            {
                TMagasin magasin = DbRecord.CreateEmptyOrGetInstance<TMagasin>(id_magasin);

                if(MessageBox.Show("Voulez-vous supprimer le magasin #" + id_magasin + " (" + magasin.nom_magasin + ")",
                    "Suppression d'un magasin", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    magasin.DeleteFrom("magasin", conn);
                    magasin.adresse_localisation?.DeleteFrom("adresse", conn);
                    Reload_Magasins();
                }
            }
        }

        private void Magasin_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_magasin))
            {
                TMagasin magasin = DbRecord.CreateEmptyOrGetInstance<TMagasin>(id_magasin);

                AddEditMagasin addEditWindow = new(magasin);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    magasin.adresse_localisation?.Update("adresse", conn);
                    magasin.Update("magasin", conn);

                    Reload_Magasins();
                }
            }
        }
    }

    public static class CustomDataClass
    {

        public static readonly DependencyProperty CustomDataProperty = DependencyProperty.RegisterAttached("CustomData",
            typeof(string), typeof(CustomDataClass), new FrameworkPropertyMetadata(null));

        public static string GetCustomData(UIElement? element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (string)element.GetValue(CustomDataProperty);
        }
        public static void SetCustomData(UIElement? element, string value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(CustomDataProperty, value);
        }
    }
}
