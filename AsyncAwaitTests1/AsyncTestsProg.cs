// examples from 'C# in a Nutshell'

internal class AsyncTestsProg
{
    private static void Main()
    {
        AsyncTestsProg program = new();
        // program.Test01();
        // program.Test02();
        // program.Test03();
        // program.Test04();
        // program.Test05();
        // program.Test06();
        // program.Test07();
        // program.Test08();
        program.Test09();
        program.Test10();
    }


    private static Task<int> Test01GetPrimesCountAsync(int start, int count)
        => Task.Run(
            () =>
                ParallelEnumerable
                    .Range(start, count)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
                );


    private static async void Test01DisplayPrimeCounts()
    {
        for (int i = 0; i < 10; i++)
        {
            int count = await Test01GetPrimesCountAsync(i * 1000000 + 2, 1000000);
            Console.WriteLine(count);
        }
    }


    public void Test01()
    {
        Test01DisplayPrimeCounts();
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


    private static async Task Test04Task()
    {
        await Task.Run(() => Console.WriteLine("lala"));
        throw new Exception();
    }


    public void Test04()
    {
        try
        {
            // Test04Async().GetAwaiter().GetResult();
            Test04Task().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine($"-- {nameof(Test04)} done --");
    }


    private static async Task Test05Task()
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
        Task task = Test05Task();
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


    // modified version, without throw and with catch of throw in Task.Delay
    private async static Task Test07Task(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
            // await przechwytuje wyjątki
            try
            {
                await Task.Delay(200, cancellationToken);
            }
            catch (Exception)
            {
                Console.WriteLine("(delay exception caught)");
                //Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
                break;
            }
            // cancellationToken.ThrowIfCancellationRequested();
            if (cancellationToken.IsCancellationRequested)
                break;
        }
    }


    public void Test07()
    {
        var cancelSource = new CancellationTokenSource();
        Task.Delay(1000).ContinueWith(ant => cancelSource.Cancel());
        var task = Test07Task(cancelSource.Token).ConfigureAwait(false);
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


    private static async Task Test08Task()
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
        Test08Task().Wait();
        Console.WriteLine($"-- {nameof(Test08)} done --");
    }


    private static Task<int> Test09Task()
    {
        static int GetResult()
        {
            Thread.Sleep(2000);
            return 666;
        }
        return Task.FromResult(GetResult());
    }


    public void Test09()
    {
        Console.WriteLine($"-- {nameof(Test09)} start --");
        var task = Task.Run(async () => Console.WriteLine($"result = {await Test09Task()}"));
        Console.WriteLine("something");
        task.Wait();
        Console.WriteLine($"-- {nameof(Test09)} done --");
    }

    private static async Task<int> Test10Task()
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<int> GetResult()
        {
            Thread.Sleep(1000);
            return 666;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        return await GetResult();
    }


    public void Test10()
    {
        Console.WriteLine($"-- {nameof(Test10)} start --");
        var task = Task.Run(async () => Console.WriteLine($"result = {await Test10Task()}"));
        Console.WriteLine("something");
        task.Wait();
        Console.WriteLine($"-- {nameof(Test10)} done --");
    }
}
