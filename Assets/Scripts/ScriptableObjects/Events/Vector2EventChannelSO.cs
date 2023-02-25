using BaseClasses.ScriptableObjects.Events;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "Vector2 Event Channel", 
                     menuName = "Events/Vector2 Event Channel", 
                     order = 0)]
    public class Vector2EventChannelSO : TypedEventChannelSO<Vector2>
    {
        
    }
}