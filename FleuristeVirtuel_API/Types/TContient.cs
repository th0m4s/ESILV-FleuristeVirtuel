using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TContient : DbRecord
    {
        [PrimaryKey(0)]
        [DbColumn]
        public uint id_commande { get; private set; }

        [PrimaryKey(1)]
        [DbColumn]
        public uint id_produit { get; private set; }

        [DbColumn]
        public uint quantite_contient { get; set; }


        [ForeignReference("commande", "id_commande", "id_commande")]
        public TCommande? commande { get; set; }

        [ForeignReference("produit", "id_produit", "id_produit")]
        public TProduit? produit { get; set; }
    }
}
