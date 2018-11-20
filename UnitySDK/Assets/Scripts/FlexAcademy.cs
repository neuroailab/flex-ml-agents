using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using NVIDIA.Flex;

public class FlexAcademy : Academy {
	/// <summary>
	/// The number of steps in the current experiment.
	/// </summary>
	public int numSteps = 0;

	/// <summary>
	/// The maximum number of steps per experiment.
	/// </summary>
	public int maxStepsPerExperiment = 100;

	/// <summary>
	/// Maximum number of particles in scene.
	/// </summary>
	public int maxParticles = 16;

	/// <summary>
	/// Radius of flex particles.
	/// </summary>
	public float particleRadius = 0.5f;

	/// <summary>
	/// Dimension of particle state (pos_x, pos_y, pos_z, mass, vel_x, vel_y, vel_z, id).
	/// </summary>
	const int particleDimension = 8;

	/// <summary>
	/// The flex container or solver.
	/// </summary>
	public FlexContainer flexContainer;

	/// <summary>
	/// The flex agent.
	/// </summary>
	public FlexAgent agent;

	/// <summary>
	/// The target the agent has to push.
	/// </summary>
	public GameObject target;

	/// <summary>
	/// Initantiates and initializes a new flex container.
	/// </summary>
	/// <param name="maxParticles">Maximum number of particles of flex container.</param>
	/// <param name="particleRadius">Radius of flex particles.</param>
	void InitializeFlexContainer(int maxParticles, float particleRadius)
	{
		flexContainer = ScriptableObject.CreateInstance<FlexContainer>();
		// number of particles of all FleX objects, in this case 8 agent and 8 target particles
		flexContainer.maxParticles = maxParticles;
		flexContainer.radius = particleRadius;
		flexContainer.solidRest = particleRadius;
		flexContainer.collisionDistance = particleRadius / 2.0f;
	}

	/// <summary>
	/// Adds a flex solid actor to gameObject.
	/// </summary>
	/// <param name="gameObject">Input gameObject with mesh to which flex solid actor should be attached.</param>
	void AddFlexSolidActor(GameObject gameObject)
	{
		// Build the solid flex asset
		FlexSolidAsset flexSolidAsset = ScriptableObject.CreateInstance<FlexSolidAsset> ();
		flexSolidAsset.particleSpacing = particleRadius;
		flexSolidAsset.boundaryMesh = gameObject.GetComponent<MeshFilter> ().mesh;
		flexSolidAsset.Rebuild ();
		// Build the solid flex actor
		FlexSolidActor flexActor = gameObject.AddComponent<FlexSolidActor> ();
		flexActor.asset = flexSolidAsset;
		flexActor.container = this.flexContainer;
		flexActor.drawParticles = true;
		flexActor.enabled = false;
		flexActor.enabled = true;
	}

	/// <summary>
	/// Returns a new random float between -4 to -1, and 1 to 4.
	/// </summary>
	float NewRandomPosition()
	{
		float x = Random.value * 8.0f - 4.0f;
		while (x < 1.0f && x > -1.0f)
			x = Random.value * 8.0f - 4.0f;
		return x;
	}

	/// <summary>
	/// Initializes target cube and adds flex actor.
	/// </summary>
	void InitializeTarget()
	{
		target = GameObject.CreatePrimitive (PrimitiveType.Cube);
		target.name = "Target";
		target.GetComponent<MeshRenderer>().material = (Material) Resources.Load ("Materials/Blue", typeof(Material));
		target.transform.position =  new Vector3(NewRandomPosition(), 0.5f, NewRandomPosition());
		AddFlexSolidActor (target);
	}

	/// <summary>
	/// Initializes agent cube and adds flex actor.
	/// </summary>
	void InitializeAgent()
	{
		agent.GetComponent<MeshRenderer>().material = (Material) Resources.Load ("Materials/Red", typeof(Material));
		AddFlexSolidActor (agent.gameObject);
	}
		

	/// <summary>
	/// Sets the observation size to the maximum number of particles that the flex container can hold.
	/// </summary>
	void SetObservationVectorSize()
	{
		agent.brain.brainParameters.vectorObservationSize = maxParticles * particleDimension;
	}

	/// <summary>
	/// Initializes academy by initializing a new flex container, target and agent.
	/// </summary>
	public override void InitializeAcademy()
	{
		InitializeFlexContainer(maxParticles, particleRadius);
		InitializeTarget();
		InitializeAgent();
		SetObservationVectorSize();
		numSteps = 0;
	}

	/// <summary>
	/// Resets the academy by teleporting the target to a new random location and resetting the number of steps.
	/// </summary>
	public override void AcademyReset()
	{
		target.GetComponent<FlexActor>().Teleport
		(new Vector3(NewRandomPosition(), 0.5f, NewRandomPosition()),
			Quaternion.Euler(Vector3.zero));
		numSteps = 0;
	}
}
