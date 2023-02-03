using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class StageModel : MonoBehaviour
{
    public readonly IntReactiveProperty Time = new IntReactiveProperty(0);
    public readonly IntReactiveProperty Score = new IntReactiveProperty(0);
}
