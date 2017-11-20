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
            string input = File.ReadAllText("../../Example/Input.json");

            string transformer = File.ReadAllText("../../Example/Transformer_copy.json");

            string transformedString = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("################################################################################################");
            Console.WriteLine(transformedString);
        }

        static void testOPCUA()
        {
            string input = getOPCUAinput();

            Console.WriteLine("input= {0}\n", input);

            string transformer = getOPCUATransformer(69, 84);

            Console.WriteLine("transformer= {0}\n", transformer);

            // DO THE TRANSFORM
            string output = JsonTransformer.Transform(transformer, input);
            Console.WriteLine("output= {0}", output);

            // Save the JSON file
            var filePath = @".\transformer.json";
            File.WriteAllText(filePath, transformer);
            Console.WriteLine("filePath= {0}", filePath);
        }

        static string getOPCUAinput()
        {
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

            return inputJson.ToString();
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
    }
}
