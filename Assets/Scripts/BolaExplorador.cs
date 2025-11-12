using UnityEngine;
using UnityEngine.AI;

// -----------------------------------------------
// MOVIMENT PATROL DE LA BOLA FANTASMA DE L'EXPLORADOR
// -----------------------------------------------
public class BolaExplorador : MonoBehaviour
{
    public NavMeshAgent bolaAgent;
    public Transform[] waypoints;

    private const float proximitatPatrol = 0.2f;
    private int indexWaypointActual = 0;

    private void Start()
    {
        Seek();
    }

    private void Update()
    {
        if (bolaAgent.remainingDistance < proximitatPatrol)
        {
            indexWaypointActual = (indexWaypointActual + 1) % waypoints.Length;
            Seek();
        }
    }

    private void Seek()
    {
        bolaAgent.destination = waypoints[indexWaypointActual].position;
    }
}
