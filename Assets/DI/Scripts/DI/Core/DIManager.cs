using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DI.Injection
{
	public class DIManager : MonoBehaviour
	{
		private static Dictionary<string, object> dependenciesContainer;
		private string dependenciesInjected = string.Empty;

		[SerializeField] private bool debugMode = false;

        public long BirthTime { get; private set; }

        void Awake()
        {
            BirthTime = DateTime.Now.Ticks;

            DontDestroyOnLoad(gameObject);

            DestroyOldest();
            
            dependenciesContainer = new Dictionary<string, object>();
            BuildDependencyContainer();
            BindDependenciesToInjectors();                        
        }

        private void DestroyOldest()
        {
            List<DIManager> dis = FindObjectsOfType<DIManager>().ToList();

            if (1 == dis.Count)
            {
                return;
            }

            DIManager firstBorn = dis.Where(x => x.BirthTime == dis.Min(y => y.BirthTime)).Single();

            if (firstBorn.GetInstanceID() != GetInstanceID())
            {
                Destroy(firstBorn.gameObject);
            }
        }

		private void BuildDependencyContainer()
		{
			//Find all objects in the scene implementing IDependency ( these objects will be used as dependencies )
			var dependencies = FindObjectsOfType<MonoBehaviour>().Where(x => typeof(IDependency).IsAssignableFrom(x.GetType()));

			if (debugMode)
			{
				Debug.Log("<color=#0f0> Dependencies: " + dependencies.Count() + "</color>");
			}

            string typeKey = string.Empty;
            string dependencyName = string.Empty;

            foreach (MonoBehaviour dependency in dependencies)
			{
                typeKey = dependency.GetType().ToString();
                dependencyName = dependency.gameObject.name;

                SingletonDependencyAttribute ds = dependency.GetType().GetCustomAttribute<SingletonDependencyAttribute>();
                if (null != ds)
                {
                    DontDestroyOnLoad(dependency.gameObject);
                    dependencyName += "(SingletonDependency)";

                    if (dependenciesContainer.ContainsKey(typeKey))
                    {
                        var newest = (MonoBehaviour) dependenciesContainer[typeKey];
                        dependenciesContainer.Remove(typeKey);
                        Destroy(newest.gameObject);
                    }
                }

                try
                {
                    dependenciesContainer?.Add(typeKey, dependency);
                }
                catch(Exception e)
                {
                    Debug.Log("<color=#f00> Depencency Injection Error: Consider using SingletonDependencyAttribute for the type: " + typeKey + "</color>");
                    Debug.Log("<color=#f00> Exception Original Message: " + e.Message + "</color>");
                }

                if (debugMode)
                {
                    Debug.Log("<color=#ff0> Dependency: " + dependencyName + " </color>");
                }                
			}
		}

		private void BindDependenciesToInjectors()
		{
			//Find all monobehaviours implementing IInject interface			
			var injectors = FindObjectsOfType<MonoBehaviour>().Where(x => typeof(IInjector).IsAssignableFrom(x.GetType()));

			if (debugMode)
			{
				Debug.Log("<color=#0f0> Injectors: " + injectors.Count() + "</color>");
			}

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
						object reference = null;

						//Try to find in the dependencyContainer an object by the injected type from the injector
						dependenciesContainer?.TryGetValue(fieldRef.FieldType.ToString(), out reference);

						//Set the object reference in the injector to be the same located in the dependencyContainer
						fieldRef?.SetValue(injector, reference);

						if (debugMode)
						{
							dependenciesInjected += (dependenciesInjected != string.Empty ? "," : string.Empty) + fieldRef.FieldType.ToString();
						}
					}
				}

				if (debugMode)
				{
					Debug.Log("<color=#ff0> Injector: " + injector.gameObject.name + " -> (" + dependenciesInjected + ")" + "</color>");
				}
			}
		}
	}
}