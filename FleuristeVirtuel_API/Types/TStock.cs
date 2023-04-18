using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TStock : DbRecord
    {
        [PrimaryKey(0)]
        [DbColumn]
        public uint id_produit { get; private set; }

        [PrimaryKey(1)]
        [DbColumn]
        public uint id_magasin { get; private set; }

        [DbColumn]
        public uint quantite_stock { get; set; }

        [ForeignReference("produit", "id_produit", "id_produit")]
        public TProduit? produit { get; set; }

        [ForeignReference("magasin", "id_magasin", "id_magasin")]
        public TMagasin? magasin { get; set; }
    }
}
