using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TCompose : DbRecord
    {
        [PrimaryKey(0)]
        [DbColumn]
        public uint id_produit { get; private set; }

        [PrimaryKey(1)]
        [DbColumn]
        public uint id_bouquet { get; private set; }

        [ForeignReference("produit", "id_produit", "id_produit")]
        public TProduit? produit { get; set; }

        [ForeignReference("bouquet", "id_bouquet", "id_bouquet")]
        public TBouquet? bouquet { get; set; }
    }
}
