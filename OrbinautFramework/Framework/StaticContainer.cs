using DryIoc;

namespace OrbinautFramework3.Framework;

public static class StaticContainer
{
    private static IContainer _container;

    public static void Set(IContainer container)
    {
        _container = container;
    }

    public static void Inject(object instance)
    {
        _container.InjectPropertiesAndFields(instance);
    }
}