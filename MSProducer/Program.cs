using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MSProducer
{
    class Program
    {
        static void Main(string[] args)
        {

            RunAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        static async Task RunAsync()
        {
            string endPoint = _APIUrl();
            await GetAllFoods(endPoint);
        }

        private static async Task GetAllFoods(string endPoint)
        {
            using (HttpClientHandler clientHandler = new HttpClientHandler())
            {
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }; // in case of ssl

                using (HttpClient httpClient = new HttpClient(clientHandler))
                {
                    //MediaTypeWithQualityHeaderValue contentType =
                    //new MediaTypeWithQualityHeaderValue("application/json");
                    //httpClient.DefaultRequestHeaders.Accept.Add(contentType);
                    using (var Response = await httpClient.GetAsync(endPoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string stringData = Response.Content.ReadAsStringAsync().Result;
                            try
                            {
                                var value = JsonConvert.DeserializeObject<Value>(stringData);
                                Publish(value.value.ToString());
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
        }

        public static void Publish(string json)
        {
            var factory = new ConnectionFactory()
            {
                //UserName = "guest",
                //Password = "guest",
                //VirtualHost = "/",
                //factory.AmqpUriSslProtocols = Protocols.DefaultProtocol();
                //HostName = "0.0.0.0:5672",//"192.168.0.12";
                HostName = "localhost",
                //Port = 15672,
                //RequestedHeartbeat = TimeSpan.FromSeconds(100),
                //AutomaticRecoveryEnabled = true
            };
            
            var connection = factory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "food",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "",
                            routingKey: "food",
                            basicProperties: null,
                            body: body);
            }

        }

        public static dynamic _APIUrl() => "https://localhost:44378/api/v1/foods";
    }

    public class Value
    {
        [JsonProperty("value")]
        public string value { get; set; }
    }

    public class FoodEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Calories { get; set; }
        public DateTime Created { get; set; }
    }
}
