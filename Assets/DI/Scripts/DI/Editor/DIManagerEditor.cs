#if UNITY_EDITOR

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
	}
}
#endif