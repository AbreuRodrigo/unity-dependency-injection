#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DI.Injection.Editor
{
    public class DIManagerEditor : MonoBehaviour
    {
        private const int executionOrder = -15999;

        [MenuItem("DependencyInjection/Reset Execution Order")]
        static void SetExecutionOrder()
        {
            if (Application.isEditor)
            {
                MonoBehaviour monobehaviour = FindObjectOfType<DIManager>();

                if (monobehaviour != null)
                {
                    MonoScript monoScript = MonoScript.FromMonoBehaviour(monobehaviour);

                    int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

                    MonoImporter.SetExecutionOrder(monoScript, executionOrder);

                    Debug.Log("<color=#0f0> DI: Execution order was reset successfully!</color>");
                }
                else
                {
                    Debug.Log("<color=#ff0> DI: Please make sure to add a DIManager to your scene before reseting the execution order!</color>");
                }
            }
        }

        [MenuItem("DependencyInjection/Print Scene Dependencies")]
        static void PrintSceneDependencies()
        {
            //Find all objects in the scene implementing IDependency ( these objects will be used as dependencies )
            var dependencies = FindObjectsOfType<MonoBehaviour>().Where(x => typeof(IDependency).IsAssignableFrom(x.GetType()));

            Debug.Log("<color=#0f0> Dependencies: " + dependencies.Count() + "</color>");

            string typeKey = string.Empty;
            string dependencyName = string.Empty;

            foreach (MonoBehaviour dependency in dependencies)
            {
                typeKey = dependency.GetType().ToString();
                dependencyName = dependency.gameObject.name;

                SingletonDependencyAttribute ds = dependency.GetType().GetCustomAttribute<SingletonDependencyAttribute>();
                if (null != ds)
                {
                    dependencyName += "(SingletonDependency)";
                }

                Debug.Log("<color=#ff0> Dependency: " + dependencyName + " </color>");
            }
        }

        [MenuItem("DependencyInjection/Print Scene Injectors")]
        static void PrintSceneInjectors()
        {
            string dependenciesInjected = string.Empty;

            //Find all monobehaviours implementing IInject interface			
            var injectors = FindObjectsOfType<MonoBehaviour>().Where(x => typeof(IInjector).IsAssignableFrom(x.GetType()));

            Debug.Log("<color=#0f0> Injectors: " + injectors.Count() + "</color>");

            //For each injector found iterte to get it's own fields with Inject (Attribute)
            foreach (MonoBehaviour injector in injectors)
            {
                //Get original injector type ( The script on it )
                Type injectorType = injector?.GetType();

                //Get all private fields with InjectAttribute in the injector ( to inject an object the type declared dependency should be private )
                var fields = injectorType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.IsDefined(typeof(InjectAttribute), false));

                foreach (FieldInfo field in fields)
                {
                    //Get a private field by name from the inject
                    FieldInfo fieldRef = injectorType?.GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (fieldRef != null)
                    {
                        dependenciesInjected += (dependenciesInjected != string.Empty ? "," : string.Empty) + fieldRef.FieldType.ToString();
                    }
                }

                Debug.Log("<color=#ff0> Injector: " + injector.gameObject.name + " -> (" + dependenciesInjected + ")" + "</color>");
            }
        }
    }
}
#endif