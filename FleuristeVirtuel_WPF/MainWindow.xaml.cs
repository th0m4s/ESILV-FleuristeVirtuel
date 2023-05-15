using FleuristeVirtuel_API;
using FleuristeVirtuel_API.Types;
using MySqlX.XDevAPI;
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

            stats_period.ItemsSource = new List<int>() { 1, 3, 6, 12 };
            stats_period.SelectedIndex = 0;
            
            // we should use environment variables as this is not secure AT ALL
            // but we are required to use this username/password and our teachers might not change an environment
            // variable from MODE=dev to MODE=prod (for example)
            // Login("root", "root");
        }

        public bool IsLoggedIn()
        {
            return conn != null && conn.Connection?.State == System.Data.ConnectionState.Open;
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

                    var alerts = GetAllAlerts();
                    if (alerts.Count > 0)
                    {
                        string pluriel = alerts.Count > 1 ? "s" : "";
                        MessageWindow.Show($"Vous avez {alerts.Count} alerte{pluriel} de stock en cours.\n" +
                            "Ouvrez l'onglet Stocks et cochez la case \"Alertes\" pour plus d'informations.", "Alerte de stock");
                    }
                }
            }

            Visibility adminTabVisiblity = accountStatus == LoggedInStatus.Admin ? Visibility.Visible : Visibility.Collapsed;
            Magasin_Tab.Visibility = adminTabVisiblity;
            Produit_Tab.Visibility = adminTabVisiblity;
            Stock_Tab.Visibility = adminTabVisiblity;
            Bouquet_Tab.Visibility = adminTabVisiblity;
            Client_Tab.Visibility = adminTabVisiblity;
            Commande_Tab.Visibility = adminTabVisiblity;
            Stats_Tab.Visibility = adminTabVisiblity;

            Visibility clientTabVisibility = accountStatus == LoggedInStatus.Client ? Visibility.Visible : Visibility.Collapsed;
            PasserCommande_Tab.Visibility = clientTabVisibility;
        }

        public List<TStock> GetAllAlerts()
        {
            return conn.SelectMultipleRecords<TStock>("SELECT * FROM stock WHERE quantite_stock < @stocks_threshold;",
                new DbParam("@stocks_threshold", STOCKS_THRESHOLD));
        }

        private void setStat(TextBlock element, string value)
        {
            element.Text = element.Text.Split(":")[0].Trim() + " : " + value;
        }

        private void setStatPlural(TextBlock element, int value, string text)
        {
            setStat(element, value == 1 ? ("1 " + text) : (value + " " + text + "s"));
        }

        public void Reload_Stats()
        {
            if (!IsLoggedIn()) return;

            DbParam paramDuration = new DbParam("@duration", (int)stats_period.SelectedValue);

            int nb_commandes = conn.SelectSingleCell<int>("SELECT COUNT(*) FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH;", 0, paramDuration);
            setStatPlural(stats_nb_commandes, nb_commandes, "commande");

            float? ca_total = conn.SelectSingleCell<float?>("SELECT SUM(prix_avant_reduc - (prix_avant_reduc * pourc_reduc_prix / 100)) FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH;", 0, paramDuration);
            setStat(stats_total_ca, ca_total == null ? "N/A" : (ca_total + " €"));

            float? prix_moyen_commande = conn.SelectSingleCell<float?>("SELECT AVG(prix_avant_reduc) FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH;", 0, paramDuration);
            setStat(stats_prix_moyen, prix_moyen_commande == null ? "N/A" : (prix_moyen_commande + " €"));

            uint? id_bouquet = conn.SelectSingleCell<uint?>("SELECT id_bouquet_base, COUNT(id_bouquet_base) AS cnt FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH" +
                " GROUP BY id_bouquet_base ORDER BY cnt DESC LIMIT 1;", 0, paramDuration);
            TBouquet? bouquet = id_bouquet != null && id_bouquet > 0 ? 
                conn.SelectSingleRecord<TBouquet>("SELECT * FROM bouquet WHERE id_bouquet = @id_bouquet", new DbParam("@id_bouquet", id_bouquet)) : null;
            setStat(stats_bouquet, bouquet?.ToString() ?? "N/A");

            uint? id_magasin_commandes = conn.SelectSingleCell<uint?>("SELECT id_magasin, COUNT(id_magasin) AS cnt FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH" +
                " GROUP BY id_magasin ORDER BY cnt DESC LIMIT 1;", 0, paramDuration);
            TMagasin? magasin_commandes = id_magasin_commandes != null && id_magasin_commandes > 0 ?
                conn.SelectSingleRecord<TMagasin>("SELECT * FROM magasin WHERE id_magasin = @id_magasin", new DbParam("@id_magasin", id_magasin_commandes)) : null;
            setStat(stats_magasin_commandes, magasin_commandes?.ToString() ?? "N/A");

            uint? id_magasin_ca = conn.SelectSingleCell<uint?>("SELECT id_magasin, SUM(prix_avant_reduc - (prix_avant_reduc * pourc_reduc_prix / 100)) AS prix_total FROM commande" +
                " WHERE date_commande > CURDATE() - INTERVAL @duration MONTH GROUP BY id_magasin ORDER BY prix_total DESC LIMIT 1;", 0, paramDuration);
            TMagasin? magasin_ca = id_magasin_ca != null && id_magasin_ca > 0 ?
                conn.SelectSingleRecord<TMagasin>("SELECT * FROM magasin WHERE id_magasin = @id_magasin", new DbParam("@id_magasin", id_magasin_ca)) : null;
            setStat(stats_magasin_ca, magasin_ca?.ToString() ?? "N/A");

            uint? id_client = conn.SelectSingleCell<uint?>("SELECT id_client, SUM(prix_avant_reduc - (prix_avant_reduc * pourc_reduc_prix / 100)) AS prix_total FROM commande" +
                " WHERE date_commande > CURDATE() - INTERVAL @duration MONTH GROUP BY id_client ORDER BY prix_total DESC LIMIT 1;", 0, paramDuration);
            TClient? meilleur_client = id_client != null && id_client > 0 ?
                conn.SelectSingleRecord<TClient>("SELECT * FROM client WHERE id_client = @id_client", new DbParam("@id_client", id_client)) : null;
            setStat(stats_client, meilleur_client?.ToString() ?? "N/A");

            uint? id_fleur_plus = conn.SelectSingleCell<uint?>("SELECT id_produit, SUM(quantite_contient) AS cnt FROM contient WHERE id_produit IN (SELECT id_produit FROM produit" +
                " WHERE categorie_produit = 'fleur') AND id_commande IN (SELECT id_commande FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH)" +
                " GROUP BY id_produit ORDER BY cnt DESC LIMIT 1;", 0, paramDuration);
            TProduit? fleur_plus = id_fleur_plus != null && id_fleur_plus > 0 ?
                conn.SelectSingleRecord<TProduit>("SELECT * FROM produit WHERE id_produit = @id_produit", new DbParam("@id_produit", id_fleur_plus)) : null;
            setStat(stats_fleur_plus, fleur_plus?.ToString() ?? "N/A");

            uint? id_fleur_moins = conn.SelectSingleCell<uint?>("SELECT id_produit, SUM(quantite_contient) AS cnt FROM contient WHERE id_produit IN (SELECT id_produit FROM produit" +
                " WHERE categorie_produit = 'fleur') AND id_commande IN (SELECT id_commande FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH)" +
                " GROUP BY id_produit ORDER BY cnt ASC LIMIT 1;", 0, paramDuration);
            TProduit? fleur_moins = id_fleur_moins != null && id_fleur_moins > 0 ?
                conn.SelectSingleRecord<TProduit>("SELECT * FROM produit WHERE id_produit = @id_produit", new DbParam("@id_produit", id_fleur_moins)) : null;
            setStat(stats_fleur_moins, fleur_moins?.ToString() ?? "N/A");

            uint? id_acc_plus = conn.SelectSingleCell<uint?>("SELECT id_produit, SUM(quantite_contient) AS cnt FROM contient WHERE id_produit IN (SELECT id_produit FROM produit" +
                " WHERE categorie_produit = 'accessoire') AND id_commande IN (SELECT id_commande FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH)" +
                " GROUP BY id_produit ORDER BY cnt DESC LIMIT 1;", 0, paramDuration);
            TProduit? acc_plus = id_acc_plus != null && id_acc_plus > 0 ?
                conn.SelectSingleRecord<TProduit>("SELECT * FROM produit WHERE id_produit = @id_produit", new DbParam("@id_produit", id_acc_plus)) : null;
            setStat(stats_acc_plus, acc_plus?.ToString() ?? "N/A");

            uint? id_acc_moins = conn.SelectSingleCell<uint?>("SELECT id_produit, SUM(quantite_contient) AS cnt FROM contient WHERE id_produit IN (SELECT id_produit FROM produit" +
                " WHERE categorie_produit = 'accessoire') AND id_commande IN (SELECT id_commande FROM commande WHERE date_commande > CURDATE() - INTERVAL @duration MONTH)" +
                " GROUP BY id_produit ORDER BY cnt ASC LIMIT 1;", 0, paramDuration);
            TProduit? acc_moins = id_acc_moins != null && id_acc_moins > 0 ?
                conn.SelectSingleRecord<TProduit>("SELECT * FROM produit WHERE id_produit = @id_produit", new DbParam("@id_produit", id_acc_moins)) : null;
            setStat(stats_acc_moins, acc_moins?.ToString() ?? "N/A");
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

        public void Reload_Bouquets()
        {
            try
            {
                List<TBouquet> bouquets = conn.SelectMultipleRecords<TBouquet>("SELECT * FROM bouquet");
                Bouquet_DataGrid.ItemsSource = bouquets;
            } catch(Exception e)
            {
                Bouquet_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser la liste des bouquets standards :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        public bool StocksAlertsOnly { get; set; }

        public void Prepare_Stocks()
        {
            ignore_stock_selectionchanged = true;
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

                StocksAlertsOnly = false;
            }
            catch (Exception e)
            {
                Stocks_Selector_Magasin.ItemsSource = new TMagasin[] { new() { nom_magasin = "Tous les magasins" } };
                Stocks_Selector_Produit.ItemsSource = new TProduit[] { new() { nom_produit = "Tous les produits" } };
                MessageWindow.Show("Impossible de remplir les filtres des stocks :\n" +e, "Impossible de filtrer les stocks");
            }
            ignore_stock_selectionchanged = false;
        }

        public const int STOCKS_THRESHOLD = 5;

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
                    else command += " AND ";
                    hasWhere = true;
                    command += "id_produit = @produit";
                    cmdParams.Add(new("@produit", ((TProduit)Stocks_Selector_Produit.SelectedValue).id_produit));
                }

                if(StocksAlertsOnly)
                {
                    if (!hasWhere) command += " WHERE ";
                    else command += " AND ";
                    hasWhere = true;
                    command += "quantite_stock < @stocks_threshold";
                    cmdParams.Add(new("@stocks_threshold", STOCKS_THRESHOLD));
                }

                List<TStock> stocks = LoadingWindow.SelectMultipleRecords<TStock>(conn,command, cmdParams.ToArray());
                foreach (var s in stocks) s.FetchForeignReferences(conn);
                Stock_DataGrid.ItemsSource = stocks;
            } catch(Exception e)
            {
                Stock_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser les stocks :\n" + e, "Impossible d'actualiser la liste");
            }
        }

        public void Reload_Commandes()
        {
            try
            {
                List<TCommande> commandes = LoadingWindow.SelectMultipleRecords<TCommande>(conn, "SELECT * FROM commande");
                foreach(var c in commandes) c.FetchForeignReferences(conn);
                Commande_DataGrid.ItemsSource = commandes;
            }
            catch (Exception e)
            {
                Commande_DataGrid.ItemsSource = null;
                MessageWindow.Show("Impossible d'actualiser la liste des commandes :\n" + e, "Impossible d'actualiser la liste");
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
            if (e.Source is TabControl tabControl)
            {
                TabItem? tab = tabControl.SelectedItem as TabItem;
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
                    case "Bouquet_Tab":
                        Reload_Bouquets();
                        break;
                    case "Client_Tab":
                        Reload_Clients();
                        break;
                    case "Commande_Tab":
                        Reload_Commandes();
                        break;
                    case "Stats_Tab":
                        Reload_Stats();
                        break;
                    case "PasserCommande_Tab":
                        RemplirComboCommandeClient();
                        UpdateClientFidelite();
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
                uint id_magasin = CustomDataClass.GetPrimaryKey0(sender as UIElement);
                TMagasin magasin = DbRecord.CreateEmptyOrGetInstance<TMagasin>(id_magasin);

                if (MessageWindow.Show("Voulez-vous supprimer le magasin #" + id_magasin + " (" + magasin.nom_magasin + ")",
                    "Suppression d'un magasin", true, true) == MessageWindow.MessageResult.Continue)
                {
                    magasin.DeleteFrom("magasin", conn);
                    magasin.adresse_localisation?.DeleteFrom("adresse", conn);
                    Reload_Magasins();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le magasin :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Magasin_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TMagasin magasin = DbRecord.CreateEmptyOrGetInstance<TMagasin>(CustomDataClass.GetPrimaryKey0(sender as UIElement));

                AddEditMagasin addEditWindow = new(magasin);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    magasin.adresse_localisation?.Update("adresse", conn);
                    magasin.Update("magasin", conn);

                    Reload_Magasins();
                }
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
                TProduit produit = DbRecord.CreateEmptyOrGetInstance<TProduit>(CustomDataClass.GetPrimaryKey0(sender as UIElement));

                AddEditProduit addEditWindow = new(produit);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    produit.Update("produit", conn);

                    Reload_Produits();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de modifier le produit :\n" + ex, "Impossible de modifier l'élément");
            }
        }

        private void Produit_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint id_produit = CustomDataClass.GetPrimaryKey0(sender as UIElement);
                TProduit produit = DbRecord.CreateEmptyOrGetInstance<TProduit>(id_produit);

                if (MessageWindow.Show("Voulez-vous supprimer le produit #" + id_produit + " (" + produit.nom_produit + ")\n\n" +
                    "Cette action sera impossible si le produit est utilisé dans un bouquet ou une commande.",
                    "Suppression d'un produit", true, true) == MessageWindow.MessageResult.Continue)
                {
                    produit.DeleteFrom("produit", conn);
                    Reload_Produits();
                }
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
                uint id_client = CustomDataClass.GetPrimaryKey0(sender as UIElement);
                TClient client = DbRecord.CreateEmptyOrGetInstance<TClient>(id_client);

                if (MessageWindow.Show("Voulez-vous supprimer le client #" + id_client + " (" + client.prenom_client + " " + client.nom_client + ")\n\n" +
                    "Cette action sera impossible si le client a déjà passé une commande.",
                    "Suppression d'un client", true, true, false) == MessageWindow.MessageResult.Continue)
                {
                    client.DeleteFrom("client", conn);
                    client.adresse_facturation?.DeleteFrom("adresse", conn);
                    Reload_Clients();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le client :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Client_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TClient client = DbRecord.CreateEmptyOrGetInstance<TClient>(CustomDataClass.GetPrimaryKey0(sender as UIElement));

                AddEditClient addEditWindow = new(client);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    client.Update("client", conn);
                    client.adresse_facturation?.Update("adresse", conn);

                    Reload_Clients();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le client :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Stock_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TStock stock = DbRecord.CreateEmptyOrGetInstance<TStock>(CustomDataClass.GetPrimaryKey0(sender as UIElement),
                    CustomDataClass.GetPrimaryKey1(sender as UIElement));

                EditStock editWindow = new(stock);
                editWindow.ShowDialog();

                if (editWindow.Submitted)
                {
                    stock.Update("stock", conn);
                    Reload_Stocks();
                }
            } catch(Exception ex)
            {
                MessageWindow.Show("Impossible de modifier le stock :\n" + ex, "Impossible de modifier le stock");
            }
        }

        private void Stock_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Stocks();
        }

        bool ignore_stock_selectionchanged = false;
        private void Stocks_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignore_stock_selectionchanged) return;
            Reload_Stocks();
        }

        private void Bouquet_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditBouquet addEditWindow = new();
                addEditWindow.ShowDialog();
                
                TBouquet? new_bouquet = addEditWindow.value;
                if (addEditWindow.Submitted && new_bouquet != null)
                {
                    new_bouquet.InsertInto("bouquet", conn);

                    Reload_Bouquets();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible d'ajouter un bouquet :\n" + ex, "Impossible d'ajouter l'élément");
            }
        }

        private void Bouquet_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Bouquets();
        }

        private void Bouquet_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TBouquet bouquet = DbRecord.CreateEmptyOrGetInstance<TBouquet>(CustomDataClass.GetPrimaryKey0(sender as UIElement));

                AddEditBouquet addEditWindow = new(bouquet);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    bouquet.Update("bouquet", conn);

                    Reload_Bouquets();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible de modifier le bouquet standard :\n" + ex, "Impossible de modifier l'élément");
            }
        }

        private void Bouquet_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint id_bouquet = CustomDataClass.GetPrimaryKey0(sender as UIElement);
                TBouquet bouquet = DbRecord.CreateEmptyOrGetInstance<TBouquet>(id_bouquet);

                if (MessageWindow.Show("Voulez-vous supprimer le bouquet #" + id_bouquet + " (" + bouquet.nom_bouquet + ") ?",
                    "Suppression d'un bouquet standard", true, true, false) == MessageWindow.MessageResult.Continue)
                {
                    bouquet.DeleteFrom("bouquet", conn);
                    Reload_Bouquets();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer le bouquet :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Bouquet_OpenCompose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TBouquet bouquet = DbRecord.CreateEmptyOrGetInstance<TBouquet>(CustomDataClass.GetPrimaryKey0(sender as UIElement));
                ComposeBouquetSublistWindow sublistWindow = new(bouquet, conn);
                sublistWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible d'afficher les produits du bouquet standard :\n" + ex, "Impossible d'accéder à l'élément");
            }
        }

        private void Commande_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin;");
                List<TClient> clients = conn.SelectMultipleRecords<TClient>("SELECT * FROM client;");
                List<TBouquet> bouquets = conn.SelectMultipleRecords<TBouquet>("SELECT * FROM bouquet;");

                AddEditCommande addEditWindow = new(magasins, clients, bouquets);
                addEditWindow.ShowDialog();

                TCommande? new_commande = addEditWindow.value;
                if (addEditWindow.Submitted && new_commande != null && new_commande.adresse_livraison != null)
                {
                    new_commande.adresse_livraison.InsertInto("adresse", conn);
                    new_commande.id_adresse = new_commande.adresse_livraison.id_adresse;
                    new_commande.InsertInto("commande", conn);

                    Reload_Commandes();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible d'ajouter une commande :\n" + ex, "Impossible d'ajouter l'élément");
            }
        }

        private void Commande_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload_Commandes();
        }

        private void Commande_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TCommande commande = DbRecord.CreateEmptyOrGetInstance<TCommande>(CustomDataClass.GetPrimaryKey0(sender as UIElement));

                List<TMagasin> magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin;");
                List<TClient> clients = conn.SelectMultipleRecords<TClient>("SELECT * FROM client;");
                List<TBouquet> bouquets = conn.SelectMultipleRecords<TBouquet>("SELECT * FROM bouquet;");

                AddEditCommande addEditWindow = new(magasins, clients, bouquets, commande);
                addEditWindow.ShowDialog();

                if (addEditWindow.Submitted)
                {
                    commande.Update("commande", conn);

                    Reload_Commandes();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible de modifier la commande :\n" + ex, "Impossible de modifier l'élément");

            }
        }

        private void Commande_Delete_Click(object sender, RoutedEventArgs e)
        {   
            try
            {
                uint id_commande = CustomDataClass.GetPrimaryKey0(sender as UIElement);
                TCommande commande = DbRecord.CreateEmptyOrGetInstance<TCommande>(id_commande);

                if (MessageWindow.Show("Voulez-vous supprimer la commande #" + id_commande + " ?",
                    "Suppression d'une commande", true, true, false) == MessageWindow.MessageResult.Continue)
                {
                    commande.DeleteFrom("commande", conn);
                    commande.adresse_livraison?.DeleteFrom("adresse", conn);
                    Reload_Commandes();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible de supprimer la commande :\n" + ex, "Impossible de supprimer l'élément");
            }
        }

        private void Commande_OpenContient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TCommande commande = DbRecord.CreateEmptyOrGetInstance<TCommande>(CustomDataClass.GetPrimaryKey0(sender as UIElement));
                ContientCommandeSublistWindow sublistWindow = new(commande, conn);
                sublistWindow.ShowDialog();
                Reload_Commandes();
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Impossible d'afficher les produits de la commande :\n" + ex, "Impossible d'accéder à l'élément");
            }
        }

        private void OpenExport_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                string name = button.Name;
                if(name.StartsWith("OpenExport_"))
                {
                    switch(name.Substring(11))
                    {
                        case "Magasin":
                            ExportDataWindow.Open<TMagasin>(Magasin_DataGrid.ItemsSource);
                            break;
                        case "Produit":
                            ExportDataWindow.Open<TProduit>(Produit_DataGrid.ItemsSource);
                            break;
                        case "Stock":
                            ExportDataWindow.Open<TStock>(Stock_DataGrid.ItemsSource);
                            break;
                        case "Bouquet":
                            ExportDataWindow.Open<TBouquet>(Bouquet_DataGrid.ItemsSource);
                            break;
                        case "Client":
                            ExportDataWindow.Open<TClient>(Client_DataGrid.ItemsSource);
                            break;
                        case "Commande":
                            ExportDataWindow.Open<TCommande>(Commande_DataGrid.ItemsSource);
                            break;
                        default:
                            MessageWindow.Show("Type de données à exporter invalide !", "FleuristeVirtuel");
                            break;
                    }
                }
            }
        }

        private void stats_period_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reload_Stats();
        }

        private void stats_export_xml_Click(object sender, RoutedEventArgs e)
        {
            List<TClient> list = conn.SelectMultipleRecords<TClient>("SELECT client.*, COUNT(id_commande) AS cnt FROM client" +
                " JOIN commande ON commande.id_client = client.id_client WHERE date_commande > CURDATE() - INTERVAL 1 MONTH" +
                " GROUP BY commande.id_client HAVING cnt > 1;");

            ExportDataWindow.Export<TClient>(list, "xml");
        }

        private void stats_export_json_Click(object sender, RoutedEventArgs e)
        {
            List<TClient> list = conn.SelectMultipleRecords<TClient>("SELECT client.* FROM client LEFT JOIN commande ON client.id_client = commande.id_client" +
                " AND commande.date_commande > CURDATE() - INTERVAL 6 MONTH WHERE commande.id_client IS NULL;");

            ExportDataWindow.Export<TClient>(list, "json");
        }

        uint reduc_actuelle = 0;

        private TCommande? RecupBaseCommandeClient()
        {
            TMagasin? _magasin = (TMagasin)cclient_magasin.SelectedItem;
            if (_magasin == null)
            {
                MessageWindow.Show("Merci de choisir un magasin dans la liste", "Donné invalide");
                return null;
            }

            DateTime? _date = cclient_datelivraison.SelectedDate;
            if(_date == null)
            {
                MessageWindow.Show("Merci de choisir une date de livraison", "Donné invalide");
                return null;
            }

            if(!uint.TryParse(cclient_addr_numero.Text, out uint _numero))
            {
                MessageWindow.Show("Merci d'indiquer un numéro entier positif pour le numéro de rue", "Donné invalide");
                return null;
            }

            string _rue = cclient_addr_rue.Text.Trim();
            if(_rue.Length == 0)
            {
                MessageWindow.Show("Merci d'indiquer un nom de rue", "Donné invalide");
                return null;
            }

            if (!uint.TryParse(cclient_addr_cp.Text, out uint _cp) || _cp >= 100_000)
            {
                MessageWindow.Show("Merci d'indiquer un code postal valide", "Donné invalide");
                return null;
            }

            string _ville = cclient_addr_ville.Text.Trim();
            if (_ville.Length == 0)
            {
                MessageWindow.Show("Merci d'indiquer une ville", "Donné invalide");
                return null;
            }

            string _acc = cclient_msg_accompagnement.Text.Trim();
            string _commentaire = cclient_commentaire.Text.Trim();

            TAdresse addr = new()
            {
                numero_rue = _numero,
                nom_rue = _rue,
                code_postal = _cp,
                ville = _ville
            };

            return new()
            {
                id_magasin = _magasin.id_magasin,
                date_commande = DateTime.Today,
                adresse_livraison = addr,
                date_livraison_souhaitee = _date,
                message_accompagnement = _acc,
                commentaire_commande = _commentaire,
                pourc_reduc_prix = reduc_actuelle,
                id_client = conn.SelectSingleCell<uint>("SELECT id_client FROM client WHERE email_client = @email", 0, new DbParam("@email", currentUser))
            };
        }

        private void client_valider_commande_perso_Click(object sender, RoutedEventArgs e)
        {
            TCommande? commande = RecupBaseCommandeClient();
            if (commande == null) return;
        }

        private void client_valider_commande_standard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TCommande? commande = RecupBaseCommandeClient();
                if (commande == null) return;

                TBouquet? bouquet = (TBouquet)cclient_bouquet.SelectedItem;
                if (bouquet == null)
                {
                    MessageWindow.Show("Merci de choisir un bouquet!", "Donnée invalide");
                    return;
                }

                commande.id_bouquet_base = bouquet.id_bouquet;

                if (commande.date_livraison_souhaitee < DateTime.Today.AddDays(3)) commande.statut = "VINV";
                else commande.statut = "CC";

                commande.adresse_livraison?.InsertInto("adresse", conn);
                commande.prix_avant_reduc = bouquet.prix_bouquet;
                commande.id_adresse = commande.adresse_livraison?.id_adresse ?? throw new Exception("Impossible de sauvegarder la commande!");
                commande.InsertInto("commande", conn);

                List<TCompose> composition = conn.SelectMultipleRecords<TCompose>("SELECT * FROM compose WHERE id_bouquet = @bouquet", new DbParam("@bouquet", bouquet.id_bouquet));
                foreach(TCompose compositionItem in composition)
                {
                    TContient contient = DbRecord.CreateEmptyOrGetInstance<TContient>(commande.id_commande, compositionItem.id_produit);
                    contient.quantite_contient = compositionItem.quantite_compose;
                    contient.InsertInto("contient", conn, true);
                }

                CommandeValidee();
            } catch(Exception er)
            {
                MessageWindow.Show("Impossible de passer la commande : " + er, "Erreur de commande");
            }
        }

        private void RemplirComboCommandeClient()
        {
            cclient_magasin.ItemsSource = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin");
            cclient_bouquet.ItemsSource = conn.SelectMultipleRecords<TBouquet>("SELECT * FROM bouquet");

            cclient_datelivraison.DisplayDateStart = DateTime.Today;
        }

        private void UpdateClientFidelite()
        {
            uint userId = conn.SelectSingleCell<uint>("SELECT id_client FROM client WHERE email_client = @email", 0, new DbParam("@email", currentUser));
            int lastMonthCount = conn.SelectSingleCell<int>("SELECT COUNT(*) FROM commande WHERE id_client = @client AND date_commande > CURDATE() - INTERVAL 1 MONTH;", 0, new DbParam("@client", userId));

            if(lastMonthCount >= 5)
            {
                reduc_actuelle = 15;
                setStat(cclient_fidelite, "Fidélité or (15% de réduction)");
            } else if(lastMonthCount >= 1)
            {
                reduc_actuelle = 5;
                setStat(cclient_fidelite, "Fidélité bronze (5% de réduction)");
            } else
            {
                reduc_actuelle = 0;
                setStat(cclient_fidelite, "Aucune, passez plus de commandes pour y arriver");
            }
        }

        private void ClearClientCommandeForm()
        {
            cclient_magasin.SelectedItem = null;
            cclient_datelivraison.SelectedDate = null;
            cclient_addr_numero.Text = "";
            cclient_addr_rue.Text = "";
            cclient_addr_cp.Text = "";
            cclient_addr_ville.Text = "";
            cclient_msg_accompagnement.Text = "";
            cclient_commentaire.Text = "";
            cclient_bouquet.SelectedItem = null;

            UpdateClientFidelite();
        }

        private void CommandeValidee()
        {
            ClearClientCommandeForm();
            MessageWindow.Show("Votre commande a bien été enregistrée !\nMerci de votre confiance", "Commande validée");
        }

        private void cclient_datelivraison_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = cclient_datelivraison.SelectedDate;
            if(selectedDate != null)
            {
                if(selectedDate < DateTime.Today.AddDays(3))
                {
                    MessageWindow.Show("Attention, nous ne pouvons pas garantir les livraisons à moins de 3j !", "Date de livraison");
                }
            }
        }

        private void cclient_bouquet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TBouquet? bouquet = (TBouquet)cclient_bouquet.SelectedItem;
            if(bouquet == null)
            {
                cclient_bouquet_text.Text = "Vous n'avez pas encore choisi de bouquet !";
            } else
            {
                cclient_bouquet_text.Text = $"Bouquet choisi : {bouquet.nom_bouquet}           Prix : {bouquet.prix_bouquet} €";
            }
        }
    }

    public static class CustomDataClass
    {

        public static readonly DependencyProperty PrimaryKey0_property = DependencyProperty.RegisterAttached("PrimaryKey0",
            typeof(uint), typeof(CustomDataClass), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PrimaryKey1_property = DependencyProperty.RegisterAttached("PrimaryKey1",
            typeof(uint), typeof(CustomDataClass), new FrameworkPropertyMetadata(null));

        public static uint GetPrimaryKey0(UIElement? element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (uint)element.GetValue(PrimaryKey0_property);
        }
        public static void SetPrimaryKey0(UIElement? element, uint value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(PrimaryKey0_property, value);
        }

        public static uint GetPrimaryKey1(UIElement? element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (uint)element.GetValue(PrimaryKey1_property);
        }
        public static void SetPrimaryKey1(UIElement? element, uint value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(PrimaryKey1_property, value);
        }
    }

    public enum LoggedInStatus
    {
        None, Client, Admin
    }
}
