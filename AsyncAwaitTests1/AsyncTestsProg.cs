// examples from 'C# in a Nutshell'

internal class AsyncTestsProg
{
    private static void Main()
    {
        AsyncTestsProg program = new();
        program.Test01();
        program.Test02();
        program.Test03();
        program.Test04();
        program.Test05();
        program.Test06();
        program.Test07();
        program.Test08();
    }


    private static Task<int> GetPrimesCountAsync(int start, int count)
    {
        return Task.Run(
            () =>
                ParallelEnumerable
                    .Range(start, count)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
            );
    }


    private async void DisplayPrimeCounts()
    {
        for (int i = 0; i < 10; i++)
        {
            int count = await GetPrimesCountAsync(i * 1000000 + 2, 1000000);
            Console.WriteLine(count);
        }
    }


    public void Test01()
    {
        DisplayPrimeCounts();
        // (zbyt krótki sleep --> nie wszystkie się obliczą)
        Thread.Sleep(1000);
        Console.WriteLine($"-- {nameof(Test01)} done --");
    }


    public void Test02()
    {

        var awaiter =
            Task
                .Run(() => throw new Exception())
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter();

        awaiter.OnCompleted(
            () =>
            {
                try
                {
                    awaiter.GetResult(); // re-throw
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                Console.WriteLine("..done");
            });

        Thread.Sleep(1000);
        Console.WriteLine($"-- {nameof(Test02)} done --");
    }


    public void Test03()
    {
        Task
            .Run(() => throw new Exception())
            .ContinueWith(
                task =>
                {
                    try
                    {
                        task.GetAwaiter().GetResult(); // re-throw
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Console.WriteLine("..done");
                });
        Thread.Sleep(1000);
        Console.WriteLine($"-- {nameof(Test03)} done --");
    }


    private static async Task MyAsyncFunc1()
    {
        await Task.Run(() => Console.WriteLine("lala"));
        throw new Exception();
    }


    public void Test04()
    {
        try
        {
            // MyFunc().GetAwaiter().GetResult();
            MyAsyncFunc1().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine($"-- {nameof(Test04)} done --");
    }


    private static async Task MyAsyncFunc2()
    {
        int res = await Task.Run(
            () =>
            {
                Thread.Sleep(2000);
                return 5;
            });
        Console.WriteLine($"result == {res}");
    }


    public void Test05()
    {
        Task task = MyAsyncFunc2();
        Console.WriteLine("waiting...");
        task.Wait();
        Console.WriteLine($"-- {nameof(Test05)} done --");
    }


    public void Test06()
    {
        if (SynchronizationContext.Current is null)
            Console.WriteLine("SynchronizationContext.Current is null");
        Console.WriteLine($"-- {nameof(Test06)} done --");
    }


    private async static Task TaskWithCancellation(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
            await Task.Delay(200, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }


    public void Test07()
    {
        var cancelSource = new CancellationTokenSource();
        Task.Delay(1000).ContinueWith(ant => cancelSource.Cancel());
        var task = TaskWithCancellation(cancelSource.Token).ConfigureAwait(false);
        try
        {
            task.GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cancelled");
            Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
        }
        Console.WriteLine($"-- {nameof(Test07)} done --");
    }


    private static async Task DoWhenAny()
    {
        // async Task<int> Delay1() { await Task.Delay(1000); return 1; }
        async Task<int> Delay1() { await Task.Delay(1000); throw new Exception("test"); }
        async Task<int> Delay2() { await Task.Delay(2000); return 2; }
        async Task<int> Delay3() { await Task.Delay(3000); return 3; }

        try
        {
            Task<int> winningTask = await Task.WhenAny(Delay1(), Delay2(), Delay3());
            Console.WriteLine(await winningTask);
            Console.WriteLine("Done");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
        }
    }


    public void Test08()
    {
        DoWhenAny().Wait();
        Console.WriteLine($"-- {nameof(Test08)} done --");
    }

}
