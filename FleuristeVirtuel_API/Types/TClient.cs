using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TClient : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_client { get; private set; }

        [DbColumn]
        public string? nom_client { get; set; }

        [DbColumn]
        public string? prenom_client { get; set; }

        [DbColumn]
        public string? email_client { get; set; }

        [DbColumn]
        public string? telephone_client { get; set; }

        [DbColumn]
        public string? mot_de_passe { get; set; }

        [DbColumn]
        public string? carte_de_credit { get; set; }

        [DbColumn("id_adresse")]
        public uint id_adresse { get; set; }

        [ForeignReference("adresse", "id_adresse", "id_adresse")]
        public TAdresse? adresse_facturation { get; set; }

        public override string ToString()
        {
            return $"{nom_client} {prenom_client}";
        }

        public string Describe()
        {
            return $"{ToString()} (${email_client})";   
        }
    }
}