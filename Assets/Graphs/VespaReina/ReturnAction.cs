using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// -----------------------------------------------
// LA REINA VESPA TORNA AL RUSC
// -----------------------------------------------
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Return", story: "Torna al rusc", category: "Action", id: "845fc33684c50034a70e2f233ea02472")]
public partial class ReturnAction : Action
{
    // =========== OBJECTES ==============
    private GameObject vespaReinaGO;                        // Game object en escena de la reina vespa
    private GameObject spawnGO;                             // Game object en escena del spawn

    // =========== COMPONENTS ============
    private BlackboardReference blackboard;                 // Referencia al Blackboard de la reina vespa

    // =========== CONSTANTS =============
    private const float velocitatRetorn = 0.15f;            // Velocitat de retorn de la reina vespa
    private const float velocitatRotacio = 1.5f;            // Velocitat de rotacio de la reina vespa
    private const float distanciaArribada = 0.15f;          // Distancia a la qual la reina vespa torna al wander

    // ============= ESTATS ==============
    private Vector3 direccio;                               // Direccio cap on va la vespa

    protected override Status OnStart()
    {
        vespaReinaGO = GameObject;

        blackboard = vespaReinaGO.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        blackboard.GetVariable<GameObject>("vespaReina", out var vespaReina);
        blackboard.GetVariable<GameObject>("soldatLider", out var soldatLider);
        blackboard.GetVariable<GameObject>("Spawn", out var spawn);
        blackboard.SetVariableValue<EstatVespa>("EstatVespa", EstatVespa.Return);

        vespaReinaGO = vespaReina.Value;
        spawnGO = spawn.Value;

        direccio = vespaReinaGO.transform.forward * velocitatRetorn;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Seek(spawnGO.transform.position);

        float distancia = Vector3.Distance(vespaReinaGO.transform.position, spawnGO.transform.position);
        if (distancia < distanciaArribada)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    private void Seek(Vector3 target)
    {
        Vector3 vecCapAlSpawn = (target - vespaReinaGO.transform.position).normalized * velocitatRetorn;

        Vector3 vectorGir = vecCapAlSpawn - direccio;
        vectorGir = Vector3.ClampMagnitude(vectorGir, 0.1f);

        direccio += vectorGir;

        vespaReinaGO.transform.position += direccio * Time.deltaTime;

        Quaternion rotacio = Quaternion.LookRotation(vecCapAlSpawn);
        vespaReinaGO.transform.rotation = Quaternion.Slerp(vespaReinaGO.transform.rotation, rotacio, velocitatRotacio * Time.deltaTime);
    }
}

