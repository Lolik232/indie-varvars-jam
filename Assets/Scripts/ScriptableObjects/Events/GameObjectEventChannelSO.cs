using BaseClasses.ScriptableObjects.Events;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "Game Object Event Channel",
                     menuName = "Events/Game Object Event Channel",
                     order = 0)]
    public class GameObjectEventChannelSO : TypedEventChannelSO<GameObject>
    {
        
    }
}