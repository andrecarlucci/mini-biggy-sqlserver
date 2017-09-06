using MiniBiggy;
using MiniBiggy.SaveStrategies;
using MiniBiggy.Serializers;
using MiniBiggy.Storage.SqlServer;
using System;

namespace Mini_Biggy.Storage.SqlServer.Cmd {
    class Program {
        static void Main(string[] args) {
            var conn = "Server=tcp:powerhub-sqlserver.database.windows.net,1433;Initial Catalog=powerhub-sql-prod;Persist Security Info=False;User ID=way2;Password=W2Energy#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            //var conn = @"Server=CARLUCCI-WAY2\SQLEXPRESS;Database=mini-biggy;Trusted_Connection=True;MultipleActiveResultSets=true";
            Console.WriteLine("Hello MiniBiggy on SqlServer!");

            var list = Create.ListOf<Tweet>()
                          .SavingOnSqlServer()
                          .KeepingLatest(10)
                          .WithConnectionString(conn)
                          .SavingOnTable("tweets")
                          .UsingPrettyJsonSerializer()
                          .BackgroundSavingEveryTwoSeconds();

            list.Saved += (sender, eventArgs) => {
                Console.WriteLine("saved");
            };


            Console.WriteLine("Hello, hit enter to create and save a tweet");
            while (true) {
                var line = Console.ReadLine();
                if (line == "exit") {
                    break;
                }
                list.Add(new Tweet {
                    DateTime = DateTime.Now,
                    Message = line,
                    Id = DateTime.Now.Second
                });
            }
            Console.WriteLine("End");
        }

        public class Tweet {
            public int Id { get; set; }
            public string Message { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
