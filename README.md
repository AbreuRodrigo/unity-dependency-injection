# Unity Dependency Injection
A simple dependency injection framework for Unity

The intention of this simple unity plugin is to facilitate the injection of components into your Injector classes. It has lots of space for improvements, the main idea behind it right now is to avoid singletons and dependency components passed as objects through inspector.

# Usage
There's an example folder showing how to use it and a sample scene "ExampleScene".

Simply add the DIManager prefab to your scene, and then you should click on the menu item "DependencyInjection >> Reset Execution Order".

To make your class a dependency it should implement the interface <b><i>IDependency</i></b>, and to make your class an injector, your should make it implement <b><i>IInjector</i></b>, and the instances of your dependencies in your injector should be annotated with <b><i>Inject</i></b>, that's it.

It's also important to mention that a dependency reference in an injector should always be non-public.

Example:

	using UnityEngine;
	using DI.Injection;

	public class GameManager : MonoBehaviour, IInjector
	{
		[Inject] private AudioManager audioManager;
		[Inject] private SpriteManager spriteManager;

		public void Start()
		{
			//Testing the dependencies injected
			audioManager?.PlayAudio();
			spriteManager?.DrawSprite();
		}
	}

	using DI.Injection;
	using UnityEngine;

	[DependencyScope(Scope.Singleton)]
	public class AudioManager : MonoBehaviour, IDependency
	{
		public void PlayAudio()
		{
			Debug.Log("Playing audio...");
		}
	}

	using DI.Injection;
	using UnityEngine;

	[DependencyScope(Scope.Singleton)]
	public class SpriteManager : MonoBehaviour, IDependency
	{
		public void DrawSprite()
		{
			Debug.Log("Drawing sprite...");
		}
	}
