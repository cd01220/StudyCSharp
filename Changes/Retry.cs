using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Changes
{
    class Retry
    {
        public static async Task<object> RetryAsync(Func<Task<object>> toTry, int tries, CancellationToken token)
        {
            for (int i = 0; i < tries; ++i)
            {
                try
                {
                    return await toTry();
                }
                catch (TaskCanceledException exp)
                {
                    if (exp.CancellationToken == token || i + 1 == tries)
                    {
                        throw exp;
                    }
                }
            }

            throw new InvalidOperationException("This should never happen!");
        }
    }
}
