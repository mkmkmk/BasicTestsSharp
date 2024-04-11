using Autofac;

namespace NUnitTests;

public class AutofacTests
{
    private class MyClass
    {
        private readonly string id = Guid.NewGuid().ToString();

        public override string ToString() => id;
    }

    [Test]
    public void KeyedTest1()
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterType<MyClass>()
            .InstancePerLifetimeScope()
            .Keyed<MyClass>(1)
            .Keyed<MyClass>(2);

        builder
            .RegisterType<MyClass>()
            .InstancePerLifetimeScope()
            .Keyed<MyClass>(3)
            .Keyed<MyClass>(4);


        var container = builder.Build();

        var keys = Enumerable.Range(1, 4).ToArray();

        var res = keys.Select(key => new { Key = key, Class = container.ResolveKeyed<MyClass>(key) }).ToArray();

        foreach (var el in res)
            Console.WriteLine(el);

        Assert.Multiple(() =>
        {
            Assert.That(res[0].Class, Is.EqualTo(res[1].Class));
            Assert.That(res[2].Class, Is.EqualTo(res[3].Class));
        });


    }
}