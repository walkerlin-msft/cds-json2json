using JUST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json2Json
{
    class Program
    {


        static void Main(string[] args)
        {
            /* Generate the Convert Script */
            //GenerateConvertScript_Opcua5231Publisher("ICP_DAS_UA5231-publisher-v212.json");
            GenerateConvertScript_OpcuaDrillerPublisher("OPCUA_Gateway_uagwadvt01.json");

            /* Watch the result */
            //WatchTransformResult("ICP_DAS_UA5231-publisher-v212.json", "Transform-ICP_DAS_UA5231-publisher-v212.json");
            //WatchTransformResult("OPCUA_Gateway_uagwadvt01.json", "Transform-OPCUA_Gateway_uagwadvt01.json");

            Console.ReadLine();
        }

        static void WatchTransformResult(string telemetryFileName, string transformerFileName)
        {
            Console.WriteLine("[WatchTransformResult]");
            Console.WriteLine("input file: {0}", telemetryFileName);
            Console.WriteLine("transformer file: {0}\n", transformerFileName);
            string input = File.ReadAllText("../../TelemetryExample/"+ telemetryFileName);
            string transformer = File.ReadAllText("../../Transform/"+ transformerFileName);

            var token = JToken.Parse(input);
            if(token is JArray)
            {
                IEnumerable<JObject> elementList = token.ToObject<List<JObject>>();

                foreach(var element in elementList)
                {
                    string transformedString = JsonTransformer.Transform(transformer, element.ToString());
                    Console.WriteLine(transformedString);
                    Console.WriteLine("################################################################################################");
                }
            }
            else if(token is JObject)
            {
                JObject element = token.ToObject<JObject>();
                string transformedString = JsonTransformer.Transform(transformer, element.ToString());
                Console.WriteLine(transformedString);
                Console.WriteLine("################################################################################################");
            }
            else
            {
                Console.WriteLine("Not a vaild JSON input file");
            }
        }

        static void GenerateConvertScript_OpcuaDrillerPublisher(string filename)
        {
            string input = File.ReadAllText("../../TelemetryExample/" + filename);
            Console.WriteLine("input= {0}\n", input);

            string transformer = getOpcuaTransformerUagwadvt01(55, 259);

            Console.WriteLine("transformer= {0}\n", transformer);

            /* Test */
            List<JObject> elementList = JsonConvert.DeserializeObject<List<JObject>>(input);
            foreach (var element in elementList.Select((value, index) => new { Value = value, Index = index }))
            {
                //// DO THE TRANSFORM
                string output = JsonTransformer.Transform(transformer, element.Value.ToString());
                Console.WriteLine("output[{0}]= {1}", element.Index, output);
            }

            //// Save the JSON file
            var filePath = @"../../Transform/Transform-" + filename;
            File.WriteAllText(filePath, transformer);
            Console.WriteLine("filePath= {0}", filePath);
        }

        static void GenerateConvertScript_Opcua5231Publisher(string filename)
        {
            string input = File.ReadAllText("../../TelemetryExample/" + filename);

            Console.WriteLine("input= {0}\n", input);

            string transformer = getOPCUATransformerUA5231(69, 85);

            Console.WriteLine("transformer= {0}\n", transformer);

            /* Test */
            List<JObject> elementList = JsonConvert.DeserializeObject<List<JObject>>(input);
            foreach (var element in elementList.Select((value, index) => new { Value = value, Index = index }))
            {
                //// DO THE TRANSFORM
                string output = JsonTransformer.Transform(transformer, element.Value.ToString());
                Console.WriteLine("output[{0}]= {1}", element.Index, output);
            }

            //// Save the JSON file
            var filePath = @"../../Transform/Transform-"+ filename;
            File.WriteAllText(filePath, transformer);
            Console.WriteLine("filePath= {0}", filePath);
        }

        static string getOpcuaTransformerUagwadvt01(int companyId, int messageCatalogId)
        {
            var transformerJson = new JObject();

            // messageCatalogId
            transformerJson.Add("messageCatalogId", messageCatalogId);

            // companyId
            transformerJson.Add("companyId", companyId);

            // msgTimestamp
            var msgTimestamp = Just.GetValueIfExistsAndNotEmpty("Value.SourceTimestamp");
            transformerJson.Add("msgTimestamp", msgTimestamp);

            // equipmentId
            var applicationUri = Just.GetValueIfExists("ApplicationUri");
            var gatewayName = Just.GetValueIfExists("Gateway");

            var equipmentId = Just.IfCondition(applicationUri,
                "urn::SchneiderElectric:M241-251UaServer",
                Just.IfCondition(gatewayName,
                    "uagwadvt01",
                    "SchneiderPLC-A",
                    Just.IfCondition(gatewayName, 
                        "uagwmoxa01",
                        "SchneiderPLC-M",
                        Just.IfCondition(gatewayName,
                            "uagwnexc01",
                            "SchneiderPLC-N",
                            Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND))),
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND);
            transformerJson.Add("equipmentId", equipmentId);

            // equipmentRunStatus
            transformerJson.Add("equipmentRunStatus", 1);// Hard code

            var valueValue = Just.GetValueIfExistsAndNotEmpty("Value.Value");
            var nodeId = Just.GetValueIfExistsAndNotEmpty("NodeId");

            // RPM
            var rpm = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=OPC.Read_Velocity",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("RPM", rpm);

            // Distance
            var distance = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=OPC.Sensor_IW0",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("Distance", distance);

            // cpuUsage
            var cpuUsage = Just.GetValueIfExistsAndNotEmpty("CPU_Usage");
            transformerJson.Add("cpuUsage", cpuUsage);

            // memoryUsage
            var memoryUsage = Just.GetValueIfExistsAndNotEmpty("Memory_Usage");
            transformerJson.Add("memoryUsage", memoryUsage);

            // Approach
            var approach = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=OPC.Sensor_IX00_ON",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("Approach", approach);

            return transformerJson.ToString();
        }

        static string getOPCUATransformerUA5231(int companyId, int messageCatalogId)
        {
            var transformerJson = new JObject();

            // messageCatalogId
            transformerJson.Add("messageCatalogId", messageCatalogId);

            // companyId
            transformerJson.Add("companyId", companyId);

            // msgTimestamp
            var msgTimestamp = Just.GetValueIfExistsAndNotEmpty("Value.SourceTimestamp");
            transformerJson.Add("msgTimestamp", msgTimestamp);

            // equipmentId
            var applicationUri = Just.GetValueIfExists("ApplicationUri");

            var equipmentId = Just.IfCondition(applicationUri,
                "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_Server",
                "Windwos-OPC-UA-Publisher-v212",
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND);
            transformerJson.Add("equipmentId", equipmentId);

            // equipmentRunStatus
            transformerJson.Add("equipmentRunStatus", 1);// Hard code


            var valueValue = Just.GetValueIfExistsAndNotEmpty("Value.Value");
            var nodeId = Just.GetValueIfExistsAndNotEmpty("NodeId");

            // TemperatureC
            var temperature = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=MTCP_CurrentValueTask.Temperature_C",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("TemperatureC", temperature);

            // CO2
            var co2 = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=MTCP_CurrentValueTask.CO2",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("CO2", co2);

            // Humidity
            var humidity = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=MTCP_CurrentValueTask.Humidity",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("Humidity", humidity);

            // TemperatureF
            var temperatureF = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=MTCP_CurrentValueTask.Temperature_F",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("TemperatureF", temperatureF);

            return transformerJson.ToString();
        }
    }
}
