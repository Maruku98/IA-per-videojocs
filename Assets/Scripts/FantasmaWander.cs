using UnityEngine;
using UnityEngine.AI;

// -----------------------------------------------
// MOVIMENT WANDER DEL FANTASMA
// -----------------------------------------------
public class FantasmaWander : MonoBehaviour
{
    public Collider colliderTerra;                      // Mesh collider del terra
    public NavMeshAgent fantasmaAgent;

    private const float proximitatWander = 0.5f;
    private const float wanderRadi = 2f;
    private const float wanderDistanciaCercle = 3f;

    private void Start()
    {
        Wander();
    }

    private void Update()
    {
        if (fantasmaAgent.remainingDistance < proximitatWander)
        {
            Wander();
        }
    }

    private void Wander()
    {
        Vector3 posicioAleatoria = Random.insideUnitSphere;
        posicioAleatoria.y = 0f;
        posicioAleatoria.Normalize();
        posicioAleatoria *= wanderRadi;
        posicioAleatoria += new Vector3(0, 0, wanderDistanciaCercle);

        Vector3 worldTarget = transform.TransformPoint(posicioAleatoria);
        worldTarget.y = 0f;

        if (colliderTerra.bounds.Contains(worldTarget))
        {
            Seek(worldTarget);
        }
        else
        {
            Seek(transform.TransformPoint(-posicioAleatoria));
        }
    }

    private void Seek(Vector3 posicioTarget)
    {
        fantasmaAgent.destination = posicioTarget;
    }
}
