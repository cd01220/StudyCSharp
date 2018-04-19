namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

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
              'DisplayName': 'John Smith',
              'SamAccountName': 'contoso\\johns'
            }";

            Student student = JsonConvert.DeserializeObject<Student>(json);

            string jsonString = JsonConvert.SerializeObject(student);
            Console.WriteLine(jsonString);
        }
    }
}
