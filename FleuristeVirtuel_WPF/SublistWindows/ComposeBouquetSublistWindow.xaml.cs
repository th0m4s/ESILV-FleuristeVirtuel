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
    /// Interaction logic for ComposeBouquetSublistWindow.xaml
    /// </summary>
    public partial class ComposeBouquetSublistWindow : GUIWindow
    {
        DbConnection conn;

        TBouquet bouquet;
        List<TCompose> composition;

        public ComposeBouquetSublistWindow(TBouquet bouquet, DbConnection conn)
        {
            this.bouquet = bouquet;
            this.conn = conn;

            List<TCompose> composition = conn.SelectMultipleRecords<TCompose>("SELECT * FROM compose WHERE id_bouquet = @bouquet",
                new DbParam("@bouquet", bouquet.id_bouquet));
            foreach (var compose in composition) compose.FetchForeignReferences(conn);
            this.composition = composition;

            InitializeComponent();

            bouquet_info.Text = $"Bouquet #{bouquet.id_bouquet} : {bouquet.nom_bouquet} (cat. {bouquet.categorie_bouquet})";
            Compose_DataGrid.ItemsSource = composition;

            UpdateOtherProducts();
        }

        /// <summary>
        /// Refreshes the products combobox to only include products not currently in the datagrid
        /// </summary>
        private void UpdateOtherProducts()
        {
            List<TProduit> produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit AS p WHERE p.id_produit NOT IN " +
                "(SELECT c.id_produit FROM compose AS c WHERE id_bouquet = @bouquet)", new DbParam("@bouquet", bouquet.id_bouquet));

            produit_combobox.ItemsSource = produits;
        }

        private void produit_add_Click(object sender, RoutedEventArgs e)
        {
            TProduit? produit = produit_combobox.SelectedItem as TProduit;
            if (produit == null) return;

            TCompose compose = DbRecord.CreateEmptyOrGetInstance<TCompose>(produit.id_produit, bouquet.id_bouquet);
            compose.quantite_compose = 1;
            compose.InsertInto("compose", conn, true);
            compose.FetchForeignReferences(conn);

            composition.Add(compose);
            Compose_DataGrid.Items.Refresh();

            UpdateOtherProducts();
        }

        private void Compose_DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            TCompose? compose = e.Row.Item as TCompose;
            if (compose == null) return;

            uint oldVal = compose.quantite_compose;
            if(e.EditingElement is TextBox tb && uint.TryParse(tb.Text, out uint newVal))
            {
                if(newVal != oldVal)
                {
                    if(newVal == 0)
                    {
                        composition.DeleteAndRemove(compose, "compose", conn);

                        // impossible de faire Compose_DataGrid.Items.Refresh(); car nous sommes dans un évènement qui édite déjà une case
                        e.Cancel = true;
                        Compose_DataGrid.ItemsSource = null;
                        Compose_DataGrid.ItemsSource = composition;

                        UpdateOtherProducts();
                    }
                    else
                    {
                        compose.quantite_compose = newVal;
                        compose.Update("compose", conn);
                    }
                }
            }
        }
    }
}
