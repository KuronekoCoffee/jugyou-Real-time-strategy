using UnityEngine;
using UnityEngine.AI;

public class RTSUnit : MonoBehaviour
{
    private NavMeshAgent agent;

    public bool IsSelected { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Select()
    {
        IsSelected = true;
        Debug.Log(gameObject.name + " ‚ð‘I‘ð");
    }

    public void Deselect()
    {
        IsSelected = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
}