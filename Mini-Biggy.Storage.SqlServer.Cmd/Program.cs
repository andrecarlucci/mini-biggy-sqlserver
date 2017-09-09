﻿using MiniBiggy;
using System;

namespace Mini_Biggy.Storage.SqlServer.Cmd {
    class Program {
        static void Main(string[] args) {
            var conn = args.Length > 0 ? args[0] : "";

            if(conn == "") {
                Console.WriteLine("Please, specify the connection string as the first argument");
                return;
            }

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
