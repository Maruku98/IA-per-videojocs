using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// -----------------------------------------------
// LA REINA VESPA PERSEGUEIX ELS SOLDATS
// -----------------------------------------------
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "Persegueix els soldats", category: "Action", id: "5e9b0e6412e0d662615d74e4ce9b9f1a")]
public partial class ChaseAction : Action
{
    // =========== OBJECTES ==============
    private GameObject vespaReinaGO;                        // Game object en escena de la reina vespa
    private GameObject soldatLiderGO;                       // Game object en escena del soldat lider
    private GameObject spawnGO;                             // Game object en escena del spawn

    // =========== COMPONENTS ============
    private BlackboardReference blackboard;                 // Referencia al Blackboard de la reina vespa
    private SkinnedMeshRenderer soldatLiderRenderer;        // Component SkinnedMeshRenderer del soldat lider

    // =========== CONSTANTS =============
    private const float velocitatPersecucio = 0.15f;        // Velocitat de persecucio de la reina vespa
    private const float velocitatRotacio = 1.5f;            // Velocitat de rotacio de la reina vespa
    private const float distanciaSeguretat = 4f;            // Distancia necessaria entre el soldat lider i el rusc

    // ============= ESTATS ==============
    private Vector3 direccio;                               // Direccio cap on va la vespa

    protected override Status OnStart()
    {
        vespaReinaGO = GameObject;

        blackboard = vespaReinaGO.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        blackboard.GetVariable<GameObject>("vespaReina", out var vespaReina);
        blackboard.GetVariable<GameObject>("soldatLider", out var soldatLider);
        blackboard.GetVariable<GameObject>("Spawn", out var spawn);
        blackboard.SetVariableValue<EstatVespa>("EstatVespa", EstatVespa.Chase);

        vespaReinaGO = vespaReina.Value;
        soldatLiderGO = soldatLider.Value;
        spawnGO = spawn.Value;
        soldatLiderRenderer = soldatLiderGO.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        
        return Status.Running;
    }

    // La reina vespa persegueix el soldat lider
    protected override Status OnUpdate()
    {
        float distancia = Vector3.Distance(soldatLiderGO.transform.position, spawnGO.transform.position);
        if (distancia > distanciaSeguretat)
        {
            return Status.Success;
        }

        Vector3 posSoldat = soldatLiderGO.transform.position;
        posSoldat.y += soldatLiderRenderer.bounds.max.y;

        Seek(posSoldat);

        return Status.Running;
    }

    private void Seek(Vector3 target)
    {
        Vector3 vecCapAlSoldat = (target - vespaReinaGO.transform.position).normalized * velocitatPersecucio;

        Vector3 vectorGir = vecCapAlSoldat - direccio;
        vectorGir = Vector3.ClampMagnitude(vectorGir, 0.1f);

        direccio += vectorGir;

        vespaReinaGO.transform.position += direccio * Time.deltaTime;

        Quaternion rotacio = Quaternion.LookRotation(vecCapAlSoldat);
        vespaReinaGO.transform.rotation = Quaternion.Slerp(vespaReinaGO.transform.rotation, rotacio, velocitatRotacio * Time.deltaTime);
    }
}

