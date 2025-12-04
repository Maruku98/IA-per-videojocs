using UnityEngine;
using UnityEngine.AI;

// -----------------------------------------------
// MOVIMENT PATROL DE LA BOLA FANTASMA DELS SOLDATS
// -----------------------------------------------
public class BolaPatrol : MonoBehaviour
{
    // ============== COMPONENTS ==============
    private NavMeshAgent bolaAgent;                         // Component NavMeshAgent de la bola

    // =============== CONSTANTS ==============
    private const float proximitatPatrol = 0.2f;            // Distancia de canvi de waypoint

    // ================ ESTATS ================
    private int indexWaypointActual = 1;                    // Index ddl waypoint actual

    // ============== VARIABLES ===============
    public Transform[] waypoints;                           // Waypoints del recorregut


    // ============== ACTUALITZACIONS =========
    private void Start()
    {
        bolaAgent = GetComponent<NavMeshAgent>();
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


    // =============== FUNCIONS ==================
    private void Seek()
    {
        bolaAgent.destination = waypoints[indexWaypointActual].position;
    }
}
