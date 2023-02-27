using System.Collections.Generic;
using UnityEngine;

public class OpenWorldManager : AnimationManager
{
	//PP => Pressure Plate 
	private void OnEnable()
	{
		TriggerPlate.OnPPActivated += PPActivated;
		FirstPersonController.OnCinemaModeActivated += CinemaMode;
	}
	private void OnDisable()
	{
		TriggerPlate.OnPPActivated -= PPActivated;
		FirstPersonController.OnCinemaModeActivated -= CinemaMode;
	}

	private void CinemaMode(GameObject target)
	{
		foreach (var item in FirstPersonController.cameraList)
		{
			item.gameObject.SetActive(false);
		}
		var cinemaCams = FirstPersonController.cameraList.Find(x => x.CompareTag("CC"));
		CameraMove(cinemaCams.gameObject, target, new Vector3(-6f, 8f, -1f));
	}

	private void PPActivated(List<GameObject> props)
	{
		if (props != null)
		{
			CinemaMode(props[0]);
			foreach (var item in props)
			{
				item.SetActive(true);
				var stepDelay = item.transform.childCount - 1;
				for (int i = 0; i < item.transform.childCount - 1; i++)
				{
					stepDelay--;
					float delay = stepDelay * 0.08f;
					StartCoroutine(PillarAnimation(item.transform.GetChild(i).gameObject, delay));
				}
			}
		}
	}
}
