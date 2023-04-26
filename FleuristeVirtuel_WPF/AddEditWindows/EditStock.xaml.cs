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
    /// Interaction logic for EditStock.xaml
    /// </summary>
    public partial class EditStock : GUIWindow
    {
        public TStock value;

        public bool Submitted { get; private set; }

        public EditStock(TStock editValue)
        {
            this.value = editValue;
            InitializeComponent();

            if (value.produit == null || value.magasin == null)
                throw new Exception("Cannot edit invalid stock!");

            details_text.Text = $"Stock du produit {value.produit.nom_produit} (#{value.produit.id_produit})\ndans le magasin " +
                $"{value.magasin.nom_magasin} (#{value.magasin.id_magasin})";
            quantite_stock.Text = "" + value.quantite_stock;
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = true;

            if(!uint.TryParse(quantite_stock.Text, out uint quantite))
            {
                MessageWindow.Show("La quantité doit être un nombre positif ou nul", "Donnée invalide");
                return;
            }

            value.quantite_stock = quantite;
            
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Submitted = false;
            Close();
        }
    }
}
