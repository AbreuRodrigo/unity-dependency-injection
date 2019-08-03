using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace DI.Injection
{
	public class DIManager : MonoBehaviour
	{
		private static Dictionary<string, object> dependenciesContainer;
		private string dependenciesInjected = string.Empty;

		[SerializeField] private bool debugMode = false;
				
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);

			dependenciesContainer = new Dictionary<string, object>();

			BuildDependencyContainer();
			BindDependenciesToInjectors();
		}

		private void BuildDependencyContainer()
		{
			//Find all objects in the scene implementing IDependency ( these objects will be used as dependencies )
			var dependencies = FindObjectsOfType<MonoBehaviour>().Where(x => typeof(IDependency).IsAssignableFrom(x.GetType()));

			if (debugMode)
			{
				Debug.Log("<color=#0f0> Dependencies: " + dependencies.Count() + "</color>");
			}

			foreach (MonoBehaviour dependency in dependencies)
			{
				if (debugMode)
				{
					Debug.Log("<color=#ff0> Dependency: " + dependency.gameObject.name + "</color>");
				}

				dependenciesContainer?.Add(dependency.GetType().ToString(), dependency);
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