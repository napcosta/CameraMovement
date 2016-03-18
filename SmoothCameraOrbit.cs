using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class SmoothCameraOrbit : MonoBehaviour {

	public Transform target;
	private Vector3 lookat_pos;
	public Vector3 end_camera_pos;
	public Vector3 end_camera_trans_pos;
	public Vector3 end_camera_trans_lookat;
	public Vector3 end_camera_lookat;
	private bool already_hit = false;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	private float distanceMin = 2.5f;
	private float distanceMax = 15f;

	public float scroll_acceleration = 5f;

	private Rigidbody rigidbody;
	[SerializeField]
	float x = 0.0f;
	[SerializeField]
	float y = 0.0f;

	bool is_rotating;
	bool is_scrolling;

	// Use this for initialization
	void Start () 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		rigidbody = GetComponent<Rigidbody>();

		// Make the rigid body not change rotation
		if (rigidbody != null)
		{
			rigidbody.freezeRotation = true;
		}

		lookat_pos = target.position;
		end_camera_lookat = target.position;
		end_camera_trans_lookat = target.position;
		end_camera_pos = transform.position;
		end_camera_trans_pos = transform.position;

		is_rotating = false;

	}

	void LateUpdate () 
	{
		ListenScrollWheelZoom();
		if (Input.GetMouseButton(0)) {
			
			x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion rotation = Quaternion.Euler(y, x, 0);

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out hit) && !already_hit) {
				end_camera_lookat = hit.transform.position;
			}
			already_hit = true;

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

			end_camera_pos = rotation * negDistance + Vector3.zero;

			transform.rotation = Quaternion.Euler(y, x, 0);
			is_rotating = true;
		}

		if (Input.GetMouseButtonUp(0)) {
			already_hit = false;
		}

		if (Input.GetMouseButton(2)) {
			TranslateCameraAlongPlane(Input.mousePosition);
		}
		//Debug.Log ((transform.position - end_camera_lookat).magnitude + " --- " + end_camera_pos.magnitude) ;
		Debug.Log (transform.position + " --- " + end_camera_pos) ;

		if (is_rotating) {
			transform.position = Vector3.Slerp(transform.position - end_camera_lookat, end_camera_pos, 0.1f) + end_camera_lookat;
			end_camera_trans_pos = transform.position;
		} else {
			transform.position = Vector3.Lerp(transform.position, end_camera_trans_pos, 0.1f);
		}
		lookat_pos = Vector3.Slerp(lookat_pos, end_camera_lookat, 0.1f);

		transform.LookAt(lookat_pos);

	}

	void OnDrawGizmos () {
		// Gizmo Frustum
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(lookat_pos, 1);

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(end_camera_lookat, 1);
	}

	private void ListenScrollWheelZoom()
	{
		Vector3 forward_translation = Camera.main.transform.forward*50*distance*Time.deltaTime*Input.GetAxis("Mouse ScrollWheel");

		//Debug.Log (transform.position + "...." + end_camera_pos);

		if (distance < distanceMin) {
			
			Vector3 new_end_camera_pos = end_camera_pos;
			new_end_camera_pos.Normalize ();
			new_end_camera_pos *= distanceMin;
			end_camera_pos = new_end_camera_pos;

		} else if (distance > distanceMax) {
			
			Vector3 new_end_camera_pos = end_camera_pos;
			new_end_camera_pos.Normalize ();
			new_end_camera_pos *= distanceMax;
			end_camera_pos = new_end_camera_pos;

		} else {
			end_camera_pos += forward_translation;
		}
		//Debug.Log (transform.position + "...." + end_camera_pos + "...." + direction);

		distance = Vector3.Distance(lookat_pos, transform.position);
		//distance = Vector3.Distance (lookat_pos, end_camera_pos);
	}

	private void TranslateCameraAlongPlane(Vector3 mouseOrigin)
	{

		float x = -Input.GetAxis("Mouse X") * xSpeed * distance * 0.001f;
		float y = -Input.GetAxis("Mouse Y") * ySpeed * distance * 0.001f;

		Vector3 direction = target.transform.position - transform.position;
		direction.Normalize();

		Transform camera_transform = Camera.main.transform;

		Vector3 horizontal = camera_transform.right;
		Vector3 vertical = camera_transform.up;
		horizontal.Normalize();
		vertical.Normalize();

		Vector3 horizontal_translation = horizontal*x*20*Time.deltaTime;
		Vector3 vertical_translation = vertical*y*20*Time.deltaTime;

		end_camera_lookat += horizontal_translation;
		end_camera_lookat += vertical_translation;

		end_camera_trans_pos += horizontal_translation;
		end_camera_trans_pos += vertical_translation;

		is_rotating = false;
	}

	private void RotateCameraAroundLookAt()
	{


		
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

 
 
}