using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// -----------------------------------------------
// MOVIMENT APRÈS AMB WANDER + OBSTACLE AVOIDANCE
// -----------------------------------------------
/*
 * Segona versió del moviment desenvolupada amb Wander.
 * El fantasma realitza un Wander amb obstacle avoidance (obstacles petits).
 * El Wander només s'encarrega de generar punts aleatoris i garantir-ne la validesa dins d'unes limitacions.
 * Augmentar els steps d'1 a 10 produeix un moviment mes fluid i realista. L'entrenament es va fer amb 1.
 * L'agent rep una recompensa positiva gran quan s'acosta prou al punt de Wander.
 * L'agent rep una recompensa relativa a quant s'ha apropat al punt de Wander en cada repetició (pot ser positiva, negativa o neutra).
 * L'agent rep una recompensa relativa a com mira cap al punt de Wander en cada repetició (pot ser positiva, negativa o neutra).
 * L'agent rep una recompensa negativa petita quan es queda quiet (per evitar que faci "trampes").
 * Per defecte, l'agent rep una recompensa negativa petita a cada step (perque prioritzi buscar el cami mes rapid).
 * L'agent rep una recompensa negativa gran quan està xocant amb un obstacle (etiqueta "Obstacle").
 * S'estableix una quantitat maxima de 1500 steps per episodi.
 * Algunes variables canvien de valor en funcio de l'escena per adaptar-se millor.
*/
public class MLFantasmaAmbWander : Agent
{
    // ============== VARIABLES =============
    public float posicioVertical;
    public float velocitatMoviment;
    public float proximitatWander;
    public float wanderRadi;
    public float wanderDistanciaCercle;

    // =============== OBJECTES =============
    public Transform pilota;                            // GameObject que guia el Wander
    public GameObject terra;                            // GameObject del terra de l'escenari
    private Collider colliderTerra;                     // Component Collider del terra

    // ============== COMPONENTS ============
    private Rigidbody fantasmaRigidBody;

    // ============ CONSTANTS ===============
    private const float velocitatRotacio = 10.0f;

    // ============== ESTATS ================
    private Vector3 midesTerra;                         // Mides del terra en funció de l'escenari
    private Vector3 targetWander;                       // Punt del Wander cap on anar
    private float distanciaAnterior;                    // Ultima distancia entre l'agent i el punt de Wander

    private void Start()
    {
        colliderTerra = terra.GetComponent<Collider>();
        fantasmaRigidBody = GetComponent<Rigidbody>();

        midesTerra = colliderTerra.bounds.size;
    }

    // Mou el fantasma a una ubicacio aleatoria i reinicia el Wander
    public override void OnEpisodeBegin()
    {
        fantasmaRigidBody.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.value * midesTerra.x - (midesTerra.x / 2),
                                              posicioVertical,
                                              Random.value * midesTerra.z - (midesTerra.z / 2));
        Wander();
    }

   public override void CollectObservations(VectorSensor sensor)
    {
        // Posicio del fantasma
        sensor.AddObservation(transform.localPosition);

        // Velocitat del fantasma
        sensor.AddObservation(fantasmaRigidBody.linearVelocity);

        // Posicio del target
        sensor.AddObservation(pilota.localPosition);

        // Distancia actual entre el fantasma i el target
        sensor.AddObservation(Vector3.Distance(transform.position, targetWander));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log(targetWander);
        // 2 accions (moviment Z i moviment X)
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Posicio
        Vector3 direction = new Vector3(moveX, 0, moveZ).normalized * velocitatMoviment;
        fantasmaRigidBody.linearVelocity = direction * velocitatMoviment;

        // Rotacio
        Quaternion rotacio = Quaternion.LookRotation(transform.forward + fantasmaRigidBody.linearVelocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacio, velocitatRotacio * Time.deltaTime);

        // RECOMPENSES
        // Recompensa final per arribar a l'objectiu
        float distanciaActual = Vector3.Distance(transform.position, targetWander);
        
        if (distanciaActual < proximitatWander)
        {
            AddReward(1.5f);
            Wander();
            return;
        }

        // Recompensa principal per apropar-se/allunyar-se (-1f a 1f)
        float maxDistanciaRecorreguda = velocitatMoviment * Time.fixedDeltaTime;
        float recompensa = (distanciaAnterior - distanciaActual) / maxDistanciaRecorreguda;
        AddReward(recompensa);

        // Recompensa per mirar cap al punt (-1f a 1f)
        Vector3 dirCapTarget = (targetWander - transform.position).normalized;
        Vector3 dirFantasma = fantasmaRigidBody.linearVelocity.normalized;
        float recompensaOrientacio = Vector3.Dot(dirCapTarget, dirFantasma);
        AddReward(recompensaOrientacio);

        // Penalització per estar quiet
        if (fantasmaRigidBody.linearVelocity.sqrMagnitude < 0.01f)
        {
            AddReward(-0.002f);
        }

        // Petita penalització per temps
        AddReward(-0.00085f);

        distanciaAnterior = distanciaActual;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Penalització gran per xocar amb un obstacle
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-2f);
        }
    }

    // Genera un nou punt de Wander i en garanteix la validesa
    private void Wander()
    {
        int repeticions = 0;
        Vector3 posicioAleatoria;
        Vector3 worldTarget;
        bool esPuntValid = false;

        do
        {
            // Mesura de contingencia per si el loop es queda pillat
            if (repeticions == 10)
            {
                EndEpisode();
                return;
            }
            repeticions++;

            posicioAleatoria = puntAleatori();
            worldTarget = transform.TransformPoint(posicioAleatoria);
            worldTarget.y = posicioVertical;

            esPuntValid = validarPuntAleatori(worldTarget);

            if (!esPuntValid)
            {
                worldTarget = transform.TransformPoint(-posicioAleatoria / 2.0f);   // Al darrere pero bastant a prop
                esPuntValid = validarPuntAleatori(worldTarget);                     // Segona validacio
            }
        }
        while (!esPuntValid);

        targetWander = worldTarget;
        pilota.transform.position = targetWander;

        distanciaAnterior = Vector3.Distance(transform.position, targetWander);
    }

    // Retorna un punt de Wander aleatori
    private Vector3 puntAleatori()
    {
        Vector3 posicioAleatoria = Random.insideUnitSphere;
        posicioAleatoria.y = posicioVertical;
        posicioAleatoria.Normalize();
        posicioAleatoria *= wanderRadi;
        posicioAleatoria += new Vector3(0, 0, wanderDistanciaCercle);

        return posicioAleatoria;
    }

    // Comprova la validesa d'un punt aleatori
    private bool validarPuntAleatori(Vector3 punt)
    {
        bool esDinsEscenari = colliderTerra.bounds.Contains(punt);
        return esDinsEscenari;
    }

    // Moviment manual del fantasma amb WASD
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        if (Input.GetKey(KeyCode.W)) continuousActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) continuousActionsOut[0] = -1;

        if (Input.GetKey(KeyCode.D)) continuousActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.A)) continuousActionsOut[1] = -1;
    }
}
