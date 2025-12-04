using UnityEngine;

// -----------------------------------------------
// MANAGER DEL FLOCK DE LES VESPES (SPAWN)
// -----------------------------------------------
public class FlockManager : MonoBehaviour
{
    // ============== OBJECTES ==============
    public GameObject vespaReina;                           // Game object de la reina vespa
    public GameObject vespaPrefab;                          // Prefab de la vespa

    // =========== CONSTANTS DEL FLOCK ======
    public const int numVespes = 30;                        // Nombre total de vespes
    public const float velocitat = 0.1f;                    // Rapidesa màxima de vol de la vespa
    public const float velocitatRotacio = 25f;              // Velocitat de rotació de la vespa
    public const float MAX_FORCE = 8f;                      // Força màxima del flock
    public const float RADI_PERCEPCIO = 0.05f;              // Radi de percepció
    public const float DIST_SEPARACIO = 0.02f;              // Distància mínima de separacio
    public const float FORCE_SEPARACIO = 8f;                // Força de separació (weight)
    public const float FORCE_COHESIO = 0.3f;                // Força de cohesió (weight)
    public const float FORCE_ALINEAMENT = 0.25f;            // Força d'alineament (weight)
    public const float FORCE_CONTENCIO = 0.2f;              // Força de contenció

    // ============== ESTATS ================
    private float distanciaVei;                             // Distància inicial mínima entre vespes

    private void Start()
    {
        // La distància inicial de separació depèn del contenidor
        distanciaVei = gameObject.GetComponent<MeshRenderer>().bounds.size.x / 2f;

        // Instancia i mou les vespes a llocs aleatoris dins del contenidor
        for (int i = 0; i < numVespes; i++)
        {
            Vector3 posicioInicial = transform.position +
                new Vector3(
                    Random.Range(-distanciaVei, distanciaVei),
                    Random.Range(-distanciaVei, distanciaVei),
                    Random.Range(-distanciaVei, distanciaVei)
                );

            GameObject go = Instantiate(vespaPrefab, posicioInicial, Quaternion.identity, transform);
            go.GetComponent<VespaFlock>().manager = gameObject;
            go.GetComponent<VespaFlock>().vespaReina = vespaReina;
        }
    }
}
