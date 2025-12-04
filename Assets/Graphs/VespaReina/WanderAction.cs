using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

// -----------------------------------------------
// LA REINA VESPA DEAMBULA A PROP DEL RUSC (WANDER)
// -----------------------------------------------
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wander", story: "Wander a prop del rusc", category: "Action", id: "2ff7475093c7a78e97e4f26e72acd260")]
public partial class WanderAction : Action
{
    // =========== OBJECTES ==============
    private GameObject vespaReinaGO;                        // Game object en escena de la reina vespa
    private GameObject spawnGO;                             // Game object en escena del spawn

    // =========== COMPONENTS ============
    private BlackboardReference blackboard;                 // Referencia al Blackboard de la reina vespa
    private MeshRenderer spawnRenderer;                     // Component MeshRenderer del spawn

    // =========== CONSTANTS =============
    private const float velocitatWander = 0.03f;            // Velocitat del wander de la reina vespa
    private const float velocitatRotacio = 1.5f;            // Velocitat de rotacio de la reina vespa
    private const float proximitatWander = 0.01f;           // Distancia minima de canvi de punt de wander
    private const float wanderRadi = 0.2f;                  // Radi del cercle
    private const float wanderDistanciaCercle = 1f;         // Distancia entre el centre del cercle i la reina vespa

    // ============= ESTATS ==============
    private Vector3 puntWanderActual;
    private Vector3 direccio;                               // Direccio cap on va la vespa

    protected override Status OnStart()
    {
        vespaReinaGO = GameObject;

        blackboard = vespaReinaGO.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        blackboard.GetVariable<GameObject>("vespaReina", out var vespaReina);
        blackboard.GetVariable<GameObject>("Spawn", out var spawn);
        blackboard.SetVariableValue<EstatVespa>("EstatVespa", EstatVespa.Wander);

        vespaReinaGO = vespaReina.Value;
        spawnGO = spawn.Value;
        spawnRenderer = spawnGO.GetComponent<MeshRenderer>();

        puntWanderActual = Wander();
        direccio = vespaReinaGO.transform.forward * velocitatWander;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float distancia = Vector3.Distance(vespaReinaGO.transform.position, puntWanderActual);

        // Wander
        if (distancia > proximitatWander)
        {
            Seek(puntWanderActual);
            return Status.Running;
        }

        // Canvi de wander
        return Status.Success;
    }

    // Retorna el proper punt del wander
    private Vector3 Wander()
    {
        Vector3 posicioAleatoria = UnityEngine.Random.insideUnitSphere;
        posicioAleatoria.Normalize();
        posicioAleatoria *= wanderRadi;
        posicioAleatoria += Vector3.forward * wanderDistanciaCercle;

        Vector3 worldTarget = vespaReinaGO.transform.TransformPoint(posicioAleatoria);

        if (!spawnRenderer.bounds.Contains(worldTarget))
        {
            worldTarget = vespaReinaGO.transform.TransformPoint(-posicioAleatoria);
        }

        return worldTarget;
    }

    private void Seek(Vector3 target)
    {
        Vector3 vecCapAlWander = (target - vespaReinaGO.transform.position).normalized * velocitatWander;

        Vector3 vectorGir = vecCapAlWander - direccio;
        vectorGir = Vector3.ClampMagnitude(vectorGir, 0.05f);

        direccio += vectorGir;

        vespaReinaGO.transform.position += direccio * Time.deltaTime;

        Quaternion rotacio = Quaternion.LookRotation(vecCapAlWander);
        vespaReinaGO.transform.rotation = Quaternion.Slerp(vespaReinaGO.transform.rotation, rotacio, velocitatRotacio * Time.deltaTime);
    }
}

