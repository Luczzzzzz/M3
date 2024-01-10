using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

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
        [SerializeField] Ease ease = Ease.InQuad;


        GridSystem2D<GridObject<Animal>> grid;

        InputReader InputReader;
        Vector2Int selectedAnimal = Vector2Int.one * -1;

        private void Awake()
        {
            InputReader = GetComponent<InputReader>();

        }

        private void Start()
        {
            InitializedGrid();
            InputReader.Fire += OnSelectAnimal;

        }
        private void OnDestroy()
        {
            InputReader.Fire -= OnSelectAnimal;
        }

        private void OnSelectAnimal()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(InputReader.Selected));

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos))
            {
                return;
            }

            if (selectedAnimal == gridPos)
            {
                DeselectGem();
            }
            else if (selectedAnimal == Vector2Int.one * -1)
            {
                SelectAnimal(gridPos);
            }
            else
            {
                StartCoroutine(RunGameLoop(selectedAnimal, gridPos));
            }
        }

        private bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

        private bool IsValidPosition(Vector2Int gridPos) => gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;

        private void SelectAnimal(Vector2Int gridPos) => selectedAnimal = gridPos;

        private void DeselectGem() => selectedAnimal = new Vector2Int(-1, -1);

        private IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            yield return StartCoroutine(SwapAnimals(gridPosA, gridPosB));

            // Mathces?
            List<Vector2Int> matches = FindMatches();
            // Make Animals Explode
            yield return StartCoroutine(ExplodeAnimal(matches));
            // Make animals fall
            yield return StartCoroutine(MakeAnimalFall());
            // Fill empty spots
            yield return StartCoroutine(FillEmptySpots());

            DeselectGem();
        }

        private IEnumerator FillEmptySpots()
        {
            //file empty spots
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        CreateAnimal(x, y);
                        //play woosh sound
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

        }

        private IEnumerator MakeAnimalFall()
        {
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        for (var i = y + 1; i < height; i++)
                        {
                            if (grid.GetValue(x, i) != null)
                            {
                                var animal = grid.GetValue(x, i).GetValue();
                                grid.SetValue(x, y, grid.GetValue(x, i));
                                grid.SetValue(x, i, null);
                                animal.transform.DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f).SetEase(ease);
                                // play woosh sound
                                yield return new WaitForSeconds(0.1f);
                                break;

                            }
                        }
                    }

                }
        }

        private IEnumerator ExplodeAnimal(List<Vector2Int> matches)
        {
            //play sound
            foreach (var match in matches)
            {
                var animal = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x, match.y, null);//位置置空
                //ExplodeVFX();
                animal.transform.DOPunchPosition(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

                animal.DestroyAnimal();
                yield return new WaitForSeconds(0.1f);

            }


        }

        private List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new HashSet<Vector2Int>();//hashset 不允许有重复  所以不用担心相同动物水平垂直都被选中导致收集两次

            //水平方向
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width - 2; x++)
                {
                    var animalA = grid.GetValue(x, y);
                    var animalB = grid.GetValue(x + 1, y);
                    var animalC = grid.GetValue(x + 2, y);

                    if (animalA == null || animalB == null || animalC == null) continue;

                    if (animalA.GetValue().GetType() == animalB.GetValue().GetType() && animalB.GetValue().GetType() == animalC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x + 1, y));
                        matches.Add(new Vector2Int(x + 2, y));
                    }

                }
            //垂直方向
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height - 2; y++)
                {
                    var animalA = grid.GetValue(x, y);
                    var animalB = grid.GetValue(x, y + 1);
                    var animalC = grid.GetValue(x, y + 2);

                    if (animalA == null || animalB == null || animalC == null) continue;

                    if (animalA.GetValue().GetType() == animalB.GetValue().GetType() && animalB.GetValue().GetType() == animalC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x, y + 1));
                        matches.Add(new Vector2Int(x, y + 2));
                    }

                }
            return new List<Vector2Int>(matches);
        }

        private IEnumerator SwapAnimals(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);

            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
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




    }
}