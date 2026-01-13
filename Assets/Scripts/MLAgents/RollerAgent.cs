using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RollerAgent : Agent
{
    public Transform targetTransform;

    private Rigidbody sphereRigidBody;
    private const float forceMultiplier = 10.0f;

    private void Start()
    {
        sphereRigidBody = GetComponent<Rigidbody>();
    }

    // Resets on episode begin
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (transform.localPosition.y < 0)
        {
            sphereRigidBody.angularVelocity = Vector3.zero;
            sphereRigidBody.linearVelocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target (cube) to a new spot
        targetTransform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    // Collects environment data
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target position
        sensor.AddObservation(targetTransform.localPosition);

        // Agent position
        sensor.AddObservation(transform.localPosition);

        // Agent velocity (2D, ignore vertical)
        sensor.AddObservation(sphereRigidBody.linearVelocity.x);
        sensor.AddObservation(sphereRigidBody.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        sphereRigidBody.AddForce(controlSignal * forceMultiplier);

        // Fell off platform
        if (transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Reached target
        if (collision.gameObject.CompareTag("Target"))
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
