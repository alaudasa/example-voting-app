using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redisa;
using System.Collections;

namespace Worker
{
    public class Program
    {
        public static int Main(string[] args)
        {      
		//get redis & postgres environment variable
		string redis_addr = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_ADDR");
		Console.WriteLine(redis_addr);
		string redis_port = Environment.GetEnvironmentVariable("REDIS_PORT_6379_TCP_PORT");
                Console.WriteLine(redis_port);
		string pqsql_addr = Environment.GetEnvironmentVariable("DB_PORT_5432_TCP_ADDR");
		Console.WriteLine(pqsql_addr);
		string pqsql_port = Environment.GetEnvironmentVariable("DB_PORT_5432_TCP_PORT");
                Console.WriteLine(pqsql_port);
		string pqsql_id = "postgres";
		string pqsql_name = "db";
            try
            {
		//string connstring = String.Format("Server={0};Port={1};" + 
                   // "User Id={2};Password={3};Database={4};",
                   // tbHost.Text, tbPort.Text, tbUser.Text, 
                   // tbPass.Text, tbDataBaseName.Text );
		string pgsqlstring = String.Format("Server={0};Port={1};" +
			"User Id={2};Database={3};",pqsql_addr,pqsql_port,pqsql_id,pqsql_name);
                var pgsql = OpenDbConnection(pgsqlstring);
                var redis = OpenRedisConnection(redis_addr).GetDatabase();

                var definition = new { vote = "", voter_id = "" };
                while (true)
                {
                    string json = redis.ListLeftPopAsync("votes").Result;
                    if (json != null)
                    {
                        var vote = JsonConvert.DeserializeAnonymousType(json, definition);
                        Console.WriteLine($"Processing vote for '{vote.vote}' by '{vote.voter_id}'");
                        UpdateVote(pgsql, vote.voter_id, vote.vote);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static NpgsqlConnection OpenDbConnection(string connectionString)
        {
            NpgsqlConnection connection;

            while (true)
            {
                try
                {
                    connection = new NpgsqlConnection(connectionString);
                    connection.Open();
                    break;
                }
                catch (SocketException)
                {
                    Console.Error.WriteLine("Waiting for db");
                    Thread.Sleep(1000);
                }
                catch (DbException)
                {
                    Console.Error.WriteLine("Waiting for db");
                    Thread.Sleep(1000);
                }
            }

            Console.Error.WriteLine("Connected to db");

            var command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS votes (
                                        id VARCHAR(255) NOT NULL UNIQUE, 
                                        vote VARCHAR(255) NOT NULL
                                    )";
            command.ExecuteNonQuery();

            return connection;
        }

        private static ConnectionMultiplexer OpenRedisConnection(string ipaddr )
        {
            // Use IP address to workaround hhttps://github.com/StackExchange/StackExchange.Redis/issues/410
            var ipAddress = ipaddr;
	    var redisPort = port;
            Console.WriteLine($"Found redis at {ipAddress}");

            while (true)
            {
                try
                {
                    Console.Error.WriteLine("Connected to redis");
                    return ConnectionMultiplexer.Connect(ipAddress);
                }
                catch (RedisConnectionException)
                {
                    Console.Error.WriteLine("Waiting for redis");
                    Thread.Sleep(1000);
                }
            }
        }

        private static string GetIp(string hostname)
            => Dns.GetHostEntryAsync(hostname)
                .Result
                .AddressList
                .First(a => a.AddressFamily == AddressFamily.InterNetwork)
                .ToString();

        private static void UpdateVote(NpgsqlConnection connection, string voterId, string vote)
        {
            var command = connection.CreateCommand();
            try
            {
                command.CommandText = "INSERT INTO votes (id, vote) VALUES (@id, @vote)";
                command.Parameters.AddWithValue("@id", voterId);
                command.Parameters.AddWithValue("@vote", vote);
                command.ExecuteNonQuery();
            }
            catch (DbException)
            {
                command.CommandText = "UPDATE votes SET vote = @vote WHERE id = @id";
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Dispose();
            }
        }
    }
}
