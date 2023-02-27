using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	protected void PopUpAnimation(GameObject props)
	{
		LeanTween.scale(props, new Vector3(3f, 2f, 2f), 2f).setEaseInBounce();
	}

	protected IEnumerator PillarAnimation(GameObject props, float delay = 0.15f)
	{
		yield return new WaitForSeconds(delay);
		LeanTween.scale(props, Vector3.one, 1f).setEase(LeanTweenType.easeInOutBounce);
	}

	protected void CameraMove(GameObject cam, GameObject targetPos, Vector3 offsetPos)
	{
		LeanTween.move(cam, targetPos.transform.position + offsetPos, 3.5f)
			.setOnStart(() =>
			{
				cam.SetActive(true);
			})
			.setOnComplete(() =>
			{
				cam.SetActive(false);
				cam.transform.localPosition = Vector3.zero;
				FirstPersonController.playerCamera.gameObject.SetActive(true);
				targetPos.transform.GetChild(targetPos.transform.childCount - 1).gameObject.SetActive(true);
				var parentGameObject = targetPos.transform.GetChild(targetPos.transform.childCount - 1).gameObject;
				var propsHolder = parentGameObject.transform.GetChild(0).gameObject;
				LeanTween.moveZ(propsHolder, targetPos.transform.position.z + 3.5f, 3f);

			});

		LeanTween.rotate(cam, new Vector3(45f, 90f, 0f), 2f);
	}
}
