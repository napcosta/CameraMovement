using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class SmoothCameraOrbit : MonoBehaviour {

	public Transform target;
	private Vector3 lookat_pos;
	private Vector3 end_camera_pos;
	private Vector3 end_camera_lookat;
	private bool already_hit = false;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	public float distanceMin = .5f;
	public float distanceMax = 15f;

	public float scroll_acceleration = 5f;

	private Rigidbody rigidbody;

	float x = 0.0f;
	float y = 0.0f;

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
		end_camera_pos = transform.position;
	}

	void LateUpdate () 
	{
	//	if (target) 
	//	{

		if (Input.GetMouseButton(0)) {
			
			x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);
		
			Quaternion rotation = Quaternion.Euler(y, x, 0);

			distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel")*5, distanceMin, distanceMax);



			/*RaycastHit hit;
			if (Physics.Linecast (target.position, transform.position, out hit)) 
			{
				distance -=  hit.distance;
			}*/

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out hit) && !already_hit) {

				already_hit = true;
				Debug.Log("Hit something: " + hit.collider.gameObject.name); 
				//lookat_pos = hit.transform.position;
				//lookat_pos = Vector3.Slerp(lookat_pos, hit.transform.position, 0.01f);
				end_camera_lookat = hit.transform.position;
				//lookat_pos = hit.transform.position;
				//distance = Vector3.Distance(hit.transform.position, transform.position);



			}

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		//	end_camera_pos = rotation * negDistance + target.position;


			end_camera_pos = rotation * negDistance + Vector3.zero;


			transform.rotation = Quaternion.Euler(y, x, 0);

			//transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.1f);


		}

		//Debug.Log(transform.position);

		if (Input.GetMouseButtonUp(0)) {
			already_hit = false;
		}

		//ListenScrollWheelZoom();

		if (Input.GetMouseButton(2)) {
			TranslateCameraAlongPlane(Input.mousePosition);
		}

		transform.position = Vector3.Slerp(transform.position - end_camera_lookat, end_camera_pos, 0.1f) + end_camera_lookat;
		lookat_pos = Vector3.Slerp(lookat_pos, end_camera_lookat, 0.1f);

		//transform.position = end_camera_pos;
		//transform.LookAt(end_camera_lookat);

		transform.LookAt(lookat_pos);
		//DrawFrustum(Camera.main);
		Debug.Log((target.position - transform.position).magnitude);


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
		Vector3 forward_translation = Camera.main.transform.forward*1000*Time.deltaTime*Input.GetAxis("Mouse ScrollWheel");
		end_camera_pos += forward_translation;
	}

	private void TranslateCameraAlongPlane(Vector3 mouseOrigin)
	{
		float x = -Input.GetAxis("Mouse X") * xSpeed * distance * 0.002f;
		float y = -Input.GetAxis("Mouse Y") * ySpeed * distance * 0.002f;

		//Vector3 pos = Camera.main.ScreenToViewportPoint(new Vector3(x,y,0));


		Vector3 direction = target.transform.position - transform.position;
		direction.Normalize();

		//transform.position += new Vector3(direction.x*x, direction.y*y, 0);
		Transform camera_transform = Camera.main.transform;

		Vector3 horizontal = camera_transform.right;
		Vector3 vertical = camera_transform.up;
		horizontal.Normalize();

		Vector3 horizontal_translation = horizontal*x*20*Time.deltaTime;
		Vector3 vertical_translation = vertical*y*20*Time.deltaTime;

		//transform.position += horizontal_translation;
		//transform.position += vertical_translation;

		end_camera_pos += horizontal_translation;
		end_camera_pos += vertical_translation;


		end_camera_lookat += horizontal*x*20*Time.deltaTime;
		end_camera_lookat += vertical*y*20*Time.deltaTime;


		//Debug.Log(Camera.main.transform.forward + " " + transform.position);
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