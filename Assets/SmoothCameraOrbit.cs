using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class SmoothCameraOrbit : MonoBehaviour {

	public Transform target;
	public Vector3 lookat_pos;
	public Vector3 end_camera_pos;
	public Vector3 end_camera_trans_pos;
	public Vector3 end_camera_trans_lookat;
	public Vector3 end_camera_lookat;
	private bool already_hit = false;
	public float distance = 5.0f;
	public float xSpeedRotation = 120.0f;
	public float ySpeedRotation = 120.0f;
	public float xSpeedTranslation = 120.0f;
	public float ySpeedTranslation = 120.0f;
	private bool has_rotated = false;
	private float yMinLimit = -80f;
	private float yMaxLimit = 80f;

	public float distanceMin = 3f;
	public float distanceMax = 20f;

	public float scroll_acceleration = 5f;

	public Texture2D regular_cursor_texture;
	public Texture2D translation_cursor_texture;
	public Texture2D rotate_cursor_texture;

	public CursorMode cursorMode = CursorMode.ForceSoftware;
	public Vector2 hotSpot = Vector2.zero;

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
		Cursor.SetCursor(regular_cursor_texture, hotSpot, cursorMode);
	}


	void LateUpdate () 
	{
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Break();
		}

		ListenScrollWheelZoom();
		Cursor.SetCursor(regular_cursor_texture, hotSpot, cursorMode);
		if (Input.GetMouseButton(0)) {
			RotateCameraAlongPlane();
		}

		if (Input.GetMouseButtonUp(0)) {
			already_hit = false;

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out hit) && !already_hit && !has_rotated) {
				end_camera_lookat = hit.point;
			}
			has_rotated = false;

		}

		if (Input.GetMouseButton(2)) {
			TranslateCameraAlongPlane(Input.mousePosition);
		}

		if (is_rotating) {
			transform.position = Vector3.Slerp(transform.position - end_camera_lookat, end_camera_pos, 0.1f) + end_camera_lookat;
			end_camera_trans_pos = transform.position;
			lookat_pos = Vector3.Lerp(lookat_pos, end_camera_lookat, 0.1f);
		} else {
			transform.position = Vector3.Lerp(transform.position, end_camera_trans_pos, 0.1f);
			lookat_pos = Vector3.Lerp(lookat_pos, end_camera_lookat, 0.1f);
		}
		//lookat_pos = Vector3.Slerp(lookat_pos, end_camera_lookat, 0.1f);
		transform.LookAt(lookat_pos);
	}

	private void RotateCameraAlongPlane()
	{
		Cursor.SetCursor(rotate_cursor_texture, hotSpot, cursorMode);

		x += Input.GetAxis("Mouse X") * xSpeedRotation ;
		y -= Input.GetAxis("Mouse Y") * ySpeedRotation;

		y = ClampAngle(y, yMinLimit, yMaxLimit);
		Quaternion rotation = Quaternion.Euler(y, x, 0);

		already_hit = true;

		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

		end_camera_pos = rotation * negDistance + Vector3.zero;

		transform.rotation = Quaternion.Euler(y, x, 0);

		is_rotating = true;
		if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
			has_rotated = true;
		}

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

		Vector3 forward_translation = Camera.main.transform.forward*scroll_acceleration*distance*Time.deltaTime*Input.GetAxis("Mouse ScrollWheel");
		if((Input.GetAxis("Mouse ScrollWheel") != 0)) {
			is_rotating = false;
		}
		if (distance < distanceMin ) {

			Vector3 new_end_camera_pos = Vector3.ClampMagnitude(end_camera_trans_pos - end_camera_lookat, distanceMin);
			end_camera_trans_pos = new_end_camera_pos + end_camera_lookat;

			if(Input.GetAxis("Mouse ScrollWheel") < 0) {
				end_camera_trans_pos += forward_translation;
			}

		} else if (distance > distanceMax ) {

			Vector3 new_end_camera_pos = Vector3.ClampMagnitude(end_camera_trans_pos - end_camera_lookat, distanceMax);
			end_camera_trans_pos = new_end_camera_pos + end_camera_lookat;

			if(Input.GetAxis("Mouse ScrollWheel") > 0) {
				end_camera_trans_pos += forward_translation;
			}

		} else {
			end_camera_trans_pos += forward_translation;
		}

		distance = Vector3.Distance (lookat_pos, end_camera_trans_pos);
	}

	private void TranslateCameraAlongPlane(Vector3 mouseOrigin)
	{
		Cursor.SetCursor(translation_cursor_texture, hotSpot, cursorMode);

		float x = -Input.GetAxis("Mouse X") * xSpeedTranslation * distance;
		float y = -Input.GetAxis("Mouse Y") * ySpeedTranslation * distance;

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
		has_rotated = false;
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