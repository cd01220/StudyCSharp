using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyCSharp;

namespace StudyMSTest
{
    [TestClass]
    public class UnitTestJsonDotNet
    {
        [TestMethod]
        public void TestJsonDotNet1()
        {
            JsonDotNet.TestJson();
            Console.WriteLine("");
        }
    }
}
