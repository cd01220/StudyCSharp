namespace StudyCSharp
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Threading;

    class MefPractices
    {
        [Import]
        private string message;

        public string Message { get => this.message; set => this.message = value; }

        public static void TestMefPractices()
        {
            MefPractices mefPractices = new MefPractices();
            mefPractices.TestMefPractices01();
        }

        public void TestMefPractices01() 
        {
            Compose01();
            Debug.WriteLine("abc00");
            Console.WriteLine("abc01");

            if (string.IsNullOrEmpty(this.message))
            {
                throw new Exception("wrong message");
            }
            Console.WriteLine(string.Empty);
        }

        private void Compose01()
        {
            //We are loading the currently-executing assembly
            AssemblyCatalog catalog = new AssemblyCatalog(typeof(MefPractices).Assembly);
            CompositionContainer container = new CompositionContainer(catalog);

            //Here we are hooking up the "plugs"
            //to the "ports".  This is one of the 
            //options to hook everything up.  I've 
            //commented out the other option below.
            container.SatisfyImportsOnce(this);
            //container.ComposeParts(this);
        }
    }
    
    //Step 4
    public class ExportClass01 
    {
        //This is the string property that we are
        //exporting (making it a "plug")
        [Export()]
        public string MyMessage { get; set; } = "This is my example message.";
    }
}
