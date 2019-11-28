using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace DataMicroApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(" ** Data Application Started ** ");

            MqttClient mClient = new MqttClient("127.0.0.1");
            string[] mStrTopicsInfo = { "sensor" };
            mClient.Connect(Guid.NewGuid().ToString());
            if (!mClient.IsConnected)
            {
                Console.WriteLine("Error connecting to message broker...");
                return;
            }

            try
            {
                //Specify events we are interest on
                //New Msg Arrived
                mClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                //Subscribe to topics
                // byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
                //MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE}; //QoS – depends on the topics number
                mClient.Subscribe(new string[] { "sensor" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                //mClient.Subscribe(mStrTopicsInfo, qosLevels);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
            if (mClient.IsConnected)
            {
                mClient.Unsubscribe(mStrTopicsInfo); //Put this in a button to see notif!
                mClient.Disconnect(); //Free process and process's resources
            }

        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);

            Console.WriteLine("Received = " + message +
            " on topic " + e.Topic);

            client_API(message);
        }

        static void client_API(String sensorToSend)
        {
            var client = new RestClient("http://localhost:50962");
            var request = new RestRequest("api/sensors", Method.POST);

            request.AddParameter("application/json", sensorToSend, ParameterType.RequestBody);

            client.ExecuteAsync(request, response => {
                Console.WriteLine(response.Content);
            });

        }
    }
}
