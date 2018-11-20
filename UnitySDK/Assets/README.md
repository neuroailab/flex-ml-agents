# FleX ML Agents Simulation Environment

You will need a Windows machine to follow this tutorial. FleX only works in the Unity Editor on Windows. You can however build a Linux binary on Windows which you can then run headless on a cluster of your choice. Please follow the [Building a FleX Linux binary tutorial](https://github.com/neuroailab/flex-ml-agents/tree/master/linux-flexlib "FleX Linux binary tutorial") if you wish to do so.

The general goal of flex-ml-agents is to use the FleX particle representation as input for training machine learning models. In the toy example we have implemented here, an agent controls a red cube, consisting of 8 FleX particles, and the task is to move the blue cube, also consisting of 8 FleX particles, without knocking it off the plattform. The agent has a maximum number of 100 steps to achieve the highest reward possible. It gets penalized with every time step and also if it knocks either the red or blue cube off the plane in which case the experiment is reset and the blue cube teleported to a new random position on the plane. This setup is implemented in the [FlexAgent.cs](https://github.com/neuroailab/flex-ml-agents/blob/master/UnitySDK/Assets/Scripts/FlexAgent.cs "FlexAgent.cs") and [FlexAcadamy.cs](https://github.com/neuroailab/flex-ml-agents/blob/master/UnitySDK/Assets/Scripts/FlexAcademy.cs "FlexAcademy.cs") scripts.

We will use the standard PPO setup in ML agents to train such an agent with the only model input being the 8 particle states of the red cube and the 8 particle states of the blue cube. Therefore, we need to extend the standard ML Agents setup to first construct a FleX particle scene and then to send those particles to the external brain trained in Tensorflow.

To construct a FleX particle scene, we first need to instantiate a `Flex container` with
```cs
FlexContainer flexContainer = ScriptableObject.CreateInstance<FlexContainer>();
```
Then we can fill this container with FleX assets.
To build a rigid FleX object from a gameobject with mesh and add it to a given `Flex container` programmatically in a script, the following code is used:

```cs
void AddFlexSolidActor(GameObject gameObject, FlexContainer flexContainer)
{
    // Build the solid flex asset
    FlexSolidAsset flexSolidAsset = ScriptableObject.CreateInstance<FlexSolidAsset>();
    flexSolidAsset.boundaryMesh = gameObject.GetComponent<MeshFilter>().mesh;
    flexSolidAsset.Rebuild();
    // Build the solid flex actor
    FlexSolidActor flexActor = gameObject.AddComponent<FlexSolidActor>();
    flexActor.asset = flexSolidAsset;
    flexActor.container = flexContainer;
    flexActor.drawParticles = true;
    flexActor.enabled = false;
    flexActor.enabled = true;
}
```

Calling `Rebuild()` as well as toggling the `enabled` flag are necessary to actually instantiate the `Flex asset` and `Flex actor` and load them into the scene. Also, this will only work if the boundary mesh is closed and scaled proportionally to the particle spacing of the asset. For instance, if the mesh is small compared to the particle spacing, it will be represented by only one particle, not allowing to distinguish the original mesh geometry from any other geometry.
As you can see in the [FlexAcadamy.cs](https://github.com/neuroailab/flex-ml-agents/blob/master/UnitySDK/Assets/Scripts/FlexAcademy.cs "FlexAcademy.cs") script, the `AddFlexSolidActor()` function is called to construct the red agent cube and the blue target cube.

Once the FleX particle scene is set up, we need to extract the particles from the `Flex container` and message them to the external Tensorflow machine learning model. In order to do that, we modified the [original NVidia FleX Unity plugin](https://assetstore.unity.com/packages/tools/physics/nvidia-flex-for-unity-1-0-beta-120425 "Unity FleX plugin") to expose the particle state of all particles in the scene, namely their position, mass and velocity. We also made sure to assign an id to every particle identifying the object it represents. With these modifications sending the particle information to the Tensorflow model via the `Vector Observation` is trivial:

```cs
public override void CollectObservations()
{
    foreach (FlexActor actor in academy.flexContainer.actors)
    {
        for (int i = 0; i < actor.particles.Length; i++)
        {
            AddVectorObs(actor.particles[i]);
            AddVectorObs(actor.velocities[i]);
            AddVectorObs(actor.id);
        }
    }
    FillUpOberservationVectorWithDummyValue(-1.0f);
}
```

Please note that this code will not work with the [original NVidia FleX Unity plugin](https://assetstore.unity.com/packages/tools/physics/nvidia-flex-for-unity-1-0-beta-120425 "Unity FleX plugin") as we needed to make modifications to expose the particle states that we are interested in transferring to our machine learning model. `FillUpOberservationVectorWithDummyValue(-1.0f)` in the last line is used to fill up the observation vector to a constant size, in case of a varying number of particles in the scene.

Lastly, the agent needs to be able to act on the particles of the red cube in order to move it around. Therefore, the action returned by the agent is transformed into an impulse on all particles of the red cube:

```cs
void ExecuteAction(float[] vectorAction, string textAction)
{
    Vector3 controlSignal = Vector3.zero;
    controlSignal.x = vectorAction[0];
    controlSignal.z = vectorAction[1];
    GetComponent<FlexActor>().ApplyImpulse(controlSignal * speed);
}
```
Note that the `ApplyImpulse(Vector3 impulse, int particleId = -1)` takes an optional `particleId` argument which allows to act on a single particle instead of all particles in an object.

To reset the scene when an episode ends, the red agent cube particles are teleported back to the center of the plane and the blue target cube particles are teleported to a new random location on the plane using the `Teleport(Vector3 position, Quaternion orientation)` method of the `FlexActor`.

This is all we need to set up our “Use red cube to push blue cube” experiment, using FleX to represent the scene with particles and Unity ML Agents to perform the coordination and training. Checkout the [FlexAgent.cs](https://github.com/neuroailab/flex-ml-agents/blob/master/UnitySDK/Assets/Scripts/FlexAgent.cs "FlexAgent.cs") and [FlexAcademy.cs](https://github.com/neuroailab/flex-ml-agents/blob/master/UnitySDK/Assets/Scripts/FlexAcademy.cs "FlexAcademy.cs") scripts for the implementation of the full experiment. If you want to run the training yourself, you can open the **UnitySDK** Unity project in the [flex-ml-agents repository](https://github.com/neuroailab/flex-ml-agents "flex-ml-agents repository") in the Unity Editor, load the `Flex` scene from the **Scenes** folder and hit play after starting up the `mlagents-learn` command, just as you would do it normally in ML Agents. You should see the agent train and the cumulative reward increase.

This concludes this tutorial. Please cite our paper if you found it helpful or if you end up using the [flex-ml-agents repository](https://github.com/neuroailab/flex-ml-agents "flex-ml-agents repository").

```
@inproceedings{mrowca2018flexible,
  title={Flexible Neural Representation for Physics Prediction},
  author={Mrowca, Damian and Zhuang, Chengxu and Wang, Elias and Haber, Nick and Fei-Fei, Li and Tenenbaum, Joshua B and Yamins, Daniel LK},
  booktitle={Advances in Neural Information Processing Systems},
  year={2018}
}
```

Please visit the [official website](https://neuroailab.github.io/flex-ml-agents/ "FleX ML Agents Website") for more information.

This code, is part of the code release of [Flexible Neural Representation for Physics Prediction](https://neuroailab.github.io/physics/ "Flexible Neural Representation for Physics Prediction").

