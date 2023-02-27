using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
	public enum CameraModes
	{
		FPS = 0,
		TPS = 1
	}
	private Rigidbody rb;

	#region Camera Movement Variables

	[Header("Camera Options")]

	public static Camera playerCamera;
	public CameraModes cameraUse = CameraModes.FPS;
	public static List<Camera> cameraList;
	public float fov = 60f;
	public bool invertCamera = false;
	public bool cameraCanMove = true;
	public float mouseSensitivity = 2f;
	public float maxLookAngle = 50f;
	private bool cinema;

	// Crosshair
	public bool lockCursor = true;
	public bool crosshair = true;
	public Sprite crosshairImage;
	public Color crosshairColor = Color.white;

	// Internal Variables
	private float yaw = 0.0f;
	private float pitch = 0.0f;
	private Image crosshairObject;

	#region Camera Zoom Variables

	public bool enableZoom = true;
	public bool holdToZoom = false;
	public KeyCode zoomKey = KeyCode.Mouse1;
	public float zoomFOV = 30f;
	public float zoomStepTime = 5f;

	// Internal Variables
	private bool isZoomed = false;

	#endregion
	#endregion

	#region Movement Variables

	public bool playerCanMove = true;
	public float walkSpeed = 5f;
	public float maxVelocityChange = 10f;

	// Internal Variables
	private bool isWalking = false;

	#region Sprint

	public bool enableSprint = true;
	public bool unlimitedSprint = false;
	public KeyCode sprintKey = KeyCode.LeftShift;
	public float sprintSpeed = 7f;
	public float sprintDuration = 5f;
	public float sprintCooldown = .5f;
	public float sprintFOV = 80f;
	public float sprintFOVStepTime = 10f;

	// Sprint Bar
	public bool useSprintBar = true;
	public bool hideBarWhenFull = true;
	public Slider sprintBar;
	public float sprintBarWidthPercent = .3f;
	public float sprintBarHeightPercent = .015f;
	public GameObject refillEffect;

	// Internal Variables
	private bool isSprinting = false;
	[SerializeField] private bool isRecoveringStamina;
	private float sprintRemaining;
	private bool isSprintCooldown = false;
	private float sprintCooldownReset;

	#endregion

	#region Jump

	public bool enableJump = true;
	public KeyCode jumpKey = KeyCode.Space;
	public float jumpPower = 5f;

	// Internal Variables
	private bool isGrounded = false;

	#endregion

	#region Crouch

	public bool enableCrouch = true;
	public bool holdToCrouch = true;
	public KeyCode crouchKey = KeyCode.LeftControl;
	public float crouchHeight = .75f;
	public float speedReduction = .5f;

	// Internal Variables
	private bool isCrouched = false;
	private Vector3 originalScale;

	#endregion
	#endregion

	#region Head Bob

	public bool enableHeadBob = true;
	public Transform joint;
	public float bobSpeed = 10f;
	public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

	// Internal Variables
	private Vector3 jointOriginalPos;
	private float timer = 0;

	#endregion

	public static Action<GameObject> OnCinemaModeActivated;

	private void Awake()
	{
		InitOnStart();

		rb = GetComponent<Rigidbody>();

		crosshairObject = GetComponentInChildren<Image>();

		// Set internal variables
		playerCamera.fieldOfView = fov;
		originalScale = transform.localScale;
		jointOriginalPos = joint.localPosition;

		if (!unlimitedSprint)
		{
			sprintRemaining = sprintDuration;
			sprintCooldownReset = sprintCooldown;
		}
	}

	void Start()
	{
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		if (crosshair)
		{
			crosshairObject.sprite = crosshairImage;
			crosshairObject.color = crosshairColor;
		}
		else
		{
			crosshairObject.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		// Control camera movement
		if (cameraCanMove)
		{
			yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

			if (!invertCamera)
			{
				pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
			}
			else
			{
				// Inverted Y
				pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
			}

			// Clamp pitch between lookAngle
			pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

			transform.localEulerAngles = new Vector3(0, yaw, 0);
			playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
		}

		if (enableZoom)
		{
			// Changes isZoomed when key is pressed
			// Behavior for toogle zoom
			if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
			{
				if (!isZoomed)
				{
					isZoomed = true;
				}
				else
				{
					isZoomed = false;
				}
			}

			// Changes isZoomed when key is pressed
			// Behavior for hold to zoom
			if (holdToZoom && !isSprinting)
			{
				if (Input.GetKeyDown(zoomKey))
				{
					isZoomed = true;
				}
				else if (Input.GetKeyUp(zoomKey))
				{
					isZoomed = false;
				}
			}

			// Lerps camera.fieldOfView to allow for a smooth transistion
			if (isZoomed)
			{
				playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
			}
			else if (!isZoomed && !isSprinting)
			{
				playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
			}
		}

		if (enableSprint)
		{
			Sprint();
		}

		// Gets input and calls jump method
		if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
		{
			Jump();
		}

		if (enableCrouch)
		{
			if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
			{
				Crouch();
			}

			if (Input.GetKeyDown(crouchKey) && holdToCrouch)
			{
				isCrouched = false;
				Crouch();
			}
			else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
			{
				isCrouched = true;
				Crouch();
			}
		}

		CheckGround();

		if (enableHeadBob)
		{
			HeadBob();
		}
	}

	void FixedUpdate()
	{
		if (playerCanMove && !cinema)
		{
			// Calculate how fast we should be moving
			Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

			// Checks if player is walking and isGrounded
			// Will allow head bob
			if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
			{
				isWalking = true;
			}
			else
			{
				isWalking = false;
			}

			// All movement calculations while sprint is active
			if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown && !isRecoveringStamina)
			{
				targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

				// Apply a force that attempts to reach our target velocity
				Vector3 velocity = rb.velocity;
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;

				// Player is only moving when valocity change != 0
				// Makes sure fov change only happens during movement
				if (velocityChange.x != 0 || velocityChange.z != 0)
				{
					isSprinting = true;

					if (isCrouched)
					{
						Crouch();
					}
				}

				rb.AddForce(velocityChange, ForceMode.VelocityChange);
			}
			// All movement calculations while walking
			else
			{
				isSprinting = false;
				targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

				// Apply a force that attempts to reach our target velocity
				Vector3 velocity = rb.velocity;
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;

				rb.AddForce(velocityChange, ForceMode.VelocityChange);
			}
		}

	}

	// Sets isGrounded based on a raycast sent straigth down from the player object
	private void CheckGround()
	{
		Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
		Vector3 direction = transform.TransformDirection(Vector3.down);
		float distance = .75f;

		if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
		{
			Debug.DrawRay(origin, direction * distance, Color.red);
			isGrounded = true;
		}
		else
		{
			isGrounded = false;
		}
	}

	private void Jump()
	{
		// Adds force to the player rigidbody to jump
		if (isGrounded)
		{
			rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
			isGrounded = false;
		}

		// When crouched and using toggle system, will uncrouch for a jump
		if (isCrouched && !holdToCrouch)
		{
			Crouch();
		}
	}

	private void Crouch()
	{
		// Stands player up to full height
		// Brings walkSpeed back up to original speed
		if (isCrouched)
		{
			transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
			walkSpeed /= speedReduction;

			isCrouched = false;
		}
		// Crouches player down to set height
		// Reduces walkSpeed
		else
		{
			transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
			walkSpeed *= speedReduction;

			isCrouched = true;
		}
	}

	private void HeadBob()
	{
		if (isWalking)
		{
			// Calculates HeadBob speed during sprint
			if (isSprinting)
			{
				timer += Time.deltaTime * (bobSpeed + sprintSpeed);
			}
			// Calculates HeadBob speed during crouched movement
			else if (isCrouched)
			{
				timer += Time.deltaTime * (bobSpeed * speedReduction);
			}
			// Calculates HeadBob speed during walking
			else
			{
				timer += Time.deltaTime * bobSpeed;
			}
			// Applies HeadBob movement
			joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
												jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
												jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z
											 );
		}
		else
		{
			// Resets when play stops moving
			timer = 0;
			joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x,
												jointOriginalPos.x, Time.deltaTime * bobSpeed),
												Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y,
												Time.deltaTime * bobSpeed),
												Mathf.Lerp(joint.localPosition.z,
												jointOriginalPos.z, Time.deltaTime * bobSpeed)
											 );
		}
	}

	private void Sprint()
	{
		if (isSprinting)
		{
			isZoomed = false;
			playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

			// Drain sprint remaining while sprinting
			if (!unlimitedSprint)
			{
				sprintRemaining -= 1 * Time.deltaTime;
				if (sprintRemaining <= 0)
				{
					isSprinting = false;
					isSprintCooldown = true;
				}
			}
		}
		else
		{
			// Regain sprint while not sprinting
			if (isRecoveringStamina)
			{
				refillEffect.SetActive(true);
			}
			sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);

		}

		// Handles sprint cooldown 
		// When sprint remaining == 0 stops sprint ability until hitting cooldown
		if (isSprintCooldown)
		{
			sprintCooldown -= 1 * Time.deltaTime;
			if (sprintCooldown <= 0)
			{
				//Debug.LogErrorFormat("All stamina used {0}",sprintCooldown);
				isSprintCooldown = false;
				isRecoveringStamina = true;
			}
		}
		else
		{
			sprintCooldown = sprintCooldownReset;
		}

		// Handles sprintBar 
		if (useSprintBar && !unlimitedSprint)
		{
			float sprintRemainingPercent = sprintRemaining / sprintDuration;
			sprintBar.value = sprintRemainingPercent;
			//Debug.Log(sprintRemainingPercent);
			if (sprintRemainingPercent > 0.98f)
			{
				refillEffect.SetActive(false);
				//Debug.LogError("Stamina fully recovered");
				isRecoveringStamina = false;
			}
		}
	}
	private void InitOnStart()
	{
		cameraList = FindObjectsOfType<Camera>().ToList();
		foreach (var item in cameraList)
		{
			item.gameObject.SetActive(false);
			Debug.LogFormat("item in CameraList {0}",item);
		}
		playerCamera = cameraList[(int)cameraUse];
		playerCamera.gameObject.SetActive(true);
	}
}
