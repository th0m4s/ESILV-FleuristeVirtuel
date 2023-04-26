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
            Stock_Tab.Visibility = adminTabVisiblity;
            Bouquet_Tab.Visibility = adminTabVisiblity;
            Client_Tab.Visibility = adminTabVisiblity;
            Commande_Tab.Visibility = adminTabVisiblity;
        }

        public void Reload_Magasins()
        {
            try
            {
                List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin");
                foreach (var m in magasins) m.FetchForeignReferences(conn);
                Magasin_DataGrid.ItemsSource = magasins;
            } catch(Exception e)
            {
                Magasin_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser la liste des magasins :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        public void Reload_Produits()
        {
            try
            {
                List<TProduit> produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit");
                Produit_DataGrid.ItemsSource = produits;
            } catch(Exception e)
            {
                Produit_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser la liste des produits :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        public void Reload_Clients()
        {
            try
            {
                List<TClient> clients = conn.SelectMultipleRecords<TClient>("SELECT * FROM client");
                foreach (var c in clients) c.FetchForeignReferences(conn);
                Client_DataGrid.ItemsSource = clients;
            } catch(Exception e)
            {
                Client_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser la liste des clients :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        public void Prepare_Stocks()
        {
            try
            {
                List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin");
                magasins.Insert(0, new() { nom_magasin = "Tous les magasins" });
                Stocks_Selector_Magasin.ItemsSource = magasins;
                Stocks_Selector_Magasin.SelectedIndex = 0;

                List<TProduit> produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit");
                produits.Insert(0, new() { nom_produit = "Tous les produits" });
                Stocks_Selector_Produit.ItemsSource = produits;
                Stocks_Selector_Produit.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Stocks_Selector_Magasin.ItemsSource = new TMagasin[] { new() { nom_magasin = "Tous les magasins" } };
                Stocks_Selector_Produit.ItemsSource = new TProduit[] { new() { nom_produit = "Tous les produits" } };
                MessageBox.Show("Impossible de remplir les filtres des stocks :\n" +e, "Impossible de filtrer les stocks");
            }
        }

        public void Reload_Stocks()
        {
            try
            {
                string command = "SELECT * FROM stock";
                List<DbParam> cmdParams = new();
                bool hasWhere = false;

                if (Stocks_Selector_Magasin.SelectedIndex > 0)
                {
                    command += " WHERE id_magasin = @magasin";
                    hasWhere = true;
                    cmdParams.Add(new("@magasin", ((TMagasin)Stocks_Selector_Magasin.SelectedValue).id_magasin));
                }

                if (Stocks_Selector_Produit.SelectedIndex > 0)
                {
                    if (!hasWhere) command += " WHERE ";
                    else command = " AND ";
                    command += "id_produit = @produit";
                    cmdParams.Add(new("@produit", ((TProduit)Stocks_Selector_Produit.SelectedValue).id_produit));
                }

                List<TStock> stocks = conn.SelectMultipleRecords<TStock>(command, cmdParams.ToArray());
                foreach (var s in stocks) s.FetchForeignReferences(conn);
                Stock_DataGrid.ItemsSource = stocks;
            } catch(Exception e)
            {
                Stock_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser les stocks :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        private void Magasin_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditMagasin addEditWindow = new();
                addEditWindow.ShowDialog();

                TMagasin? new_magasin = addEditWindow.value;
                if (addEditWindow.Submitted && new_magasin != null)
                {
                    TAdresse? new_adresse = new_magasin.adresse_localisation;
                    if (new_adresse != null)
                    {
                        new_adresse.InsertInto("adresse", conn);

                        new_magasin.id_adresse_localisation = new_adresse.id_adresse;
                        new_magasin.InsertInto("magasin", conn);

                        Reload_Magasins();
                    }
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible d'ajouter un magasin :\n" + ex, "Impossible d'ajouter l'élément");
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
                    case "Stock_Tab":
                        Prepare_Stocks();
                        Reload_Stocks();
                        break;
                    case "Client_Tab":
                        Reload_Clients();
                        break;
                }
            }
        }

        private void Magasin_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Magasins();
        }
        private void Magasin_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_magasin))
                {
                    TMagasin magasin = DbRecord.CreateEmptyOrGetInstance<TMagasin>(id_magasin);

                    if (MessageWindow.Show("Voulez-vous supprimer le magasin #" + id_magasin + " (" + magasin.nom_magasin + ")",
                        "Suppression d'un magasin", true, true) == MessageWindow.MessageResult.Continue)
                    {
                        magasin.DeleteFrom("magasin", conn);
                        magasin.adresse_localisation?.DeleteFrom("adresse", conn);
                        Reload_Magasins();
                    }
                }
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le magasin :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Magasin_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
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
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de modifier ce magasin :\n" + ex, "Impossible de modifier l'élément");
            }
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid datagridSender && e.AddedCells?.Count > 0)
                datagridSender.UnselectAll();
        }

        private void Produit_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditProduit addEditWindow = new();
                addEditWindow.ShowDialog();

                TProduit? new_produit = addEditWindow.value;
                if (addEditWindow.Submitted && new_produit != null)
                {
                    new_produit.InsertInto("produit", conn);

                    Reload_Produits();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible d'ajouter un produit :\n" + ex, "Impossible d'ajouter l'élément");
            }
        }

        private void Produit_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Produits();
        }

        private void Produit_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
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
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de modifier le produit :\n" + ex, "Impossible de modifier l'élément");
            }
        }

        private void Produit_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_produit))
                {
                    TProduit produit = DbRecord.CreateEmptyOrGetInstance<TProduit>(id_produit);

                    if (MessageWindow.Show("Voulez-vous supprimer le produit #" + id_produit + " (" + produit.nom_produit + ")\n\n" +
                        "Cette action sera impossible si le produit est utilisé dans un bouquet ou une commande.",
                        "Suppression d'un produit", true, true) == MessageWindow.MessageResult.Continue)
                    {
                        produit.DeleteFrom("produit", conn);
                        Reload_Produits();
                    }
                }
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le produit :\n" + ex, "Impossible de supprimer l'élément");
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
            try
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
                    MessageWindow.Show("L'email/nom d'utilisateur et/ou le mot de passe ne correspondent pas !",
                        "Informations invalides !");
                else
                {
                    currentUser = userEmail;
                    UpdateViewFromAccountStatus();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Une erreur est survenue lors de la connexion :\n" + ex, "Impossible de se connecter");
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
            try
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
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible d'ajouter un client :\n" + ex, "Impossible d'ajouter l'élément");
            }
        }

        private void Client_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Clients();
        }

        private void Client_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_client))
                {
                    TClient client = DbRecord.CreateEmptyOrGetInstance<TClient>(id_client);

                    if (MessageWindow.Show("Voulez-vous supprimer le client #" + id_client + " (" + client.prenom_client + " " + client.nom_client + ")\n\n" +
                        "Cette action sera impossible si le client a déjà passé une commande.",
                        "Suppression d'un client", true, true, false) == MessageWindow.MessageResult.Continue)
                    {
                        client.DeleteFrom("client", conn);
                        client.adresse_facturation?.DeleteFrom("adresse", conn);
                        Reload_Clients();
                    }
                }
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le client :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Client_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
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
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le client :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Stock_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uint.TryParse(CustomDataClass.GetCustomData(sender as UIElement), out uint id_produit)
                && uint.TryParse(CustomDataClass.GetCustomDataBis(sender as UIElement), out uint id_magasin))
                {
                    TStock stock = DbRecord.CreateEmptyOrGetInstance<TStock>(id_produit, id_magasin);

                    EditStock editWindow = new(stock);
                    editWindow.ShowDialog();

                    if (editWindow.Submitted)
                    {
                        stock.Update("stock", conn);
                        Reload_Stocks();
                    }
                }
                else throw new ValueUnavailableException("Cannot fetch item from datagrid!");
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de modifier le stock :\n" + ex, "Impossible de modifier le stock");
            }
        }

        private void Stock_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Stocks();
        }

        private void Stocks_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reload_Stocks();
        }
    }

    public static class CustomDataClass
    {

        // si jamais on fait une propriété de type int ou uint, il n'y aurait pas de conversion à faire dans les edit/delete
        // mais on risque des problèmes si on se trompe dans le xaml, enfin le binding peut dire "invalid type"
        public static readonly DependencyProperty CustomDataProperty = DependencyProperty.RegisterAttached("CustomData",
            typeof(string), typeof(CustomDataClass), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CustomDataBisProperty = DependencyProperty.RegisterAttached("CustomDataBis",
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

        public static string GetCustomDataBis(UIElement? element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (string)element.GetValue(CustomDataBisProperty);
        }
        public static void SetCustomDataBis(UIElement? element, string value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(CustomDataBisProperty, value);
        }
    }

    public enum LoggedInStatus
    {
        None, Client, Admin
    }
}
