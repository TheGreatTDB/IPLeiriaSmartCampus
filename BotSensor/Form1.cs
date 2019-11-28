using SensorEntries;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace BotSensor
{
    public partial class Form1 : Form
    {
        MqttClient mClient = new MqttClient("127.0.0.1"); //OR use the broker hostname
        string[] mStrTopicsInfo = { "sensor" };
        private static Sensor currentSensor; //sensor -> Inic
        private static JavaScriptSerializer jss;
        private static string json;
        string pathToBinary = "..\\..\\binaryToRead\\data.bin";
        string pathToJson = "..\\..\\Json\\sensor.json";

        public Form1()
        {
            InitializeComponent();
          
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            currentSensor = new Sensor();
            jss = new JavaScriptSerializer();
            //Decode Binary File

            //********************************************************
            mClient.Connect(Guid.NewGuid().ToString());
            if (!mClient.IsConnected)
            {
                MessageBox.Show("Error connecting to message broker...");
                return;
            }

            try { 

                mClient.Subscribe(new string[] { "sensor" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // **************************************************
            //Timer wait 10 sec to execute
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
            //***************************************************
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("**************");
            Console.WriteLine("Reading !!!");
            Console.WriteLine("**************");
            Decode(pathToBinary);
            //write to file 
            saveJson(json);
        }
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            MessageBox.Show("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
        }
          public void Decode(string pathToBin)
         {
            StringBuilder sb = new StringBuilder();
            sb.Append("Showing contents of data.bin file:\n\n");
            string output = "";
            if (File.Exists(pathToBin))
            {
                FileStream fs = File.OpenRead(pathToBin);
                BinaryReader reader = new BinaryReader(fs);

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    currentSensor.id = reader.ReadInt32();
                    currentSensor.temperature = reader.ReadSingle();
                    currentSensor.humidity = reader.ReadSingle();
                    currentSensor.battery = reader.ReadInt32();
                    currentSensor.timestamp = reader.ReadInt32();
                    //int id = reader.ReadInt32();
                    //float temperature = reader.ReadSingle();
                    //float humidity = reader.ReadSingle();
                    // int battery = reader.ReadInt32();
                    //int timestamp = reader.ReadInt32();
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dt = dt.AddSeconds(currentSensor.timestamp).ToLocalTime();
                    int trash = reader.ReadInt32();
                    output += $"ID:{(byte)currentSensor.id}\n" +
                        $"TEMPERATURE:{currentSensor.temperature}\n" +
                        $"HUMIDITY:{currentSensor.humidity}\n" +
                        $"BATTERY:{(byte)currentSensor.battery}\n" +
                        $"TIMESTAMP:{currentSensor.timestamp} ({dt})\n";

                    if (trash == 0)
                    {
                        output += "\n";
                    }
                    else
                    {
                        throw new FormatException("File Reading Error, could not find end of line!");
                    }

                    //Serializa e chuta para golo , se não entra na baliza salta fora (JSON criado)
                    json = jss.Serialize(currentSensor);
                    mClient.Publish("sensor", Encoding.UTF8.GetBytes(json)); //MAIS TARDE ALTERAR A FORMA DE IR BUSCAR O TOPICO ****
                    Console.WriteLine("Uploaded by Program: " + json);
                    Thread.Sleep(2000); //WAIT
                }
                    
                fs.Close();
                sb.Append($"{output}");

                Console.Write("--------------------------");
                Console.Write(" * Value Decoded *");
                Console.Write(sb.ToString());
                Console.Write("--------------------------");
            }  
            
        }

        static string ConvertBinaryToText(List<List<int>> seq)
        {
            return new String(seq.Select(s => (char)s.Aggregate((a, b) => a * 2 + b)).ToArray());
        }

        private void saveJson(string data)
        {

            /*if (!System.IO.File.Exists(pathToJson))
            {
                System.IO.FileStream f = System.IO.File.Create(pathToJson);
                f.Close();
                f.Dispose();
            }
            using (System.IO.StreamWriter sw = System.IO.File.AppendText(pathToJson))
            {
                sw.WriteLine(data);
                MessageBox.Show("Dados Copiados");
                sw.Close();
                sw.Dispose();
            }*/

            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(pathToJson, true))
            {
                file.Write(data);
            }

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
        public void deleteFiles()
        {
            try
            {
                Console.WriteLine("Deleting file ... !!!");
                File.Delete(pathToBinary);
                File.Delete(pathToJson);

            }
            catch (Exception)
            {
                Console.WriteLine(" *** Error Deleting File !!!  ***");
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mClient.IsConnected)
            {
                mClient.Unsubscribe(mStrTopicsInfo); //Put this in a button to see notif!
                mClient.Disconnect(); //Free process and process's resources
            } 
        }
    }
}
