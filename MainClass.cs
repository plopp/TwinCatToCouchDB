using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TwinCAT.Ads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

/**
 * This program reads REAL variables from a Beckhoff PLC and writes them to CouchDB.
 * All details for making the connection to the PLC and CouchDB database are contained in a
 * config file called data.conf. It should be placed at the root of the executable.
 * 
 * This software depends on JSON.NET and TwinCat ADS libraries.
 * 
 * Author: Marcus Kempe, marcus.kempe@sp.se
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 SP Technical Research Institute of Sweden
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 **/

namespace adstocdb
{
    class MainClass
    {
        /** 
         * Reads one variable from the PLC. The variable must be declared as a REAL.
         * 
         * Input: 
         * tcAds - TwinCat ADS client object
         * var - The variable name (as a string) to be read from the PLC. E.g "MAIN.var1"
         * vartype - The variable type as declared in the PLC. REAL is the only supported variable type.
         * More types can be added by making changes where appropriate.
         * 
         * Output: Floating value representing the value in the PLC.
         * 
        **/
        static float readByString(TcAdsClient tcAds,string var,string vartype)
        {
            int hVar = 0;
            try
            {
                hVar = tcAds.CreateVariableHandle(var);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            if (vartype == "REAL")
            {
                // creates a stream with a length of 4 byte 
                AdsStream ds = new AdsStream(4);
                BinaryReader br = new BinaryReader(ds);
                tcAds.Read(hVar, ds);
                try
                {
                    tcAds.DeleteVariableHandle(hVar);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                return br.ReadSingle();
            }
            else {
                Console.WriteLine("Error: Variable type not implemented!");
                return 0.0F;
            }
        }

        /** 
         * Reads the data.conf file and stores the information in the props and vars dictionary 
         * 
         * Input: 
         * props - Empty dictionary for containing keys and values for the configuration
         * vars - Empty list containing the PLC variables to be read from the PLC and stored to the database
         * 
         * Output: void
        **/
        static void readFile(Dictionary<string, string> props, List<string> vars, string file)
        {
            string line;
            using (StreamReader reader = new StreamReader("data.conf"))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains('='))
                    {
                        line = line.Trim();
                        Console.WriteLine("Property: " + line);
                        string[] split = line.Split('=');                        
                        props.Add(split[0].Trim(), split[1].Trim());
                    }
                    else
                    {
                        Console.WriteLine("Variable: " + line);
                        vars.Add(line.Trim());
                    }
                }
            }
        }

        /** 
         *  Reads the variables contained in the list "vars" (read from the config file).
         *  Adds each  supplied variable name as a key to a JSON-object, as well as its value
         *  as read from the PLC. A timestamp is added (Linux epoch milliseconds). The JSON-object 
         *  is then sent to CouchDB.
        **/
        static void Main(string[] args)
        {
            TcAdsClient tcAds = new TcAdsClient();

            //Dictionary containing the configuration keys and values from the data.conf file.
            Dictionary<string, string> props = new Dictionary<string, string>();

            //List containing the variables to be read from the PLC and stored in the CouchDB database
            List<string> vars = new List<string>();

            //Populate the props dictionary and vars list from the content of the data.conf file
            readFile(props, vars, "data.conf");

            //Connect to the PLC
            Console.WriteLine("Connecting to " + props["netId"] + " on port " + props["adsport"]);
            tcAds.Connect(props["netId"], Convert.ToUInt16(props["adsport"]));            

            //Create and build the JSON-object.
            JObject jobj = new JObject();
            jobj.Add("timestamp",(long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
            foreach (string s in vars)
            {
                float value = readByString(tcAds, s, "REAL");
                jobj.Add(s, value);
            }

            //End connection to the PLC.
            tcAds.Dispose();

            //Info printout of JSON-object
            Console.WriteLine(jobj.ToString());

            //Create a web request for posting to CouchDB.
            WebRequest request = WebRequest.Create(props["dbprotocol"]+"://"+props["dbip"]+":"+props["dbport"]+"/"+props["dbname"]);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(props["dbusername"],props["dbpassword"]);
            string postData = jobj.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json";
            request.ContentLength = postData.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Get and print the response from the server
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);

            reader.Close();
            dataStream.Close();
            response.Close();                        
        }
    }
}
