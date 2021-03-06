using System;
using System.Threading.Tasks;

namespace FG.ServiceFabric.Testing.Assertion
{
    public static class AsyncAssertions
    {
        public static async Task<bool> WaitForAsyncOperationToFinish(int timeOut, int interval, Func<int, bool> check)
        {
            int timer = 0;
            int index = 0;
            while (timer < timeOut)
            {
                var result = check(index);
                if (result ) return true;

                await Task.Delay(interval);
                timer += interval;
                index++;
            }

            return false;
        }
    }
}