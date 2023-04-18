using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TBouquet : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_bouquet { get; private set; }

        [DbColumn]
        public string? nom_bouquet { get; set; }

        [DbColumn]
        public float prix_bouquet { get; set; }

        [DbColumn]
        public string? desc_bouquet { get; set; }

        [DbColumn]
        public string? categorie_bouquet { get; set; }
    }
}
