namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// the JsonDotNet class.
    /// </summary>
    public class JsonDotNet
    {
        /// <summary>
        /// to test JsonConvert class.
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

            string jsonString = JsonConvert.SerializeObject(student);
            Console.WriteLine(jsonString);
        }

        /// <summary>
        /// Serialization Error Handling
        /// </summary>
        public static void TestJson02()
        {
            List<string> errors = new List<string>();

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

            foreach (var date in dates)
            {
                Console.WriteLine(date);
            }

            Student student = new Student
            {
                Name = "George Michael Bluth",
                Roles = null
            };

            string json = JsonConvert.SerializeObject(student, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}
