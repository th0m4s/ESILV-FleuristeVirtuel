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

        public new void InsertInto(string tableName, DbConnection connection, bool insertPrimaryKey = false)
        {
            base.InsertInto(tableName, connection, insertPrimaryKey);
            RemplirMagasins(connection);
        }

        public void RemplirMagasins(DbConnection conn, List<TMagasin>? magasins = null)
        {
            if (magasins == null)
                magasins = conn.SelectMultipleRecords<TMagasin>("SELECT * FROM magasin WHERE magasin.id_magasin NOT IN " +
                    "(SELECT s.id_magasin FROM stock AS s WHERE s.id_produit = @produit)", new DbParam("@produit", id_produit));

            foreach (TMagasin magasin in magasins)
            {
                TStock stock = DbRecord.CreateEmptyOrGetInstance<TStock>(id_produit, magasin.id_magasin);
                stock.quantite_stock = 0;
                stock.InsertInto("stock", conn, true);
            }
        }

        public override string ToString()
        {
            return $"{nom_produit} (#{id_produit})";
        }
    }
}
