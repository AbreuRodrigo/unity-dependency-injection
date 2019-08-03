# unity_dependency_injection
A simple dependency injection system for Unity

The intention of this simple unity plugin is to facilitate the injection of components into your Injector classes. It's a very simple project with lots of space for improvements, the main idea behind it right now is to avoid singletons and dependency components passed as objects through inspector.

# Usage
There's an example folder showing how to use it and a sample scene "ExampleScene".

Simply add the DIManager prefab to your scene, and then you should click on the menu item "DependencyInjection >> Reset Execution Order".

To make your class a dependency it should implement the interface <b><i>IDependency</i></b>, and to make a your class an injector, your should make it implement <b><i>IInjector</i></b>, and the instences of your dependencies in your injector should be annotated with <b><i>Inject</i></b>, that's it.

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

	public class AudioManager : MonoBehaviour, IDependency
	{
		public void PlayAudio()
		{
			Debug.Log("Playing audio...");
		}
	}

	using DI.Injection;
	using UnityEngine;

	public class SpriteManager : MonoBehaviour, IDependency
	{
		public void DrawSprite()
		{
			Debug.Log("Drawing sprite...");
		}
	}
