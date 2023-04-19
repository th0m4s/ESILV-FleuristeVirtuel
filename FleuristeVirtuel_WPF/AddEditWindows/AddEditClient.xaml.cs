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
    /// Interaction logic for AddEditClient.xaml
    /// </summary>
    public partial class AddEditClient : GUIWindow
    {
        public TClient? value;

        public bool Submitted { get; private set; }

        public AddEditClient(TClient? editValue = null)
        {
            this.value = editValue;
            InitializeComponent();

            if(value != null)
            {
                Title = "Modification du client #" + value.id_client;

                nom_client.Text = value.nom_client;
                prenom_client.Text = value.prenom_client;
                email_client.Text = value.email_client;
                carte_de_credit.Text = value.carte_de_credit;
                nom_client.Text = value.nom_client;
                mot_de_passe_help.Visibility = Visibility.Visible;


                numero_rue.Text = "" + value.adresse_facturation?.numero_rue;
                nom_rue.Text = value.adresse_facturation?.nom_rue;
                code_postal.Text = "" + value.adresse_facturation?.code_postal;
                ville.Text = value.adresse_facturation?.ville;
            } else
            {
                Title = "Ajout d'un client";
                mot_de_passe_help.Visibility = Visibility.Collapsed;
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            if(nom_client.Text.Trim().Length == 0)
            {
                MessageBox.Show("Le nom du client ne peut pas être vide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if (prenom_client.Text.Trim().Length == 0)
            {
                MessageBox.Show("Le prénom du client ne peut pas être vide", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if (!email_client.Text.Contains("@"))
            {
                MessageBox.Show("L'email du client doit contenir '@'", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if (carte_de_credit.Text.Trim().Length == 0)
            {
                MessageBox.Show("La carte du crédit du client doit être renseignée'", "Donnée invalide", MessageBoxButton.OK);
                return;
            }

            if (nom_rue.Text.Trim().Length == 0)
            {
                MessageBox.Show("La rue de l'adresse du client ne peut être vide", "Donné invalide", MessageBoxButton.OK);
                return;
            }

            if (ville.Text.Trim().Length == 0)
            {
                MessageBox.Show("La ville de l'adresse du client ne peut être vide", "Donné invalide", MessageBoxButton.OK);
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

            if (value == null)
            {
                value = new();
                value.adresse_facturation = new();
            }

            if (value.adresse_facturation == null)
                throw new Exception("Invalid data state!");

            value.nom_client = nom_client.Text;
            value.prenom_client = prenom_client.Text;
            value.email_client = email_client.Text;
            value.carte_de_credit = carte_de_credit.Text;

            string newPass = mot_de_passe.Password.Trim();
            if(newPass.Length > 0)
                value.mot_de_passe = newPass;

            value.adresse_facturation.numero_rue = numero_rue_val;
            value.adresse_facturation.nom_rue = nom_rue.Text;
            value.adresse_facturation.code_postal = code_postal_val;
            value.adresse_facturation.ville = ville.Text;
            
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
