using FleuristeVirtuel_API;
using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value?.ToString() + "€" ?? "0€";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string str)
            {
                return str.ToLower();
            }
            else return value;
        }
    }

    public class ProductCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if(value is string str)
            {
                switch(str)
                {
                    case "fleur":
                        return "Fleur";
                    case "accessoire":
                        return "Accessoire";
                }
            }
            
            return value?.ToString() ?? "null";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GUIWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        LoggedInStatus accountStatus = LoggedInStatus.None;
        string currentUser = "";
        private bool canEdit = false;
        public bool CanEdit
        {
            get => canEdit;
            set
            {
                canEdit = value;
                OnPropertyChanged();
            }
        }

        DbConnection conn;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static Dictionary<string, KeyValuePair<string, bool>> admin_accounts = new()
        {
            ["root"] = new("root", true),
            ["bozo"] = new("bozo", false)
        };

        public MainWindow()
        {            
            InitializeComponent();
            DataContext = this;

            conn = new();
            UpdateViewFromAccountStatus();
            
            // we should use environment variables as this is not secure AT ALL
            // but we are required to use this username/password and our teachers might not change an environment
            // variable from MODE=dev to MODE=prod (for example)
            Login("root", "root");
        }

        public bool IsLoggedIn()
        {
            return conn != null;
        }

        public void UpdateViewFromAccountStatus()
        {
            if(accountStatus == LoggedInStatus.None)
            {
                index_login_panel.Visibility = Visibility.Visible;
                index_logout_panel.Visibility = Visibility.Collapsed;
            } else
            {
                index_login_panel.Visibility = Visibility.Collapsed;
                index_logout_panel.Visibility = Visibility.Visible;
                
                if(accountStatus == LoggedInStatus.Client)
                {
                    index_current_account.Text = "Actuellement connecté en tant que client : " + currentUser;
                } else
                {
                    index_current_account.Text = "Actuallement connecté en tant qu'administrateur : " + currentUser;
                    if (!CanEdit) index_current_account.Text += " (lecture seule)";
                }
            }

            Visibility adminTabVisiblity = accountStatus == LoggedInStatus.Admin ? Visibility.Visible : Visibility.Collapsed;

            Magasin_Tab.Visibility = adminTabVisiblity;
            Produit_Tab.Visibility = adminTabVisiblity;
            Bouquet_Tab.Visibility = adminTabVisiblity;
            Client_Tab.Visibility = adminTabVisiblity;
            Commande_Tab.Visibility = adminTabVisiblity;
        }

        public void Reload_Magasins()
        {
            List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin");
            foreach(var m in magasins) m.FetchForeignReferences(conn);
            Magasin_DataGrid.ItemsSource = magasins;
        }

        public void Reload_Produits()
        {
            List<TProduit> produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit");
            Produit_DataGrid.ItemsSource = produits;
        }

        public void Reload_Clients()
        {
            List<TClient> clients = conn.SelectMultipleRecords<TClient>("SELECT * from client");
            foreach(var c in clients) c.FetchForeignReferences(conn);
            Client_DataGrid.ItemsSource = clients;
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

                switch(tab.Name)
                {
                    case "Magasin_Tab":
                        Reload_Magasins();
                        break;
                    case "Produit_Tab":
                        Reload_Produits();
                        break;
                    case "Client_Tab":
                        Reload_Clients();
                        break;
                }
            }
        }

        private void Magasin_Reload_Click(object sender, RoutedEventArgs e)
        {
            MessageWindow.Show("ceci est un long message\n\nsur plsueirus lignes\n\nhey\n\n\n\n\nplease continue", true, false, true);
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

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid datagridSender && e.AddedCells?.Count > 0)
                datagridSender.UnselectAll();
        }

        private void Produit_Add_Click(object sender, RoutedEventArgs e)
        {
            AddEditProduit addEditWindow = new();
            addEditWindow.ShowDialog();

            TProduit? new_produit = addEditWindow.value;
            if (addEditWindow.Submitted && new_produit != null)
            {
                new_produit.InsertInto("produit", conn);

                Reload_Produits();
            }
        }

        private void Produit_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Produits();
        }

        private void Produit_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_produit))
            {
                TProduit produit = DbRecord.CreateEmptyOrGetInstance<TProduit>(id_produit);

                AddEditProduit addEditWindow = new(produit);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    produit.Update("produit", conn);

                    Reload_Produits();
                }
            }
        }

        private void Produit_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_produit))
            {
                TProduit produit = DbRecord.CreateEmptyOrGetInstance<TProduit>(id_produit);

                if (MessageBox.Show("Voulez-vous supprimer le produit #" + id_produit + " (" + produit.nom_produit + ")\n\n" +
                    "Cette action sera impossible si le produit est utilisé dans un bouquet ou une commande.",
                    "Suppression d'un produit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    produit.DeleteFrom("produit", conn);
                    Reload_Produits();
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if(accountStatus != LoggedInStatus.None)
            {
                currentUser = "";
                accountStatus = LoggedInStatus.None;
                conn.DisposeCurrentConnection();

                login_textbox_user.Text = "";
                login_textbox_password.Password = "";

                UpdateViewFromAccountStatus();
            }
        }

        private void Login(string userEmail, string password)
        {
            if (userEmail.Length == 0 || password.Length == 0) return;

            foreach (var account in admin_accounts)
            {
                if (account.Key == userEmail && account.Value.Key == password)
                {
                    accountStatus = LoggedInStatus.Admin;
                    CanEdit = account.Value.Value;
                    conn.Open(userEmail, password);
                    break;
                }
            }

            if (accountStatus == LoggedInStatus.None)
            {
                conn.Open("root", "root");
                int count = conn.SelectSingleCell<int>("SELECT COUNT(*) FROM client WHERE email_client = @email AND mot_de_passe = @password",
                    0, new DbParam("@email", userEmail), new DbParam("@password", password));

                if (count > 0)
                {
                    accountStatus = LoggedInStatus.Client;
                }
                else
                {
                    conn.DisposeCurrentConnection();
                }
            }

            if (accountStatus == LoggedInStatus.None)
                MessageBox.Show("L'email/nom d'utilisateur et/ou le mot de passe ne correspondent pas !",
                    "Informations invalide !", MessageBoxButton.OK);
            else
            {
                currentUser = userEmail;
                UpdateViewFromAccountStatus();
            }
        }

        private void LogginButton_Click(object sender, RoutedEventArgs e)
        {
            if(accountStatus == LoggedInStatus.None)
            {
                string userEmail = login_textbox_user.Text.Trim();
                string password = login_textbox_password.Password.Trim();

                Login(userEmail, password);
            }
        }

        private void Client_Add_Click(object sender, RoutedEventArgs e)
        {
            AddEditClient addEditWindow = new();
            addEditWindow.ShowDialog();

            TClient? new_client = addEditWindow.value;
            if (addEditWindow.Submitted && new_client != null && new_client.adresse_facturation != null)
            {
                new_client.adresse_facturation.InsertInto("adresse", conn);
                new_client.id_adresse = new_client.adresse_facturation.id_adresse;
                new_client.InsertInto("client", conn);

                Reload_Clients();
            }
        }

        private void Client_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Clients();
        }

        private void Client_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_client))
            {
                TClient client = DbRecord.CreateEmptyOrGetInstance<TClient>(id_client);

                if (MessageBox.Show("Voulez-vous supprimer le client #" + id_client + " (" + client.prenom_client + " " + client.nom_client + ")\n\n" +
                    "Cette action sera impossible si le client a déjà passé une commande.",
                    "Suppression d'un client", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    client.DeleteFrom("client", conn);
                    client.adresse_facturation?.DeleteFrom("adresse", conn);
                    Reload_Clients();
                }
            }
        }

        private void Client_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_client))
            {
                TClient client = DbRecord.CreateEmptyOrGetInstance<TClient>(id_client);

                AddEditClient addEditWindow = new(client);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    client.Update("client", conn);
                    client.adresse_facturation?.Update("adresse", conn);

                    Reload_Clients();
                }
            }
        }
    }

    public static class CustomDataClass
    {

        // si jamais on fait une propriété de type int ou uint, il n'y aurait pas de conversion à faire dans les edit/delete
        // mais on risque des problèmes si on se trompe dans le xaml
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

    public enum LoggedInStatus
    {
        None, Client, Admin
    }
}
