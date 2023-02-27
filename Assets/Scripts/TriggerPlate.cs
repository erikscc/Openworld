using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlate : MonoBehaviour
{
	public static Action<List<GameObject>> OnPPActivated;
	public static Action OnSwitchAtivated;
	public enum TriggerType
	{

		PressurePlate = 0,
		Switch = 1
	}
	public enum TriggerActivity
	{

		EnableObject = 0,
		DisableObject = 1

	}
	public TriggerType triggerType = TriggerType.PressurePlate;

	public List<GameObject> props = new();

	private void OnTriggerEnter(Collider other)
	{

		switch (triggerType)
		{
			case TriggerType.PressurePlate:

				Debug.Log("PPA Activated");
				OnPPActivated?.Invoke(props);

				break;
			case TriggerType.Switch:

				Debug.Log("Switch Activated");
				OnSwitchAtivated?.Invoke();

				break;
		}
	}

}
