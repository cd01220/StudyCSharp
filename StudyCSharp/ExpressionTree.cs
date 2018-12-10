namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ExpressionTree
    {
        public static void TestExpressionTree()
        {
            ExpressionTree01();
        }

        public static void ExpressionTree01()
        {
            var nArgument = Expression.Parameter(typeof(int), "n");
            var result = Expression.Variable(typeof(int), "result");

            // Creating a label that represents the return value
            LabelTarget label = Expression.Label(typeof(int));

            var initializeResult = Expression.Assign(result, Expression.Constant(1));

            // This is the inner block that performs the multiplication,
            // and decrements the value of 'n'
            var block = Expression.Block(
                Expression.Assign(result,
                    Expression.Multiply(result, nArgument)),
                Expression.PostDecrementAssign(nArgument)
            );

            // Creating a method body.
            BlockExpression body = Expression.Block(
                new[] { result },
                initializeResult,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.GreaterThan(nArgument, Expression.Constant(1)),
                        block,
                        Expression.Break(label, result)
                    ),
                    label
                )
            );

            foreach (var expr in body.Expressions)
                Console.WriteLine(expr.ToString());
            
            //// Create a lambda expression.  
            Expression<Func<int, int>> expression = Expression.Lambda<Func<int, int>>(body, nArgument);

            Func<int, int> compiledExpression = expression.Compile();
            int v = compiledExpression(3);
            Console.WriteLine($"result = {v}");
        }
    }
}
