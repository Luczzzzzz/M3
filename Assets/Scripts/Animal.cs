using UnityEngine;

namespace Match3
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Animal : MonoBehaviour
    {
        public AnimalType type;

        public void SetType(AnimalType type)
        {
            this.type = type;
            GetComponent<SpriteRenderer>().sprite = type.sprite;
        }
        public AnimalType GetType() => type;
    }

}