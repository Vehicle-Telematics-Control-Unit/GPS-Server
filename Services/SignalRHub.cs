using GPS_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;

namespace GPS_Server.Services
{
    [Authorize(Policy = "MobileOnly")]
    public class LocationHub : Hub
    {
        private readonly IMqttClient mqttClient;
        private readonly TCUContext tcuContext;
        public LocationHub(IMqttClient mqttClient, TCUContext tcuContext)
        {
            this.mqttClient = mqttClient;
            this.tcuContext = tcuContext;
        }

        public async Task RequestGPS(long tcuId)
        {
            string connectionId = Context.ConnectionId;
            // Use the connection ID as needed
            if (Context.User == null)
            {
                await Clients.Caller.SendAsync("error", "user cannot be null");
                return;
            }
            string? deviceId = (from _claim in Context.User.Claims
                                where _claim.Type == "deviceId"
                                select _claim.Value).FirstOrDefault();
            DevicesTcu? deviceTcu = (from _tcuDevice in tcuContext.DevicesTcus
                            where _tcuDevice.TcuId == tcuId
                            && _tcuDevice.DeviceId == deviceId
                            select _tcuDevice).FirstOrDefault();
            
            if (deviceTcu == null)
            {
                await Clients.Caller.SendAsync("error", "user unauthorized");
                return;
            }

            var gpsRequest = new MqttApplicationMessageBuilder()
                .WithTopic("TCU-" + deviceTcu.TcuId.ToString() + "/sendGPS")
                .WithPayload("GPS/" + connectionId)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .WithRetainFlag(false)
                .Build();
            Console.WriteLine(connectionId);
            await mqttClient.PublishAsync(gpsRequest);
            await Clients.Caller.SendAsync("ACK", "Streaming started");
            return;
        }

        public async Task StopGPS(long tcuId)
        {
            string connectionId = Context.ConnectionId;
            // Use the connection ID as needed
            if (Context.User == null)
            {
                await Clients.Caller.SendAsync("error", "user cannot be null");
                return;
            }
            string? deviceId = (from _claim in Context.User.Claims
                                where _claim.Type == "deviceId"
                                select _claim.Value).FirstOrDefault();
            DevicesTcu? deviceTcu = (from _tcuDevice in tcuContext.DevicesTcus
                                     where _tcuDevice.TcuId == tcuId
                                     && _tcuDevice.DeviceId == deviceId
                                     select _tcuDevice).FirstOrDefault();

            if (deviceTcu == null)
            {
                await Clients.Caller.SendAsync("error", "user unauthorized");
                return;
            }

            var gpsRequest = new MqttApplicationMessageBuilder()
                .WithTopic("TCU-" + deviceTcu.TcuId.ToString() + "/stopGPS")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .WithRetainFlag(false)
                .Build();
            Console.WriteLine(connectionId);
            await mqttClient.PublishAsync(gpsRequest);
            await Clients.Caller.SendAsync("ACK", "Streaming started");
            return;
        }

    }
}
