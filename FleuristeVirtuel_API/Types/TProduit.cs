using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TProduit : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_produit { get; private set; }

        [DbColumn]
        public string? nom_produit { get; set; }

        [DbColumn]
        public float prix_produit { get; set; }

        [DbColumn]
        public string? categorie_produit { get; set; }
    }
}
