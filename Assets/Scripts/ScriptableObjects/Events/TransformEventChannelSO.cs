using BaseClasses.ScriptableObjects.Events;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "Transform Event Channel", 
                     menuName = "Events/Transform Event Channel",
                     order = 0)]
    public class TransformEventChannelSO : TypedEventChannelSO<Transform>
    {
    }
}