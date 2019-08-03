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