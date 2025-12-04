using System.Collections;
using Unity.Behavior;
using UnityEngine;

// -----------------------------------------------
// VESPA DEL FLOCK
// -----------------------------------------------
public class VespaFlock : MonoBehaviour
{
    // ============== OBJECTES ==============
    public GameObject manager;                              // Game object del manager (spawn)
    public GameObject vespaReina;                           // Game object de la vespa reina

    // ============== COMPONENTS ============
    private MeshRenderer managerBounds;                     // Component MeshRenderer del manager
    private BlackboardReference reinaVespaBlackboard;       // Component Blackboard de la vespa reina

    // ============== ESTATS ================
    private EstatVespa estatVespaReina;                     // Estat actual de la vespa reina
    public Vector3 direccioVespa;                           // Direccio cap on va la vespa
    private Vector3 vectorFlocking;                         // Vector resultant del flocking
    private Vector3 vectorContencio;                        // Vector resultant de la contencio
    private float velocitatWander;                          // Velocitat de la vespa quan fa wander
    private float velocitatPersecucio;                      // Velocitat de la vespa quan persegueix amb la reina
    private float velocitatVespa;                           // Velocitat actual que porta de la vespa

    private void Start()
    {
        managerBounds = manager.GetComponent<MeshRenderer>();
        reinaVespaBlackboard = vespaReina.GetComponent<BehaviorGraphAgent>().BlackboardReference;

        direccioVespa = new Vector3(
            Random.Range(-FlockManager.velocitat, FlockManager.velocitat),
            Random.Range(-FlockManager.velocitat, FlockManager.velocitat),
            Random.Range(-FlockManager.velocitat, FlockManager.velocitat)
        );
        velocitatWander = direccioVespa.magnitude;
        velocitatPersecucio = 0.5f;
        velocitatVespa = velocitatWander;
        vectorFlocking = Vector3.zero;
        vectorContencio = Vector3.zero;

        StartCoroutine(nameof(FlockingContingut));
    }

    private IEnumerator FlockingContingut()
    {
        while (true)
        {
            // Mirem l'estat de la vespa reina
            reinaVespaBlackboard.GetVariable<EstatVespa>("EstatVespa", out var estat);
            estatVespaReina = estat;

            if (estatVespaReina != EstatVespa.Wander)
            {
                velocitatVespa = velocitatPersecucio;
                direccioVespa = (vespaReina.transform.position - transform.position);
            }
            else
            {
                velocitatVespa = velocitatWander;
            }

            // Moviment estandard
            Flocking();
            ContenirVespa();
            MoureVespa();

            yield return null;
        }
    }

    // Aplica el moviment de flocking grupal: separacio, cohesio i alineament
    private void Flocking()
    {
        Vector3 vecSeparacio = Vector3.zero;
        Vector3 vecCohesio = Vector3.zero;
        Vector3 vecAlineament = Vector3.zero;
        int numSeparacio = 0;
        int numFlocking = 0;

        for (int i = 0; i < FlockManager.numVespes; i++)
        {
            GameObject child = manager.transform.GetChild(i).gameObject;
            float distance = Vector3.Distance(transform.position, child.transform.position);
            if (gameObject != child && distance < FlockManager.RADI_PERCEPCIO && distance != 0)
            {
                // SEPARACIÓ
                if (distance < FlockManager.DIST_SEPARACIO)
                {
                    Vector3 vectToMe = transform.position - child.transform.position;
                    vectToMe /= (distance * distance);
                    vecSeparacio += vectToMe;
                    numSeparacio++;
                }

                if (i == 0) { continue; }

                // COHESIÓ
                vecCohesio += child.transform.position;

                // ALINEAMENT
                vecAlineament += child.GetComponent<VespaFlock>().direccioVespa;

                numFlocking++;
            }
        }

        // RESOLUCIÓ DE LA SEPARACIÓ
        if (numSeparacio > 0)
        {
            vecSeparacio /= numSeparacio;
            vecSeparacio = vecSeparacio.normalized * velocitatVespa;
            vecSeparacio = Vector3.ClampMagnitude(vecSeparacio, FlockManager.MAX_FORCE);
        }

        // RESOLUCIÓ DE LA COHESIÓ I L'ALINEAMENT
        if (numFlocking > 0)
        {
            vecAlineament /= numFlocking;
            vecAlineament = vecAlineament.normalized * velocitatVespa;
            vecAlineament -= direccioVespa;
            vecAlineament = Vector3.ClampMagnitude(vecAlineament, FlockManager.MAX_FORCE);

            vecCohesio /= numFlocking;
            vecCohesio -= transform.position;
            vecCohesio = vecCohesio.normalized * velocitatVespa;
            vecCohesio = Vector3.ClampMagnitude(vecCohesio, FlockManager.MAX_FORCE);
        }

        // APLICACIÓ DE FORCES (WEIGHTS)
        vecSeparacio *= FlockManager.FORCE_SEPARACIO;
        vecAlineament *= FlockManager.FORCE_ALINEAMENT;
        vecCohesio *= FlockManager.FORCE_COHESIO;

        vectorFlocking =  vecSeparacio + vecAlineament + vecCohesio;
    }

    // Aplica un vector corrector de contencio
    private void ContenirVespa()
    {
        Bounds areaContencio = managerBounds.bounds;
        Vector3 vecCorreccio = Vector3.zero;

        Vector3 pos = transform.position;

        // Eix X
        if (pos.x < areaContencio.min.x + FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.x = 1f;
        }
        else if (pos.x > areaContencio.max.x - FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.x = -1f;
        }

        // Eix Y
        if (pos.y < areaContencio.min.y + FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.y = 1f;
        }
        else if (pos.y > areaContencio.max.y - FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.y = -1f;
        }

        // Eix Z
        if (pos.z < areaContencio.min.z + FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.z = 1f;
        }
        else if (pos.z > areaContencio.max.z - FlockManager.FORCE_CONTENCIO)
        {
            vecCorreccio.z = -1f;
        }

        // Normalització i limitació de força
        if (vecCorreccio != Vector3.zero)
        {
            vecCorreccio = vecCorreccio.normalized * FlockManager.velocitat;
            vecCorreccio -= direccioVespa;

            if (vecCorreccio.magnitude > FlockManager.MAX_FORCE)
            {
                vecCorreccio = vecCorreccio.normalized * FlockManager.MAX_FORCE;
            }
        }

        vectorContencio = vecCorreccio;
    }

    // Aplica el moviment resultant a la vespa
    private void MoureVespa()
    {
        direccioVespa += vectorFlocking * Time.deltaTime;
        direccioVespa += vectorContencio * Time.deltaTime;
        direccioVespa = Vector3.ClampMagnitude(direccioVespa, velocitatVespa);

        Vector3 posicioSeguent = transform.position + direccioVespa * Time.deltaTime;
        Vector3 vecDireccio = (posicioSeguent - transform.position).normalized;
        Quaternion rotacio = Quaternion.LookRotation(vecDireccio);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotacio, FlockManager.velocitatRotacio * Time.deltaTime);
        transform.position = posicioSeguent;
    }
}
