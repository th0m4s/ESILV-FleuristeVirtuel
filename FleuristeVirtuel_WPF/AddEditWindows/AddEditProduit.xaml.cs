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
    /// Interaction logic for AddEditProduit.xaml
    /// </summary>
    public partial class AddEditProduit : GUIWindow
    {
        public TProduit? value;

        public bool Submitted { get; private set; }

        public AddEditProduit(TProduit? editValue = null)
        {
            this.value = editValue;
            InitializeComponent();
            categorie_produit.ItemsSource = new string[] { "fleur", "accessoire" };

            if(value != null)
            {
                Title = "Modification du produit #" + value.id_produit;

                nom_produit.Text = value.nom_produit;
                prix_produit.Text = "" + value.prix_produit;
                categorie_produit.Text = value.categorie_produit;
            } else
            {
                Title = "Ajout d'un produit";
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            if(!float.TryParse(prix_produit.Text, out float parsedPrice) || parsedPrice < 0)
            {
                MessageBox.Show("Le prix n'est pas un nombre positif valide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if(nom_produit.Text.Trim().Length == 0)
            {
                MessageBox.Show("Le nom du produit ne peut pas être vide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if(value == null)
            {
                value = new();
            }

            value.nom_produit = nom_produit.Text;
            value.prix_produit = parsedPrice;
            value.categorie_produit = categorie_produit.Text;
            
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
