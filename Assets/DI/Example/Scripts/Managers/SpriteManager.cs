using DI.Injection;
using DI.Injection.Scope;
using UnityEngine;

[DependencyScope(Scope.Singleton)]
public class SpriteManager : MonoBehaviour, ISpriteManager, IDependency
{
	public void DrawSprite()
	{
		Debug.Log("Drawing sprite...");
	}
}
