using System.Diagnostics;

namespace NUnitTests;

public class AsyncAwaitTests
{

    private class MyClass
    {
        public event Func<Task> AsyncEvent = async delegate { };

        public void CallEvents()
        {
            AsyncEvent();
        }
    }

    [Test]
    public void AsyncEventsTest()
    {
        var my = new MyClass();
        int id = 0;
        my.AsyncEvent += async () => Trace.WriteLine($"Handler {id++}");
        my.AsyncEvent += async () => Trace.WriteLine($"Handler {id++}");
        my.CallEvents();
        Thread.Sleep(100);
        Assert.That(id, Is.EqualTo(2));

    }

}