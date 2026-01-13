using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// -----------------------------------------------
// MOVIMENT APRÈS SENSE WANDER
// -----------------------------------------------
/*
 * Primera versió experimental del moviment desenvolupada sense Wander.
 * El fantasma es mou en una direccio aleatoria cada 5-10 steps.
 * Reduir els steps produeix un moviment menys organic i realista, pero tambe menys erratic (xoca poques vegades).
 * Augmentar els steps produeix un moviment mes fluid i realista, pero mes erratic (xoca sovint).
 * L'agent rep una recompensa negativa gran quan xoca amb un altre objecte (etiqueta "Obstacle").
 * L'agent rep una recompensa positiva molt petita quan es mou. Es premia l'exploracio i el temps de vida.
 * L'agent rep una recompensa negativa molt petita quan es queda quiet (per evitar que aprengui a no moure's).
 * Es castiga, per tant, quan xoca o decideix "fer trampes" quedant-se quiet.
 * S'estableix una quantitat maxima de 1200 steps per episodi.
*/
public class MLFantasmaSenseWander : Agent
{
    // ============== COMPONENTS ============
    private Rigidbody fantasmaRigidBody;

    // ============ CONSTANTS ===============
    private const float midesTerra = 8f;
    private const float velocitatMoviment = 2f;
    private const float velocitatRotacio = 10f;

    private void Start()
    {
        fantasmaRigidBody = GetComponent<Rigidbody>();
    }

    // Atura el fantasma i mou-lo a una ubicacio aleatoria
    public override void OnEpisodeBegin()
    {
        fantasmaRigidBody.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.value * midesTerra - (midesTerra / 2),
                                              0f,
                                              Random.value * midesTerra - (midesTerra / 2));
    }

   public override void CollectObservations(VectorSensor sensor)
    {
        // Velocitat del fantasma
        sensor.AddObservation(fantasmaRigidBody.linearVelocity.x);
        sensor.AddObservation(fantasmaRigidBody.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 2 accions (moviment Z i moviment X)
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Posicio
        Vector3 direction = new Vector3(moveX, 0, moveZ);
        if (direction.magnitude > 1f) direction.Normalize();
        fantasmaRigidBody.linearVelocity = direction * velocitatMoviment;

        // Rotacio
        Quaternion rotacio = Quaternion.LookRotation(transform.forward + fantasmaRigidBody.linearVelocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacio, velocitatRotacio * Time.deltaTime);

        // Recompenses
        if (fantasmaRigidBody.linearVelocity.magnitude > 0.1f) AddReward(0.001f);   // Bonificacio per moviment
        if (fantasmaRigidBody.linearVelocity.magnitude < 0.1f) AddReward(-0.001f);  // Penalitzacio per estar quiet
    }

    private void OnCollisionEnter(Collision collision)
    {
        // El fantasma xoca amb qualsevol obstacle
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-0.2f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        if (Input.GetKey(KeyCode.W)) continuousActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) continuousActionsOut[0] = -1;

        if (Input.GetKey(KeyCode.D)) continuousActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.A)) continuousActionsOut[1] = -1;
    }
}
