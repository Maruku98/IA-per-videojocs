using UnityEngine;
using UnityEngine.AI;

// -----------------------------------------------
// MOVIMENT DEL SEGUIDOR DE LA FORMACIÓ
// -----------------------------------------------
public class SeguidorFormacio : MonoBehaviour
{
    // ================ OBJECTES ==============
    public GameObject liderFormacio;                        // Soldat lider de la formacio
    public NavMeshAgent liderFormacioAgent;                 // Component NavMeshAgent del soldat lider
    public LiderFormacio liderFormacioScript;               // Script del soldat lider (LiderFormacio)

    // ============== COMPONENTS ==============
    private NavMeshAgent seguidorAgent;                     // Component NavMeshAgent del seguidor
    private Animator seguidorAnimator;                      // Component Animator del seguidor

    // =============== CONSTANTS ==============
    private const float velocitatFugida = 1.25f;             // Velocitat del seguidor quan fuig

    // ================ ESTATS ================
    public enum EstatsAgent { SEEK = 0, FLEE, AWAIT };      // Possibles estats del seguidor
    public EstatsAgent estatActual;                         // Estat actual del seguidor

    // ============== VARIABLES ===============
    public Vector3 posicioRelativa;                         // Posicio relativa del seguidor envers el lider
    private float velocitatNominal;                         // Velocitat estandard quan camina
    LiderFormacio.EstatsAgent estatActualLider;             // Estat actual del lider


    // ============== ACTUALITZACIONS =========
    private void Start()
    {
        seguidorAgent = GetComponent<NavMeshAgent>();
        seguidorAnimator = GetComponent<Animator>();
        velocitatNominal = seguidorAgent.speed;

        transform.rotation = liderFormacio.transform.rotation;
        transform.position = liderFormacio.transform.TransformPoint(posicioRelativa);

        IniciarSeek();
    }

    private void Update()
    {
        // El moviment i els estats del seguidor depenen de l'estat del lider
        estatActualLider = liderFormacioScript.estatActual;

        switch (estatActual)
        {
            case EstatsAgent.SEEK:
                Seek();
                break;

            case EstatsAgent.FLEE:
                Flee();
                break;
        }

        // GESTIÓ DE TRANSICIONS
        if (estatActualLider == LiderFormacio.EstatsAgent.FLEE && estatActual == EstatsAgent.SEEK)
        {
            IniciarFlee();
        }
        else if (estatActualLider == LiderFormacio.EstatsAgent.AWAIT && estatActual == EstatsAgent.FLEE && seguidorAgent.remainingDistance <= 0.1f)
        {
            IniciarAwait();
        }
        else if (estatActualLider == LiderFormacio.EstatsAgent.SEEK && estatActual == EstatsAgent.AWAIT)
        {
            IniciarSeek();
        }
    }

    // =============== FUNCIONS ==================
    private void Seek()
    {
        seguidorAgent.destination = liderFormacio.transform.TransformPoint(posicioRelativa);
    }

    private void Flee()
    {
        seguidorAgent.destination = liderFormacio.transform.TransformPoint(posicioRelativa);
    }


    // =============== TRANSICIONS ===============
    // Seek -> flee
    private void IniciarFlee()
    {
        estatActual = EstatsAgent.FLEE;
        seguidorAgent.speed = velocitatFugida;
        seguidorAnimator.SetBool("StartWalking", false);
        seguidorAnimator.SetBool("StartRunning", true);
    }

    // Flee -> await
    private void IniciarAwait()
    {
        estatActual = EstatsAgent.AWAIT;
        seguidorAgent.isStopped = true;
        seguidorAnimator.SetBool("StartWalking", false);
        seguidorAnimator.SetBool("StartRunning", false);
    }

    // Await -> seek
    private void IniciarSeek()
    {
        estatActual = EstatsAgent.SEEK;
        seguidorAgent.isStopped = false;
        seguidorAgent.speed = velocitatNominal;
        seguidorAnimator.SetBool("StartRunning", false);
        seguidorAnimator.SetBool("StartWalking", true);
    }
}
