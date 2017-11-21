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

            testOPCUA();
            //testInputSampleFromFile();

            Console.ReadLine();
        }

        static void testInputSampleFromFile()
        {
            string input = File.ReadAllText("../../Example/Input-ICP_DAS_UA5231-publisher-v212.json");

            string transformer = File.ReadAllText("../../Example/Transformer_copy.json");

            string transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);
        }

        static void testOPCUA()
        {
            //string input = getOPCUAinput();
            string input = File.ReadAllText("../../Example/Input-ICP_DAS_UA5231-publisher-v212.json");
            //string input = File.ReadAllText("../../Example/Input-opc-ua_data.json");

            Console.WriteLine("input= {0}\n", input);

            //string transformer = getOPCUATransformer(69, 85);
            string transformer = getOPCUATransformerUA5231(69, 85);

            Console.WriteLine("transformer= {0}\n", transformer);

            List<JObject> elementList = JsonConvert.DeserializeObject<List<JObject>>(input);

            foreach (var element in elementList.Select((value, index) => new { Value = value, Index = index }))
            {
                //// DO THE TRANSFORM
                string output = JsonTransformer.Transform(transformer, element.Value.ToString());
                Console.WriteLine("output[{0}]= {1}", element.Index, output);
            }

            //// Save the JSON file
            var filePath = @".\transformer.json";
            File.WriteAllText(filePath, transformer);
            Console.WriteLine("filePath= {0}", filePath);
        }

        static string getOPCUAinput()
        {
            var inputArray = new JArray();
            var inputJson = new JObject();
            var valueJson = new JObject();

            valueJson.Add("Value", 2717);
            //valueJson.Add("Value", null);

            valueJson.Add("SourceTimestamp", "2017-11-04T09:59:48.3407981Z");
            //valueJson.Add("SourceTimestamp", "");

            //inputJson.Add("ApplicationUri", "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_ServerABC");
            inputJson.Add("ApplicationUri", "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_ServerA01");
            //inputJson.Add("ApplicationUri", "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_ServerA02");

            inputJson.Add("DisplayName", "ns=2;s=MTCP_CurrentValueTask.Temperature_C");

            //inputJson.Add("NodeId", "ns=2;s=MTCP_CurrentValueTask.Temperature_F");
            inputJson.Add("NodeId", "ns=2;s=MTCP_CurrentValueTask.Temperature_C");

            inputJson.Add("Value", valueJson);

            inputArray.Add(inputJson);
            return inputArray.ToString();
        }

        static string getOPCUATransformer(int companyId, int messageCatalogId)
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
                "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_ServerA01",
                "A001",
                Just.IfCondition(applicationUri,
                    "urn:OPC-A:ICPDAS:ICPDAS_OPC_UA_ServerA02",
                    "A002",
                    Just.SubStringFromLastColon(applicationUri)));
            transformerJson.Add("equipmentId", equipmentId);

            // equipmentRunStatus
            transformerJson.Add("equipmentRunStatus", 1);// Hard code


            var valueValue = Just.GetValueIfExistsAndNotEmpty("Value.Value");
            var nodeId = Just.GetValueIfExistsAndNotEmpty("NodeId");

            // Temperature
            var temperature = Just.IfCondition(
                nodeId,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND,
                Just.IfCondition(
                    nodeId,
                    "ns=2;s=MTCP_CurrentValueTask.Temperature_C",
                    valueValue,
                    Just.CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND));
            transformerJson.Add("Temperature", temperature);

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
