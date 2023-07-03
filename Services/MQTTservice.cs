using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace GPS_Server.Services
{
    public class MQTTservice : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IHubContext<LocationHub> _hubContext;
        private readonly IConfiguration _config;
        public MQTTservice(IMqttClient mqttClient, IConfiguration config, IHubContext<LocationHub> hubContext)
        {
            _mqttClient = mqttClient;
            _config = config;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = _config.GetSection("MQTT:server").Value ?? "127.0.0.1";
            int port = int.Parse(_config.GetSection("MQTT:port").Value ?? "1883");
            var options = new MqttClientOptionsBuilder();
            options.WithTcpServer(server, port);
            options.WithCredentials("GPS", "P@ssw0rd123");
            options.WithClientId("GPS-Server");
            await _mqttClient.ConnectAsync(options.Build(), stoppingToken);
            _mqttClient.ApplicationMessageReceivedAsync += e => { 
                return HandleReceivedMessage(e);
            };

            await SubscribeToTopics(stoppingToken);
        }

        private async Task SubscribeToTopics(CancellationToken stoppingToken)
        {
            MqttFactory mqttFactory = new();
            var mqttSubscribeOptionsBuilder = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic("GPS/#");
                });
            // Subscribe to topics and handle received messages
            await _mqttClient.SubscribeAsync(mqttSubscribeOptionsBuilder.Build(), stoppingToken);
        }

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            MqttApplicationMessage appMessage = eventArgs.ApplicationMessage;
            var topic = appMessage.Topic;
            string connectionId = topic.Split("/", 2)[1];
            Console.WriteLine(connectionId);
            // Extract the location data from the MQTT message payload
            JObject location = ParseLocationFromPayload(eventArgs.ApplicationMessage.PayloadSegment);
            double? lat = location["lat"]?.Value<double>();
            if (lat == null)
                return;
            double? lng = location["lng"]?.Value<double>();
            if (lng == null)
                return;
            // Broadcast the location update to connected SignalR clients
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveLocation", lat, lng);
        }

        private static JObject ParseLocationFromPayload(ArraySegment<byte> payload)
        {
            var payloadStr = Encoding.UTF8.GetString(payload);
            return JObject.Parse(payloadStr);
        }
    }
}
