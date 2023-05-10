using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FleuristeVirtuel_WPF
{
    public class DbRecordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value == null ? "N/A" : value.ToString() + "€" ?? "0€";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string str)
            {
                return str.ToLower();
            }
            else return value;
        }
    }

    public class ProduitNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is TProduit produit)
            {
                return produit.nom_produit ?? value;
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class MagasinNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is TMagasin magasin)
            {
                return magasin.nom_magasin ?? value;
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class BouquetNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is TBouquet bouquet)
            {
                return bouquet.nom_bouquet ?? value;
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string str)
            {
                switch (str)
                {
                    case "fleur":
                        return "Fleur";
                    case "accessoire":
                        return "Accessoire";
                }
            }

            return value?.ToString()?.ToLower() ?? "null";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class SingleLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string str)
            {
                return str.Replace("\n", " ").Replace("\r", "");
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is DateTime date)
            {
                return date.ToShortDateString();
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class CommandePriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is TCommande commande)
            {
                if (commande.pourc_reduc_prix == 0)
                    return commande.prix_avant_reduc + "€";
                else return $"{commande.prix_avant_reduc * (1 - commande.pourc_reduc_prix / 100f)}€" +
                        $" (-{commande.pourc_reduc_prix}% de {commande.prix_avant_reduc}€)";
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class CommandeStatutConverter : IValueConverter
    {
        public static Dictionary<string, string> STATUTS = new() {
            { "VINV", "Vérification de l'inventaire requise" },
            { "CC", "Commande complète (à préparer)" },
            { "CPAV", "Commande personnalisée à vérifier" },
            { "CAL", "Commande à livrer (préparée)" },
            { "CL", "Commande livrée" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string str)
                return STATUTS.GetValueOrDefault(str, str);
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class DataGridPositiveIntColumn : DataGridTextColumn
    {
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            TextBox? edit = editingElement as TextBox;

            if (edit != null) edit.PreviewTextInput += OnPreviewTextInput;

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int value;

            if (!int.TryParse(e.Text, out value) || value < 0)
                e.Handled = true;
        }
    }    
}
