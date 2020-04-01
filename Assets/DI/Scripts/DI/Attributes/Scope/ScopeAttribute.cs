using System;

namespace DI.Injection.Scope
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DependencyScopeAttribute : Attribute
    {
        public Scope Scope { get; private set; }

        public DependencyScopeAttribute(Scope scope = Scope.Singleton)
        {
            Scope = scope;
        }

        public void SetScope(Scope scope)
        {
            Scope = scope;
        }
    }
}