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

namespace FleuristeVirtuel_WPF
{
    /// <summary>
    /// Interaction logic for ContientCommandeSublistWindow.xaml
    /// </summary>
    public partial class ContientCommandeSublistWindow : GUIWindow
    {
        DbConnection conn;

        TCommande commande;
        List<TContient> produitsContenus;

        public ContientCommandeSublistWindow(TCommande commande, DbConnection conn)
        {
            this.commande = commande;
            this.conn = conn;

            List<TContient> produitsContenus = conn.SelectMultipleRecords<TContient>("SELECT * FROM contient WHERE id_commande = @commande",
                new DbParam("@commande", commande.id_commande));
            foreach (var contient in produitsContenus) contient.FetchForeignReferences(conn);
            this.produitsContenus = produitsContenus;

            InitializeComponent();

            commande_info.Text = $"Commande #{commande.id_commande} passée par {commande.client}";
            if (commande.prix_maximum != null && commande.prix_maximum > 0) commande_info.Text += $" (prix maximum : {commande.prix_maximum}€)";
            Contient_DataGrid.ItemsSource = produitsContenus;

            price_input.Text = "" + commande.prix_avant_reduc;

            UpdateOtherProducts();
            UpdateBouquets();
        }

        /// <summary>
        /// Refreshes the products combobox to only include products not currently in the datagrid
        /// </summary>
        private void UpdateOtherProducts()
        {
            List<TProduit> produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit AS p WHERE p.id_produit NOT IN " +
                "(SELECT c.id_produit FROM contient AS c WHERE id_commande = @commande)", new DbParam("@commande", commande.id_commande));

            produit_combobox.ItemsSource = produits;
        }

        /// <summary>
        /// Refreshes the list of bouquets in the combobox
        /// </summary>
        private void UpdateBouquets()
        {
            List<TBouquet> bouquets = conn.SelectMultipleRecords<TBouquet>("SELECT * FROM bouquet");
            bouquet_combobox.ItemsSource = bouquets;
        }

        private void produit_add_Click(object sender, RoutedEventArgs e)
        {
            TProduit? produit = produit_combobox.SelectedItem as TProduit;
            if (produit == null) return;

            TContient contient = DbRecord.CreateEmptyOrGetInstance<TContient>(commande.id_commande, produit.id_produit);
            contient.quantite_contient = 1;
            contient.InsertInto("contient", conn, true);
            contient.FetchForeignReferences(conn);

            produitsContenus.Add(contient);
            Contient_DataGrid.Items.Refresh();

            UpdateOtherProducts();
        }

        private void Contient_DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            TContient? contient = e.Row.Item as TContient;
            if (contient == null) return;

            uint oldVal = contient.quantite_contient;
            if(e.EditingElement is TextBox tb && uint.TryParse(tb.Text, out uint newVal))
            {
                if(newVal != oldVal)
                {
                    if(newVal == 0)
                    {
                        produitsContenus.DeleteAndRemove(contient, "contient", conn);

                        // impossible de faire Compose_DataGrid.Items.Refresh(); car nous sommes dans un évènement qui édite déjà une case
                        e.Cancel = true;
                        Contient_DataGrid.ItemsSource = null;
                        Contient_DataGrid.ItemsSource = produitsContenus;

                        UpdateOtherProducts();
                    }
                    else
                    {
                        contient.quantite_contient = newVal;
                        contient.Update("contient", conn);
                    }
                }
            }
        }

        private void use_bouquet_Click(object sender, RoutedEventArgs e)
        {
            TBouquet? bouquet = bouquet_combobox.SelectedItem as TBouquet;
            if(bouquet != null)
            {
                if(MessageWindow.Show("Voulez vous vraiment remplacer tous les produits de la commande actuelle par ceux du bouquet " + bouquet.nom_bouquet + " ?",
                    "Confirmation", true, true) == MessageWindow.MessageResult.Continue)
                {
                    try
                    {
                        foreach (TContient contient in produitsContenus)
                            contient.DeleteFrom("contient", conn);

                        List<TCompose> composition = conn.SelectMultipleRecords<TCompose>("SELECT * FROM compose WHERE id_bouquet = @bouquet;",
                            new DbParam("@bouquet", bouquet.id_bouquet));

                        List<TContient> nouveauContient = new();
                        foreach (TCompose comp in composition)
                        {
                            TContient contient = DbRecord.CreateEmptyOrGetInstance<TContient>(commande.id_commande, comp.id_produit);
                            contient.FetchForeignReferences(conn);
                            contient.quantite_contient = comp.quantite_compose;
                            nouveauContient.Add(contient);
                            contient.InsertInto("contient", conn, true);
                        }

                        commande.prix_avant_reduc = bouquet.prix_bouquet;
                        commande.id_bouquet_base = bouquet.id_bouquet;
                        commande.bouquet_base = bouquet;
                        commande.Update("commande", conn);

                        price_input.Text = "" + bouquet.prix_bouquet;

                        produitsContenus = nouveauContient;
                        Contient_DataGrid.ItemsSource = nouveauContient;

                        UpdateOtherProducts();

                        bouquet_combobox.SelectedIndex = -1;

                        MessageWindow.Show("La commande a été modifiée!", "Commande modifiée");
                    } catch(Exception err)
                    {
                        MessageWindow.Show("Une erreur est survenue lors de la modification de la commande : " + err, "Erreur");
                    }
                }
            }
        }

        private void auto_update_price_Click(object sender, RoutedEventArgs e)
        {
            float price = 0;

            foreach (TContient contient in produitsContenus)
                if(contient.produit != null)
                    price += contient.quantite_contient * contient.produit.prix_produit;

            commande.prix_avant_reduc = price;
            commande.Update("commande", conn);

            price_input.Text = "" + price;

            MessageWindow.Show($"La commande coûte désormais {price} € (hors réduction).", "Prix modifié");
        }

        private void manual_price_button_Click(object sender, RoutedEventArgs e)
        {
            if(float.TryParse(price_input.Text, out float price) && price >= 0)
            {
                commande.prix_avant_reduc = price;
                commande.Update("commande", conn);

                price_input.Text = "" + price;

                MessageWindow.Show($"La commande coûte désormais {price}€ (hors réduction).", "Prix modifié");
            } else
            {
                MessageWindow.Show("Le prix n'est pas au bon format. Merci de renseigner un nombre flottant positif!", "Prix invalide");
            }
        }
    }
}
