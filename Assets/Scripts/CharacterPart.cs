using UnityEngine;
using System.Collections;

public class CharacterPart : MonoBehaviour
{
    // Used by Character script in parent to locate various body parts
    
    public enum BodyPart { head, chest, shoulders, arms, hands, legs, shins, feet, mask, hand_right, hand_left }
    public BodyPart bodyPart;
}
