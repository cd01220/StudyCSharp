namespace StudyCSharp
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public class Program
    {
        class Point
        {
            protected int x, y;

            public Point() : this(0, 0)
            { }

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public override bool Equals(Object obj)
            {
                //Check for null and compare run-time types.
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    Point p = (Point)obj;
                    return (this.x == p.x) && (this.y == p.y);
                }
            }

            public override int GetHashCode()
            {
                return (this.x << 2) ^ this.y;
            }

            public override string ToString()
            {
                return string.Format("Point({0}, {1})", this.x, this.y);
            }
        }

        static void Main(string[] args)
        {
            MefPractices.TestMefPractices();
        }
    }
}
