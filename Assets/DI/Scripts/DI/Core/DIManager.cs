using DI.Injection.Scope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace DI.Injection
{
	[ExecuteInEditMode]
	public class DIManager : MonoBehaviour
	{
		private static Dictionary<string, object> monobehaviourSingletonDependencies = new Dictionary<string, object>();
		private static Dictionary<string, object> instanceSingletonDependencies = new Dictionary<string, object>();

		private static List<object> monobehaviourInjectors;

		private string dependenciesInjected = string.Empty;

		[SerializeField] private bool debugMode = false;


		[Header("Assemblies")]
		[SerializeField] private List<AssemblyDefinitionAsset> assemblyReferences = null;

		public long BirthTime { get; private set; }

        void Awake()
        {
            BirthTime = DateTime.Now.Ticks;

			if (!Application.isEditor)
			{
				DontDestroyOnLoad(gameObject);
			}

            DestroyOldest();           

			//Build Monobehaviour Dependencies
            BuildMonobehaviourDependencyContainer();
            BindMonobehaviourDependenciesToInjectors();

			//BuildInstanceDependencyContainer();
			//BindDependenciesToInterfaces();
        }

		private void DestroyOldest()
        {
            List<DIManager> dependencyManagers = FindObjectsOfType<DIManager>().ToList();

            if (1 == dependencyManagers.Count)
            {
                return;
            }

            DIManager firstBorn = dependencyManagers.Where(x => x.BirthTime == dependencyManagers.Min(y => y.BirthTime)).Single();

            if (firstBorn.GetInstanceID() != GetInstanceID())
            {
                Destroy(firstBorn.gameObject);
            }
        }

		private void BindDependenciesToInterfaces()
		{
			// Get all passed assemblies
			if (assemblyReferences != null && assemblyReferences.Count > 0)
			{
				// For each assembly, get all non Monobehaviour types from it, if implementing IInjector
				foreach (TextAsset assemblyDefinition in assemblyReferences)
				{
					Assembly assembly = Assembly.Load(assemblyDefinition.name);

					var types = assembly.GetTypes();
					var injectorTypes = types?.Where(x => typeof(IInjector).IsAssignableFrom(x) && !typeof(MonoBehaviour).IsAssignableFrom(x))
						                      .ToList();

					foreach (var injectorType in injectorTypes)
					{
						object injectorInstance = assembly.CreateInstance(injectorType.FullName);
						
						//List<FieldInfo> injectorFields = injectorType.GetRuntimeFields().ToList();

						var fields = injectorType.GetFields()
							                 .Where(x => x.IsDefined(typeof(InjectAttribute), false))
											 .ToList();

						foreach (FieldInfo field in fields)
						{
							//Get a private field by name from the injection if it's an interface
							FieldInfo fieldRef = injectorType?.GetField(field.Name, BindingFlags.Instance | BindingFlags.NonPublic);

							var fieldType = field.FieldType;

							if (fieldRef != null && fieldType.IsInterface)
							{								
								//Try to find in the dependencyContainer an object by the injected type from the injector
								foreach(var singletonDependency in monobehaviourSingletonDependencies)
								{
									var dependencyValue = singletonDependency.Value;
									var singletonDependencyType = dependencyValue.GetType();

									if (fieldType.IsAssignableFrom(singletonDependencyType))
									{										
										//Set the object reference in the injector to be the same located in the dependencyContainer
										fieldRef?.SetValue(injectorInstance, dependencyValue);

										var val = fieldRef.GetValue(injectorInstance);
									}
								}																								

								if (debugMode)
								{
									dependenciesInjected += (dependenciesInjected != string.Empty ? "," : string.Empty) + fieldRef.FieldType.ToString();
								}
							}
							else
							{
								FieldInfo publicFieldRef = injectorType?.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);

								if (publicFieldRef != null)
								{
									Debug.LogError($"Only non-public fields can be injected as a dependency. Please make sure to change the field { publicFieldRef.Name } to protected or private.");
								}
							}
						}
					}
				}
			}
		}

		private void BuildInstanceDependencyContainer()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly a in assemblies)
			{

			}
		}

		private void BuildMonobehaviourDependencyContainer()
		{
			//Find all objects in the scene implementing IDependency ( these objects will be used as dependencies )
			var dependencies = FindObjectsOfType<MonoBehaviour>()
								.Where(x => typeof(IDependency).IsAssignableFrom(x.GetType()))
								.ToList();

			if (debugMode)
			{
				Debug.Log($"<color=#0f0> Dependencies: {dependencies.Count()}</color>");
			}

            string typeKey = string.Empty;
            string dependencyName = string.Empty;

            foreach (MonoBehaviour dependency in dependencies)
			{
                typeKey = dependency.GetType().ToString();
                dependencyName = dependency.gameObject.name;

				DependencyScopeAttribute ds = dependency.GetType().GetCustomAttribute<DependencyScopeAttribute>();

				Scope.Scope scope = Scope.Scope.Singleton;

				if (null != ds)
				{
					scope = ds.Scope;
				}
								
				if (Application.isPlaying && Scope.Scope.Singleton == scope)
				{
					DontDestroyOnLoad(dependency.gameObject);
				}

				dependencyName += $" (Scope: {scope})";

				if (monobehaviourSingletonDependencies != null && 
					monobehaviourSingletonDependencies.ContainsKey(typeKey))
                {
                    var newest = (MonoBehaviour) monobehaviourSingletonDependencies[typeKey];
                    monobehaviourSingletonDependencies.Remove(typeKey);
					
					if (newest != null && newest.gameObject != null)
					{
						Destroy(newest.gameObject);
					}
                }                

				try
                {
                    monobehaviourSingletonDependencies?.Add(typeKey, dependency);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Dependency Injection Error: Consider using SingletonDependencyAttribute for the type: {typeKey}");
                    Debug.LogError($"Exception Original Message: {e.Message}");
                }

                if (debugMode)
                {
                    Debug.Log($"<color=#ff0> Dependency: {dependencyName} </color>");
                }                
			}
		}

		private void BindMonobehaviourDependenciesToInjectors()
		{
			monobehaviourInjectors = new List<object>();

			//Find all monobehaviours implementing IInject interface		
			var injectors = FindObjectsOfType<MonoBehaviour>()
								.Where(x => typeof(IInjector).IsAssignableFrom(x.GetType()))
								.ToList();

			if (debugMode)
			{
				Debug.Log($"<color=#0f0> Injectors: {injectors.Count()}</color>");
			}

			//For each injector found iterte to get it's own fields with Inject (Attribute)
			foreach (MonoBehaviour injector in injectors)
			{
				monobehaviourInjectors.Add(injector);

				//Get original injector type ( The script on it )
				Type injectorType = injector?.GetType();

				//Get all fields with InjectAttribute in the injector class
				List<FieldInfo> fields = injectorType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
								         .Where(x => x.IsDefined(typeof(InjectAttribute), false))
										 .ToList();

				//Foreach field in the current injector class
				foreach (FieldInfo field in fields)
				{
					//Try to find a given field as non-public (protected / private) by name from the injectorType
					FieldInfo fieldRef = injectorType?.GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Instance);
					
					if (fieldRef != null)
					{
						Type fieldType = fieldRef.FieldType;

						if (fieldType.IsInterface)
						{
							//Try to find in the dependencyContainer an object by the injected type from the injector
							var values = monobehaviourSingletonDependencies.Values.Where(x => fieldType.IsAssignableFrom(x.GetType())).ToList();							
							var reference = (values != null && values.Count > 0) ? values.First() : null;

							//Set the object reference in the injector to be the same located in the dependencyContainer
							fieldRef?.SetValue(injector, reference);

							if (debugMode)
							{
								dependenciesInjected += (dependenciesInjected != string.Empty ? "," : string.Empty) + fieldType.ToString();
							}
						}
					}
					else
					{
						FieldInfo publicFieldRef = injectorType?.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);

						if (publicFieldRef != null)
						{
							Debug.LogError($"Please make sure to change the field { injectorType.Name}.{ publicFieldRef.Name } to protected or private before injecting it.");
						}
					}
				}

				if (debugMode)
				{
					Debug.Log($"<color=#ff0> Injector: {injector.gameObject.name} -> ({dependenciesInjected})" + "</color>");
				}
			}
		}
	}
}