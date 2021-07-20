using UnityEngine;

// Visualises the minimum translation vectors required to separate apart from other colliders found in a given radius
// Attach to a GameObject that has a Collider attached.
[ExecuteInEditMode]
public class ShowPenetration : MonoBehaviour
{
	private const int MAX_NEIGHBOURS = 16; // maximum amount of collisionNeighbours visualised
	private Collider[] collisionNeighbours;

	public void Start()
	{
		collisionNeighbours = new Collider[MAX_NEIGHBOURS];
	}

	public void OnDrawGizmos()
	{
		Collider thisCollider = GetComponent<Collider>();

		// Use the AABB to find all the colliders we might be overlapping
		int count = Physics.OverlapBoxNonAlloc(transform.position, thisCollider.bounds.extents, collisionNeighbours);

		Vector3 depenetrationVector = Vector3.zero;
		
		for (int i = 0; i < count; i++)
		{
			Collider collisionNeighbour = collisionNeighbours[i];
			
			if (collisionNeighbour == thisCollider)
			{
				// Skip ourself
				continue;
			}
			
			if (Physics.ComputePenetration(
				thisCollider, 
				transform.position, 
				transform.rotation,
				collisionNeighbour, 
				collisionNeighbour.gameObject.transform.position, 
				collisionNeighbour.gameObject.transform.rotation,
				out Vector3 direction, 
				out float distance
			))
			{
				// Draw a line showing the depenetration direction if overlapped
				Debug.DrawRay(transform.position, direction * distance, Color.red);
				// Accumulate all depenetration vectors
				depenetrationVector += (direction * distance);
			}
		}
		
		Debug.DrawLine(transform.position, transform.position + depenetrationVector, Color.blue);
	}
}
