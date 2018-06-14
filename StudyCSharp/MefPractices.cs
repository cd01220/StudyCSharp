namespace StudyCSharp
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;

    class MefPractices
    {
        public static void TestMefPractices()
        {
            TestMefPractices01();
        }

        public static void TestMefPractices01() 
        {
            MefPractices mefPractices = new MefPractices();

            var myImport = new MyClass() { MajorRevision = 1 };
            var myExport = new MyExportClass();
            Console.WriteLine("{0}, {1}", myImport.MajorRevision, myExport.MajorRevision);

            mefPractices.Compose(myImport, myExport);

            myExport.MajorRevision = 10;
            Console.WriteLine("{0}, {1}", myImport.MajorRevision, myExport.MajorRevision);

            Console.WriteLine(string.Empty);
        }

        private void Compose(MyClass myClass, MyExportClass myExportClass)
        {
            var catalog = new AssemblyCatalog(typeof(MefPractices).Assembly);
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(myClass, myExportClass);
        }
    }

    public class MyClass
    {
        [Import("MajorRevision")]
        public int MajorRevision { get; set; }
    }

    public class MyExportClass
    {
        [Export("MajorRevision")] //This one will match.  
        public int MajorRevision = 4;

        [Export("MinorRevision")]
        public int MinorRevision = 16;
    }
}
