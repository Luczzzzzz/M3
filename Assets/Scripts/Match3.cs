
using UnityEngine;

namespace Match3
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPostion = Vector3.zero;
        [SerializeField] bool debug = true;

        [SerializeField] Animal animalPrefab;
        [SerializeField] AnimalType[] animalTypes;

        GridSystem2D<GridObject<Animal>> grid;
        private void Start()
        {
            InitializedGrid();
        }

        void InitializedGrid()
        {
            grid = GridSystem2D<GridObject<Animal>>.VerticalGrid(width, height, cellSize, originPostion, debug);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    CreateAnimal(x, y);
                }
        }

        private void CreateAnimal(int x, int y)
        {
            Animal animal = Instantiate(animalPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            animal.SetType(animalTypes[Random.Range(0, animalTypes.Length)]);
            var gridObject = new GridObject<Animal>(grid, x, y);
            gridObject.SetValue(animal);
            grid.SetValue(x, y, gridObject);
        }
        // Init Grid


        // Read player input and swap animals

        // Start coroutine:
        // Spawn Animal
        // Mathces?
        // Make Animals Explode
        // Replace empty spot
        // Is Game Over?

    }
}