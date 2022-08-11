using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum ImpactType { FLESH, ROBOT, CONCRETE };

    public AudioClip[] impactFlesh;
    public AudioClip[] impactRobot;
    public AudioClip[] impactConcrete;

    public Dictionary<ImpactType, AudioClip[]> ImpactSounds;
}
