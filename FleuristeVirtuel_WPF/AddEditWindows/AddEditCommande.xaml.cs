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
    /// Interaction logic for AddEditCommande.xaml
    /// </summary>
    public partial class AddEditCommande : GUIWindow
    {
        public TCommande? value;

        public bool Submitted { get; private set; }

        public AddEditCommande(List<TMagasin> magasins, List<TClient> clients, TCommande? editValue = null)
        {
            this.value = editValue;
            InitializeComponent();

            statut_commande.ItemsSource = CommandeStatutConverter.STATUTS.Keys;
            magasin.ItemsSource = magasins;
            client.ItemsSource = clients;

            if(value != null)
            {
                Title = "Modification de la commande #" + value.id_commande;

                statut_commande.SelectedValue = value.statut;
                message_accompagnement.Text = value.message_accompagnement;
                commentaire_commande.Text = value.commentaire_commande;
                magasin.SelectedValue = value.magasin;
                prix_maximum.Text = "" + value.prix_maximum;
                prix_avant_reduc.Text = "" + value.prix_avant_reduc;
                pourc_reduc_prix.Text = "" + value.pourc_reduc_prix;
                date_commande.SelectedDate = value.date_commande;
                date_livraison_souhaitee.SelectedDate = value.date_livraison_souhaitee;
                client.SelectedValue = value.client;
                numero_rue.Text = "" + value.adresse_livraison?.numero_rue;
                nom_rue.Text = value.adresse_livraison?.nom_rue;
                code_postal.Text = "" + value.adresse_livraison?.code_postal;
                ville.Text = value.adresse_livraison?.ville;
            } else
            {
                Title = "Création d'une commande";
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            uint? parsed_prix_maximum = null;
            if(prix_maximum.Text.Length != 0)
            {
                if (!uint.TryParse(prix_maximum.Text, out uint prix_max) || parsed_prix_maximum < 0)
                {
                    MessageWindow.Show("Le prix maximum n'est pas un nombre entier strictement positif.\nMerci de ne rien écrire si le prix maximum n'est pas applicable.", "Donnée invalide");
                    return;
                }
                else parsed_prix_maximum = prix_max;
            }

            if (!float.TryParse(prix_avant_reduc.Text, out float parsed_prix_avant_reduc) || parsed_prix_avant_reduc < 0)
            {
                MessageWindow.Show("Le prix avant réduction n'est pas un nombre positif valide", "Donnée invalide");
                return;
            }

            uint parsed_reduc = 0;
            if(pourc_reduc_prix.Text.Length != 0)
            {
                if(!uint.TryParse(pourc_reduc_prix.Text, out parsed_reduc) || parsed_reduc < 0 || parsed_reduc > 100)
                {
                    MessageWindow.Show("Le pourcentage de réduction doit être un nombre entier compris entre 0 et 100.", "Donné invalide");
                    return;
                }
            }

            if (nom_rue.Text.Trim().Length == 0)
            {
                MessageWindow.Show("La rue de l'adresse du client ne peut être vide", "Donnée invalide");
                return;
            }

            if (ville.Text.Trim().Length == 0)
            {
                MessageWindow.Show("La ville de l'adresse du client ne peut être vide", "Donnée invalide");
                return;
            }

            if (!uint.TryParse(numero_rue.Text, out uint parsed_numero_rue))
            {
                MessageWindow.Show("Le numéro de rue de l'adresse n'est pas valide", "Donnée invalide");
                return;
            }

            if (!uint.TryParse(code_postal.Text, out uint parsed_code_postal))
            {
                MessageWindow.Show("Le code postal de l'adresse n'est pas valide", "Donnée invalide");
                return;
            }

            if ((((string)statut_commande.SelectedValue) ?? "").Trim().Length == 0)
            {
                MessageWindow.Show("Merci de renseigner un statut!", "Donnée invalide");
                return;
            }

            if (client.SelectedItem == null)
            {
                MessageWindow.Show("Merci de choisir un client qui passe cette commande!", "Donnée invalide");
                return;
            }

            if (magasin.SelectedItem == null)
            {
                MessageWindow.Show("Merci de choisir un magasin affecté!", "Donnée invalide");
                return;
            }

            if (date_commande.SelectedDate == null)
            {
                MessageWindow.Show("Merci de renseigner la date de la commande!", "Donnée invalide");
                return;
            }

            if (date_livraison_souhaitee.SelectedDate == null)
            {
                MessageWindow.Show("Merci de renseigner la date de livraison souhaitée!", "Donnée invalide");
                return;
            }

            if (value == null)
            {
                value = new();
                value.adresse_livraison = new();
            }

            if (value.adresse_livraison == null)
                throw new Exception("Invalid data state!");

            value.statut = statut_commande.SelectedValue as string;
            value.message_accompagnement = message_accompagnement.Text;
            value.commentaire_commande = commentaire_commande.Text;
            value.magasin = (TMagasin)magasin.SelectedValue;
            value.id_magasin = value.magasin.id_magasin;
            value.prix_maximum = parsed_prix_maximum;
            value.prix_avant_reduc = parsed_prix_avant_reduc;
            value.pourc_reduc_prix = parsed_reduc;
            value.date_commande = date_commande.SelectedDate;
            value.date_livraison_souhaitee = date_livraison_souhaitee.SelectedDate;
            value.client = (TClient)client.SelectedValue;
            value.id_client = value.client.id_client;
            value.adresse_livraison.numero_rue = parsed_numero_rue;
            value.adresse_livraison.nom_rue = nom_rue.Text;
            value.adresse_livraison.code_postal = parsed_code_postal;
            value.adresse_livraison.ville = ville.Text;

            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
