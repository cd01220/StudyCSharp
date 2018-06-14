namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class ReflectionPractices
    {
        public static void TestReflectionPractices()
        {
            TestReflectionPractices01();
        }

        // using Activator and ConstructorInfo to create instance separately.
        public static void TestReflectionPractices01()
        {
            Type myClassType = typeof(MyClass);

            MyClass myClass01 = (MyClass)Activator.CreateInstance(myClassType, 1);


            ConstructorInfo constructor = myClassType.GetConstructor(new Type[] { typeof(int) });
            MyClass myClass02 = (MyClass)constructor.Invoke(new object[] { 1 });

            Console.WriteLine(string.Empty);
        }


        class MyClass
        {
            private int value;

            public MyClass(int para)
            {
                value = para;
            }
        }
    }
}
