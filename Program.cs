namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// the Program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// the Main method.
        /// </summary>
        /// <param name="args">the args</param>
        private static void Main(string[] args)
        {
            TestJson01();
        }

        /// <summary>
        /// to test JsonConvert class.
        /// For simple scenarios where you want to convert to and from a JSON string, the SerializeObject() 
        /// and DeserializeObject() methods on JsonConvert provide an easy-to-use wrapper over JsonSerializer.
        /// </summary>
        private static void TestJson01()
        {
            Student originalObject = new Student();
            originalObject.ID = 1;
            originalObject.Name = "Liu Hao";

            string jsonString = JsonConvert.SerializeObject(originalObject);
            Console.WriteLine(jsonString);

            Student deserializedObject = JsonConvert.DeserializeObject(jsonString, typeof(Student)) as Student;
        }
    }    
}
