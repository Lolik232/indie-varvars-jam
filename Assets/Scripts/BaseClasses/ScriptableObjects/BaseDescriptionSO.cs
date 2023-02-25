using UnityEngine;

namespace BaseClasses.ScriptableObjects
{
    [CreateAssetMenu(fileName = "description",
                     menuName = "Description/Base description",
                     order = 0)]
    public class BaseDescriptionSO : ScriptableObject
    {
        [SerializeField, TextArea]
        private string _description = default;

        public string Description => _description;
    }
}