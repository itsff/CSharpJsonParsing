using System;
using System.Runtime.Serialization;
using System.Collections.Generic;


// Third-party, open source library for fast JSON parsing.
// Microsoft itself is using it in nearly all of their
// open-source code (ex. ASP.NET MVC)
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace CSharpJsonParsing
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string json = "{}";

            //
            // Download sample data from web.
            // Get yourself an access token at: https://developers.facebook.com/tools/explorer/
            //
            // var accessToken = "AAACEdEose0cBAISmbnaCK6cqaHmXqVyQLzZBuiDmre2RKXMGcJkME6hHn4JuTOxmmB0hIvUG0oS14Kn94JKBoMR9XAybCac9FskuZBTAZDZD";
            // var webClient = new System.Net.WebClient();
            // json = webClient.DownloadString("https://graph.facebook.com/zuck?access_token=" + accessToken);
            // System.IO.File.WriteAllText("zuck.js", json);

            //
            // Read a file containg some sample JSON data.
            //
            json = System.IO.File.ReadAllText("zuck.js");



            readViaArrayIndexers(json);
            readViaDynamicBinding(json);
            readViaStronglyTypedSerialization(json);


            writeViaArrayIndexers();
            writeViaDynamicBinding();
            writeViaStronglyTypedSerialization();
        }


        static void readViaArrayIndexers(string json)
        {
            // Accessing parsed JSON object via [] indexers.
            // Error-prone. Typos matter. Types can change.

            JObject obj = JObject.Parse(json);

            Console.WriteLine(
                "First name: {0}\nLast name: {1}\nbio: {2}\nwork: {3}\n{4}\n\n",
                obj ["first_name"], // <--- Typos or using unknown fields/properties will cause crash at runtime.
                obj ["last_name"],
                obj ["bio"],
                obj ["work"][0] ["employer"] ["name"],
                obj ["work"][0] ["description"]);

        }

        static void readViaDynamicBinding(string json)
        {
            // The "dynamic" keyword allows you to write code Python-style.
            // Everything is late-bound and evaluated at run-time.
            // Flexible, easy to write, looks prettier than array indexers.
            // No intellisense support.
            // Error-prone. Typos matter. Types can change.

            dynamic obj = JObject.Parse(json);


            Console.WriteLine(
                "First name: {0}\nLast name: {1}\nbio: {2}\nwork: {3}\n{4}\n\n",
                obj.first_name, // <--- Typos or using unknown fields/properties will cause crash at runtime.
                obj.last_name,
                obj.bio,
                obj.work[0].employer.name,
                obj.work[0].description);
        }

        static void readViaStronglyTypedSerialization(string json)
        {
            // If you have a schema for data you expect to work with,
            // you can pre-generate appropriate C# classes (see below).
            // This allows for working with strong types and relying on
            // the compiler to find typos and obvious bugs.
            // Generated classes can be serialized into different formats
            // (json, xml, protobuf, fix, thrift, whatever)

            PersonInfo obj = JsonConvert.DeserializeObject<PersonInfo>(json);

            Console.WriteLine(
                "First name: {0}\nLast name: {1}\nbio: {2}\nwork: {3}\n{4}\n\n",
                obj.FirstName, // <--- Typos or using unknown fields/properties will trigger compile error.
                obj.LastName,
                obj.Bio,
                obj.Work[0].employer.name,
                obj.Work[0].description);
        }



        static void writeViaArrayIndexers()
        {
            JObject obj = new JObject();
            obj ["first_name"] = "Filip";
            obj ["last_name"] = "Frącz";
            obj ["bio"] = "I'm just a boring person.";


            JObject work = new JObject();
            work ["employer"] = new JObject();
            work ["employer"]["name"] = "Trading Technologies";
            work ["description"] = "I've been working here for a while";

            obj ["work"] = new JArray(work);

            Console.WriteLine(obj.ToString());
            Console.WriteLine();
            Console.WriteLine();
        }

        static void writeViaDynamicBinding()
        {
            dynamic obj = new JObject();
            obj.first_name = "Filip";
            obj.last_name = "Frącz";
            obj.bio = "I'm just a boring person.";
            obj.work = new JArray();

            dynamic work = new JObject();
            work.employer = new JObject();
            work.employer.name = "Trading Technologies";
            work.description = "I've been working here for a while";

            obj.work.Add(work);

            Console.WriteLine(obj.ToString());
            Console.WriteLine();
            Console.WriteLine();
        }

        static void writeViaStronglyTypedSerialization()
        {
            // Create strongly-typed object
            var obj = new PersonInfo();
            obj.FirstName = "Filip";
            obj.LastName = "Frącz";
            obj.Bio = "I'm just a boring person.";

            obj.Work.Add(new WorkInfo() {
                employer = new IdName() { name = "Trading Technologies" },
                description = "I've been working here for a while"
            });


            // These are purely optional. I'm setting them to match output of
            // other example methods.
            var settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            Console.WriteLine(JsonConvert.SerializeObject(obj, settings));
            Console.WriteLine();
            Console.WriteLine();
        }
    }


    // Custom class for holding person's information.
    // Normally you can generate such container classes from schema information.
    // 
    // This class demonstrates that the C# property/member variable name can differ
    // from the names used in JSON. 
    // It also demonstrates the use of constructor to populate certain fields:
    // guarding from null list.
    //
    [DataContract]
    class PersonInfo
    {
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "bio")]
        public string Bio { get; set; }

        [DataMember(Name = "work")]
        public List<WorkInfo> Work { get; private set; }


        public PersonInfo()
        {
            this.Work = new List<WorkInfo>();
        }
    }

    // Sample class for work information.
    // Uses properties with names matching JSON.
    class WorkInfo
    {
        public IdName employer  { get; set; }
        public IdName location  { get; set; }
        public IdName position  { get; set; }

        public string description { get; set; }
    }

    // Simple id-name tuple class.
    // Uses raw fields insted of properties. Same names as JSON.
    class IdName
    {
        public string id;
        public string name;
    }
}
