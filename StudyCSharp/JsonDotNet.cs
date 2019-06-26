﻿namespace StudyCSharp
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Newtonsoft.Json.Serialization;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// the JsonDotNet class.
    /// </summary>
    public class JsonDotNet
    {
        /// <summary>
        /// Debugging entrance.
        /// </summary>
        public static void TestJson()
        {
            TestJson05();
        }

        /// <summary>
        /// to test JsonConvert class: SerializeObject and DeserializeObject
        /// For simple scenarios where you want to convert to and from a JSON string, the SerializeObject() 
        /// and DeserializeObject() methods on JsonConvert provide an easy-to-use wrapper over JsonSerializer.
        /// </summary>
        public static void TestJson01()
        {
            string json = @"{
              'Name': 'John Smith',
              'SamAccountName': 'contoso\\johns'
            }";

            Student student = JsonConvert.DeserializeObject<Student>(json);
            student.Roles = new List<string>();
            Console.WriteLine(JsonConvert.SerializeObject(student));
        }

        /// <summary>
        /// Serialization Error Handling
        /// </summary>
        public static void TestJson02()
        {
            List<string> errors = new List<string>();

            // Error Event
            List<DateTime> dates = JsonConvert.DeserializeObject<List<DateTime>>(
            @"[
                '2009-09-09T00:00:00Z',
                'I am not a date and will error!',
                [1],
                '1977-02-20T00:00:00Z',
                null,
                '2000-12-01T00:00:00Z'
            ]",
            new JsonSerializerSettings
            {
                Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
                Converters = { new IsoDateTimeConverter() }
            });

            Console.WriteLine(string.Join(", ", dates));

            // OnErrorAttribute
            Student student = new Student
            {
                Name = "George Michael Bluth",
                Roles = null
            };

            string json = JsonConvert.SerializeObject(student, Formatting.Indented);
            Console.WriteLine(json);

            json = @"{
              'Name': 'John Smith',
              'SamAccountName': 'contoso\\johns'
             }";
             student = JsonConvert.DeserializeObject<Student>(json);
             Console.WriteLine(JsonConvert.SerializeObject(student, Formatting.Indented));
        }

        /// <summary>
        /// Validating with JSON Schema
        /// </summary>
        public static void TestJson03()
        {
            string jsonSchema = @"{
              'description': 'A person',
              'type': 'object',
              'properties':
              {
                'name': {'type':'string'},
                'hobbies': {
                  'type': 'array',
                  'items': {'type':'string'}
                }
              }
            }";

            JSchema schema = JSchema.Parse(jsonSchema);
            JObject person01 = JObject.Parse(@"{
              'name': 'James',
              'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
            }");
            bool valid01 = person01.IsValid(schema);

            JObject person02 = JObject.Parse(@"{
              'name': 'James',
              'hobbies': ['Invalid content', 0.123456789]
            }");
            IList<string> messages;
            bool valid02 = person02.IsValid(schema, out messages);
            Console.WriteLine(string.Join(";", messages));

            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// This sample deserializes JSON into an anonymous type.
        /// </summary>
        public static void TestJson04()
        {
            Type t = typeof(int);

            var definition = new { Name = string.Empty };

            string json1 = @"{'Name':'James'}";
            var customer1 = JsonConvert.DeserializeAnonymousType(json1, definition);
            Console.WriteLine(customer1.Name);

            string json2 = @"{'Name':'Mike'}";
            var customer2 = JsonConvert.DeserializeAnonymousType(json2, definition);
            Console.WriteLine(customer2.Name);
        }

        /// <summary>
        /// Deserialize with CustomCreationConverter.
        /// https://www.newtonsoft.com/json/help/html/DeserializeCustomCreationConverter.htm
        /// </summary>
        public static void TestJson05()
        {
            string json = @"{
                    'Department': 'Furniture',
                    'JobTitle': 'Carpenter',
                    'FirstName': 'John',
                    'LastName': 'Joinery',
                    'BirthDate': '1983-02-02T00:00:00'
                }";

            Person person = JsonConvert.DeserializeObject<Person>(json, new PersonConverter());

            Console.WriteLine(person.GetType().Name); // Employee
            Employee employee = (Employee)person;

            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// the PersonConverter class.
        /// </summary>
        public class PersonConverter : CustomCreationConverter<Person>
        {
            /// <summary>
            /// Crete Person object. 
            /// </summary>
            /// <param name="objectType">the object type</param>
            /// <returns>the person object.</returns>
            public override Person Create(Type objectType)
            {
                return new Employee();
            }
        }

        /// <summary>
        /// PreserveReferencesHandling setting.
        /// https://www.newtonsoft.com/json/help/html/PreserveReferencesHandlingObject.htm
        /// </summary>
        public static void TestJson06()
        {
            Directory root = new Directory { Name = "Root" };
            Directory documents = new Directory { Name = "My Documents", Parent = root };
            File file = new File { Name = "ImportantLegalDocument.docx", Parent = documents };

            documents.Files = new List<File> { file };

            try
            {
                string defaultJson = JsonConvert.SerializeObject(documents, Formatting.Indented);
                Console.WriteLine(defaultJson);
            }
            catch (JsonSerializationException ex)
            {
                // Self referencing loop detected for property 'Parent' with type
                // 'Newtonsoft.Json.Tests.Documentation.Examples.ReferenceLoopHandlingObject+Directory'. Path 'Files[0]'.
                Console.WriteLine(ex.Message);
            }

            var settings0 = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
            string preserveReferenacesAll = JsonConvert.SerializeObject(documents, Formatting.Indented, settings0);

            Console.WriteLine(preserveReferenacesAll);
            // {
            //   "$id": "1",
            //   "Name": "My Documents",
            //   "Parent": {
            //     "$id": "2",
            //     "Name": "Root",
            //     "Parent": null,
            //     "Files": null
            //   },
            //   "Files": {
            //     "$id": "3",
            //     "$values": [
            //       {
            //         "$id": "4",
            //         "Name": "ImportantLegalDocument.docx",
            //         "Parent": {
            //           "$ref": "1"
            //         }
            //       }
            //     ]
            //   }
            // }

            var settings1 = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            string preserveReferenacesObjects = JsonConvert.SerializeObject(documents, Formatting.Indented, settings1);

            Console.WriteLine(preserveReferenacesObjects);
            // {
            //   "$id": "1",
            //   "Name": "My Documents",
            //   "Parent": {
            //     "$id": "2",
            //     "Name": "Root",
            //     "Parent": null,
            //     "Files": null
            //   },
            //   "Files": [
            //     {
            //       "$id": "3",
            //       "Name": "ImportantLegalDocument.docx",
            //       "Parent": {
            //         "$ref": "1"
            //       }
            //     }
            //   ]
            // }
            Console.WriteLine(string.Empty);
        }
        
        /// <summary>
        /// 学习 Timestamp => Datatime 的转换
        /// </summary>
        public static void TestJson07() 
        {
            string json = @"{
                    'FirstName': 'John',
                    'LastName': 'Joinery',
                    'BirthDate': '1234656000'
                }";

            Person person = JsonConvert.DeserializeObject<Person>(json, UnixSecondsConverter.Instance);

            Console.WriteLine(person.BirthDate);
        }

        /// <summary>
        /// 解析 okex future ticker
        /// </summary>
        public static void TestJson08() 
        {
            string json = @"
            {
                'date':'1528557628',
	            'ticker':
	            {
	                'high':7715.13,
	                'vol':1557226,
	                'day_high':0,
	                'last':7626.31,
	                'low':7555.84,
	                'contract_id':201806150000012,
	                'buy':7626.89,
	                'sell':7627.55,
	                'coin_vol':0,
	                'day_low':0,
	                'unit_amount':100
	            }
            }";

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    UnixSecondsConverter.Instance
                }
            };

            TickerContainer tickerContainer = JsonConvert.DeserializeObject<TickerContainer>(json, settings);
            Console.WriteLine(JsonConvert.SerializeObject(tickerContainer, Formatting.Indented, settings));
        }        
    } //public class JsonDotNet

    /// <summary>
    /// the entity class Student.
    /// </summary>
    public class Student
    {
        /// <summary>
        /// keep the additional data.
        /// </summary>
        [JsonExtensionData]
        private readonly IDictionary<string, JToken> additionalData;

        /// <summary>
        /// the person's roles
        /// </summary>
        private List<string> roles;

        /// <summary>
        /// Initializes a new instance of the Student class.
        /// </summary>
        public Student()
        {
            this.additionalData = new Dictionary<string, JToken>();
        }

        /// <summary>
        /// Gets or sets the Name of the student.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Domain of the student.
        /// </summary>
        [JsonIgnore]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Roles of the student.
        /// </summary>
        public List<string> Roles
        {
            get
            {
                return this.roles;
            }

            set
            {
                this.roles = value;
            }
        }

        /// <summary>
        /// Gets or sets the UserName of the student.
        /// these properties are set in OnDeserialized
        /// </summary>
        [JsonIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// the OnDeserialized method.
        /// Example:
        /// string json = @"{
        ///  'Name': 'John Smith',
        ///  'SAMAccountName': 'contoso\\johns'
        /// }";
        /// var student = JsonConvert.DeserializeObject<Student>(json);
        /// </summary>
        /// <param name="context">the context</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // SAMAccountName is not deserialized to any property
            // and so it is added to the extension data dictionary
            string samAccountName = (string)this.additionalData["SamAccountName"];

            string[] samAccountDic = samAccountName.Split('\\');
            this.Domain = samAccountDic[0];
            this.UserName = samAccountDic[1];
        }

        /// <summary>
        /// the OnError method.
        /// </summary>
        /// <param name="context">the context</param>
        /// /// <param name="errorContext">the errorContext</param>
        [OnError]
        private void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
