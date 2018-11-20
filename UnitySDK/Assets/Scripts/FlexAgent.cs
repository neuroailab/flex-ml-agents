using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using NVIDIA.Flex;

public class FlexAgent : Agent
{	
	/// <summary>
	/// The flex academy. Contains the flex container and controls the environment.
	/// </summary>
	public FlexAcademy academy;

	/// <summary>
	/// The speed. Proportional to how fast the agent can move.
	/// </summary>
	public float speed = 10;
		
	/// <summary>
	/// Agent reset. Teleports the agent back to the center and the target to a new random position.
	/// </summary>
	public override void AgentReset()
	{
		academy.AcademyReset ();

		if (this.transform.position.y < -1.0)
		{
			// The agent fell
			GetComponent<FlexActor>().Teleport(new Vector3(0f, 0.5f, 0), Quaternion.Euler(Vector3.zero));
		}
	}

	/// <summary>
	/// Collects observations which correspond to all the particles in the scene (positions, masses, velocities)
	/// </summary>
	public override void CollectObservations()
	{
		foreach (FlexActor actor in academy.flexContainer.actors)
		{
			for (int i = 0; i < actor.particles.Length; i++) 
			{
				AddVectorObs(actor.particles [i]);
				AddVectorObs(actor.velocities [i]);
				AddVectorObs(actor.id);
			}
		}

		FillUpOberservationVectorWithDummyValue(-1.0f);
	}

	/// <summary>
	/// Computes average velocity magnitude.
	/// </summary>
	/// <param name="velocities">List of 3D velocites.</param>
	float ComputeAverageVelocityMagnitude(Vector3[] velocities)
	{
		Vector3 averageVelocity = Vector3.zero;

		foreach(Vector3 velocity in velocities)
		{
			averageVelocity.x += velocity.x;
			averageVelocity.y += velocity.y;
			averageVelocity.z += velocity.z;
		}	
		averageVelocity.x /= velocities.Length;
		averageVelocity.y /= velocities.Length;
		averageVelocity.z /= velocities.Length;

		return averageVelocity.magnitude;
	}

	/// <summary>
	/// Specifies the reward setup of the push the target task.
	/// </summary>
	void AddAgentRewards()
	{
		// Target was pushed.
		if (ComputeAverageVelocityMagnitude(academy.target.GetComponent<FlexActor>().velocities) > 0.1f)
		{
			AddReward(1.0f);
		}

		// Time penalty
		AddReward(-0.05f);

		// Agent or target fell off platform
		if (this.transform.position.y < -1.0 || 
			academy.target.transform.position.y < -1.0)
		{
			AddReward(-1.0f);
			Done();
		}
	}

	/// <summary>
	/// Executes the action specified by the brain.
	/// </summary>
	/// <param name="vectorAction">The float action vector.</param>
	/// <param name="textAction">The string text action.</param>
	void ExecuteAction(float[] vectorAction, string textAction)
	{
		// Actions, size = 2
		Vector3 controlSignal = Vector3.zero;
		controlSignal.x = vectorAction[0];
		controlSignal.z = vectorAction[1];
		GetComponent<FlexActor>().ApplyImpulse (controlSignal * speed);
	}

	/// <summary>
	/// Agent action. Specifies rewards and executes the action specified by the brain.
	/// </summary>
	/// <param name="vectorAction">The float action vector.</param>
	/// <param name="textAction">The string text action.</param>
	public override void AgentAction(float[] vectorAction, string textAction)
	{
		AddAgentRewards();
		ExecuteAction(vectorAction, textAction);
	}
}
