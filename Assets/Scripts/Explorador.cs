using UnityEngine;
using UnityEngine.AI;

// -----------------------------------------------
// MOVIMENT SEEK, FLEE I AWAIT DE L'EXPLORADOR
// -----------------------------------------------
public class Explorador : MonoBehaviour
{
    public Animator exploradorAnimator;
    public NavMeshAgent exploradorAgent;
    public GameObject bolaAgent;
    public GameObject fantasmaEnemic;

    enum EstatsAgent
    {
        SEEK,
        FLEE,
        AWAIT
    }

    private const float distanciaDeteccioFantasma = 2f;     // Distancia de deteccio del fantasma
    private const float distanciaSeguretat = 5f;            // Distancia minima despres de fugir
    private const float tempsEspera = 2f;                   // Temps minim que ha de romandre quiet sense el fantasma aprop

    private EstatsAgent estatActual;     
    private float tempsEnSeguretat = 0;                     // Temps que porta en espera despres de fugir
    private float distanciaFantasma;                        // Distancia entre l'explorador i el fantasma

    private void Start()
    {
        estatActual = EstatsAgent.SEEK;
    }

    private void Update()
    {
        distanciaFantasma = Vector3.Distance(transform.position, fantasmaEnemic.transform.position);

        // Passa a flee
        if (distanciaFantasma < distanciaDeteccioFantasma && estatActual == EstatsAgent.SEEK)
        {
            estatActual = EstatsAgent.FLEE;
            exploradorAnimator.SetBool("isFleeing", true);
            exploradorAgent.speed = 2.3f;
        }
        // Passa a await
        else if ((distanciaFantasma > distanciaSeguretat || exploradorAgent.remainingDistance < 0.2f) && estatActual == EstatsAgent.FLEE)
        {
            estatActual = EstatsAgent.AWAIT;
            exploradorAnimator.SetBool("isFleeing", false);
            exploradorAnimator.SetBool("isWaiting", true);
            exploradorAgent.isStopped = true;
        }
        // Torna a seek
        else if (tempsEnSeguretat > tempsEspera && distanciaFantasma > distanciaDeteccioFantasma && estatActual == EstatsAgent.AWAIT)
        {
            estatActual = EstatsAgent.SEEK;
            exploradorAnimator.SetBool("isWaiting", false);
            exploradorAgent.isStopped = false;
            exploradorAgent.speed = 1.9f;
            tempsEnSeguretat = 0;
        }

        switch (estatActual)
        {
            case EstatsAgent.SEEK:
                Seek();
                break;

            case EstatsAgent.FLEE:
                Flee();
                break;

            case EstatsAgent.AWAIT:
                Await();
                break;
        }
    }
    private void Seek()
    {
        exploradorAgent.destination = bolaAgent.transform.position;
    }

    private void Flee()
    {
        Vector3 vectorFlee = transform.position - fantasmaEnemic.transform.position;
        Vector3 posicioFlee = transform.position + vectorFlee;
        exploradorAgent.destination = posicioFlee;
    }

    private void Await()
    {
        tempsEnSeguretat += Time.deltaTime;
    }
}
