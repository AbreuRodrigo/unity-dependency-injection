using DI.Injection;
using UnityEngine;

public class SpriteManager : MonoBehaviour, IDependency
{
	public void DrawSprite()
	{
		Debug.Log("Drawing sprite...");
	}
}
