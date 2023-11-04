using UnityEngine;
using UnityEngine.AI;

public class PlayerControl : MonoBehaviour
{
    public Camera camera;
    public NavMeshAgent agent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(ray, out raycastHit))
            {
                agent.SetDestination(raycastHit.point);
            }
        }
    }
}
