using UnityEngine;

namespace TESUnity
{
	public class PlayerComponent : MonoBehaviour
	{
		public float slowSpeed = 3;
		public float normalSpeed = 5;
		public float fastSpeed = 10;
		public float flightSpeedMultiplier = 3;
		public float airborneForceMultiplier = 5;

		public float mouseSensitivity = 3;
		public float minVerticalAngle = -90;
		public float maxVerticalAngle = 90;

		public bool isFlying
		{
			get
			{
				return _isFlying;
			}
			set
			{
				_isFlying = value;

				if(!_isFlying)
				{
					rigidbody.useGravity = true;
				}
				else
				{
					rigidbody.useGravity = false;
				}
			}
		}

		public new GameObject camera;
		public GameObject lantern;
		public GameObject steamVRCam;
		public GameObject steamVRHeadCam;
		public GameObject rightController;
		public GameObject leftController;

		SteamVR_TrackedObject ctR;
		SteamVR_TrackedObject ctL;
		SteamVR_Controller.Device deviceL;
		SteamVR_Controller.Device deviceR;

		private CapsuleCollider capsuleCollider;
		private Rigidbody rigidbody;

		private bool isGrounded;
		private bool _isFlying = false;
		
		private void Start()
		{
			capsuleCollider = GetComponent<CapsuleCollider>();
			rigidbody = GetComponent<Rigidbody>();


			//Temporary change spawn (Felix)
			transform.position = new Vector3 (0, 20f, 0);

			steamVRCam = GameObject.Find ("SteamCamera");
			steamVRHeadCam = GameObject.Find ("Camera (head)");

			steamVRCam.transform.position = new Vector3 (0, 20f, 0);
			transform.parent = steamVRHeadCam.transform;
			transform.localPosition = new Vector3 (0, 0, 0);
			//transform.parent = steamVRCam.transform;
			//transform.localPosition = new Vector3 (0, 0, 0);

			rightController = GameObject.Find ("Controller (right)");
			leftController = GameObject.Find ("Controller (left)");
			ctR = rightController.GetComponent<SteamVR_TrackedObject> ();
			ctL = leftController.GetComponent<SteamVR_TrackedObject> ();



		}
		private void Update()
		{
			//Rotate();

			deviceL = SteamVR_Controller.Input ((int)ctL.index);
			deviceR = SteamVR_Controller.Input ((int)ctR.index);

			Debug.Log (deviceL.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad));



			/*if(Input.GetKeyDown(KeyCode.Tab))
			{
				isFlying = !isFlying;
			}*/


			transform.parent.parent.position += transform.parent.forward*deviceL.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
			transform.parent.parent.position += transform.parent.right*deviceL.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;

			if (deviceR.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
				Vector3 v = new Vector3 (0, 90, 0);
				transform.parent.parent.localEulerAngles += v;
			}

			if (deviceL.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
				Vector3 v = new Vector3 (0, -90, 0);
				transform.parent.parent.localEulerAngles += v;
			}

			/*if(isGrounded && !isFlying && Input.GetKeyDown(KeyCode.Space))
			{
				var newVelocity = rigidbody.velocity;
				newVelocity.y = 5;

				rigidbody.velocity = newVelocity;
			}

			if(Input.GetKeyDown(KeyCode.L))
			{
				var light = lantern.GetComponent<Light>();
				light.enabled = !light.enabled;
			}*/
		}


		/*
		private void FixedUpdate()
		{
			isGrounded = CalculateIsGrounded();

			if(isGrounded || isFlying)
			{
				SetVelocity();
			}
			else if(!isGrounded || !isFlying)
			{
				ApplyAirborneForce();
			}
		}

		private void Rotate()
		{
			var eulerAngles = new Vector3(camera.transform.localEulerAngles.x, transform.localEulerAngles.y, 0);

			// Make eulerAngles.x range from -180 to 180 so we can clamp it between a negative and positive angle.
			if(eulerAngles.x > 180)
			{
				eulerAngles.x = eulerAngles.x - 360;
			}

			var deltaMouse = mouseSensitivity * (new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

			eulerAngles.x = Mathf.Clamp(eulerAngles.x - deltaMouse.y, minVerticalAngle, maxVerticalAngle);
			eulerAngles.y = Mathf.Repeat(eulerAngles.y + deltaMouse.x, 360);

			camera.transform.localEulerAngles = new Vector3(eulerAngles.x, 0, 0);
			transform.localEulerAngles = new Vector3(0, eulerAngles.y, 0);

			//transform.rotation = steamVRHeadCam.transform.rotation;
		}
		private void SetVelocity()
		{
			Vector3 velocity;

			if(!isFlying)
			{
				velocity = transform.TransformVector(CalculateLocalVelocity());
				velocity.y = rigidbody.velocity.y;
			}
			else
			{
				velocity = camera.transform.TransformVector(CalculateLocalVelocity());
			}

			rigidbody.velocity = velocity;
		}
		private void ApplyAirborneForce()
		{
			var forceDirection = transform.TransformVector(CalculateLocalMovementDirection());
			forceDirection.y = 0;

			forceDirection.Normalize();

			var force = airborneForceMultiplier * rigidbody.mass * forceDirection;

			rigidbody.AddForce(force);
		}

		private Vector3 CalculateLocalMovementDirection()
		{
			// Calculate the local movement direction.
			var direction = Vector3.zero;

			direction += Vector3.forward*deviceL.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;
			direction += Vector3.right*deviceL.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;
			

			if(Input.GetKey(KeyCode.S))
			{
				direction += Vector3.back;
			}

			if(Input.GetKey(KeyCode.D))
			{
				direction += Vector3.right;
			}

			return direction.normalized;
		}
		private float CalculateSpeed()
		{
			float speed;

			if(Input.GetKey(KeyCode.LeftShift))
			{
				speed = fastSpeed;
			}
			else if(Input.GetKey(KeyCode.LeftControl))
			{
				speed = slowSpeed;
			}
			else
			{
				speed = normalSpeed;
			}

			if(isFlying)
			{
				speed *= flightSpeedMultiplier;
			}

			return speed;
		}
		private Vector3 CalculateLocalVelocity()
		{
			return CalculateSpeed() * CalculateLocalMovementDirection();
		}

		private bool CalculateIsGrounded()
		{
			var playerCenter = transform.position;
			var castedSphereRadius = 0.8f * capsuleCollider.radius;
			var sphereCastDistance = (capsuleCollider.height / 2);
			
			return Physics.SphereCast(new Ray(playerCenter, -transform.up), castedSphereRadius, sphereCastDistance);
		}*/
	}
}