using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using System.IO;
using System.Linq;

namespace l2db_bug
{
    class Db : DataConnection
    {
        public Db(string path) :
            base("SQLite", $@"Data Source = {path}; foreign keys = true; Version = 3;")
        { }
    }

    interface IService
    {
        int Id { get; set; }
        int? IdClient { get; set; }
    }

    [Table(Schema = "dbo", Name = "adsl")]
    public partial class Adsl : IService
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; } // int
        [Column("id_client"), Nullable] public int? IdClient { get; set; } // int
    }

    [Table(Schema = "dbo", Name = "client")]
    public partial class Client
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; } // int
    }

    class Program
    {
        static void Main(string[] args)
        {
            string dbPath = "db.db";

            using (File.Create(dbPath)) { }

            using (var db = new Db(dbPath))
            {
                db.CreateTable<Adsl>();
                db.CreateTable<Client>();

                IQueryable<Client> q_clients = db.GetTable<Client>();

                // Start combining query as generic
                var q_services = db.GetTable<Adsl>() as IQueryable<IService>;

                // Then make something specific for table
                var q_adsl = q_services as IQueryable<Adsl>;
                q_services = (from adsl in q_adsl select adsl) as IQueryable<IService>;

                // And continue as generic again
                var q_test = (
                    from serv in q_services
                    join client in q_clients on serv.IdClient equals client.Id
                    select serv.Id
                );

                var res = q_test.ToList();
            }
        }
    }
}
