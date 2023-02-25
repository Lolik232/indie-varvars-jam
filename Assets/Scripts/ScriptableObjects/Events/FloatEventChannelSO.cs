using BaseClasses.ScriptableObjects.Events;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "Float Event Channel", 
                     menuName = "Events/Float Event Channel",
                     order = 0)]
    public class FloatEventChannelSO : TypedEventChannelSO<float>
    {
        
    }
}