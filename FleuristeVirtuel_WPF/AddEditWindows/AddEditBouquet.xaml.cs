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
    /// Interaction logic for AddEditBouquet.xaml
    /// </summary>
    public partial class AddEditBouquet : GUIWindow
    {
        public TBouquet? value;

        public bool Submitted { get; private set; }

        public AddEditBouquet(TBouquet? editValue = null)
        {
            this.value = editValue;
            InitializeComponent();

            if(value != null)
            {
                Title = "Modification du bouquet standard #" + value.id_bouquet;

                nom_bouquet.Text = value.nom_bouquet;
                prix_bouquet.Text = "" + value.prix_bouquet;
                categorie_bouquet.Text = value.categorie_bouquet;
                desc_bouquet.Text = value.desc_bouquet;
            } else
            {
                Title = "Ajout d'un bouquet standard";
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            if(!float.TryParse(prix_bouquet.Text, out float parsedPrice) || parsedPrice < 0)
            {
                MessageWindow.Show("Le prix n'est pas un nombre positif valide", "Donnée invalide");
                return;
            }

            if(nom_bouquet.Text.Trim().Length == 0)
            {
                MessageWindow.Show("Le nom du bouquet ne peut pas être vide", "Donnée invalide");
                return;
            }

            if (categorie_bouquet.Text.Trim().Length == 0)
            {
                MessageWindow.Show("La catégorie du bouquet ne peut pas être vide", "Donnée invalide");
                return;
            }

            if (value == null)
            {
                value = new();
            }

            value.nom_bouquet = nom_bouquet.Text;
            value.prix_bouquet = parsedPrice;
            value.categorie_bouquet = categorie_bouquet.Text;
            value.desc_bouquet = desc_bouquet.Text;

            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
