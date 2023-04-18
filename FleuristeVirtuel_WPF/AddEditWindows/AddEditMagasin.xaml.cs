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
    /// Interaction logic for AddEditMagasin.xaml
    /// </summary>
    public partial class AddEditMagasin : GUIWindow
    {
        public TMagasin? value;

        public bool Submitted { get; private set; }

        public AddEditMagasin(TMagasin? editValue = null)
        {
            this.value = editValue;
            InitializeComponent();

            if(value != null)
            {
                Title = "Modification du magasin #" + value.id_magasin;

                nom_magasin.Text = value.nom_magasin;
                numero_rue.Text = "" + value.adresse_localisation?.numero_rue;
                nom_rue.Text = value.adresse_localisation?.nom_rue;
                code_postal.Text = "" + value.adresse_localisation?.code_postal;
                ville.Text = value.adresse_localisation?.ville;
            } else
            {
                Title = "Ajout d'un magasin";
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            if (value == null)
            {
                value = new();
                value.adresse_localisation = new();
            }

            if (value.adresse_localisation == null)
                throw new Exception("Invalid data state!");

            if(nom_magasin.Text.Trim().Length == 0)
            {
                MessageBox.Show("Le nom du magasin ne peut pas être vide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if(nom_rue.Text.Trim().Length == 0)
            {
                MessageBox.Show("La rue de l'adresse du magasin ne peut être vide", "Donné invalide", MessageBoxButton.OK);
                return;
            }

            if (ville.Text.Trim().Length == 0)
            {
                MessageBox.Show("La ville de l'adresse du magasin ne peut être vide", "Donné invalide", MessageBoxButton.OK);
                return;
            }

            if(!uint.TryParse(numero_rue.Text, out uint numero_rue_val))
            {
                MessageBox.Show("Le numéro de rue de l'adresse n'est pas valide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if(!uint.TryParse(code_postal.Text, out uint code_postal_val))
            {
                MessageBox.Show("Le code postal de l'adresse n'est pas valide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            value.nom_magasin = nom_magasin.Text;
            value.adresse_localisation.numero_rue = numero_rue_val;
            value.adresse_localisation.nom_rue = nom_rue.Text;
            value.adresse_localisation.code_postal = code_postal_val;
            value.adresse_localisation.ville = ville.Text;
            
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
