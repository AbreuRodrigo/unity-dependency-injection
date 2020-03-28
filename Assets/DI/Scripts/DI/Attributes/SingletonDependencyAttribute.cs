using System;

namespace DI.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SingletonDependencyAttribute : Attribute
    {
    }
}
