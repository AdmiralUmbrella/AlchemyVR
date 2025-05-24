using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TutorialSequence",
    menuName = "Game/Tutorial/Sequence",
    order = 1)]
public class TutorialSequenceSO : ScriptableObject
{
    public List<TutorialStepSO> steps = new();
}