using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TMagasin : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_magasin { get; private set; }

        [DbColumn]
        public string? nom_magasin { get; set; }

        [DbColumn("id_adresse")]
        public uint id_adresse_localisation { get; set; }

        [ForeignReference("adresse", "id_adresse", "id_adresse_localisation")]
        public TAdresse? adresse_localisation { get; set; }

        public override string ToString()
        {
            return $"{nom_magasin} (#{id_magasin})";
        }
    }
}