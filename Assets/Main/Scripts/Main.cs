using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Rnd = UnityEngine.Random;


public class Main : MonoBehaviour
{
    [SerializeField]
    private bool debug;

    private KMAudio Audio;

    private Cell[,] grid;

    private bool colorBlindOn;

    [SerializeField]
    private TextMesh cbTextPrefab;

    [SerializeField]
    private CellSelectable[] buttons;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Color32[] colors; // red, orange, green, blue, purple, pink, yellow

    [SerializeField]
    private GameObject orange;

    [SerializeField]
    private GameObject lemon;

    [SerializeField]
    private GameObject heart;

    [SerializeField]
    private GameObject exclamationPoint;

    [SerializeField]
    private GameObject gridGameObject;

    [SerializeField]
    private GameObject fightingGameObjects;

    [SerializeField]
    private Material[] enemyMaterials;

    [SerializeField]
    private GameObject bar;

    [SerializeField]
    private MeshRenderer enemyRenderer;

    [SerializeField]
    private Image currentHealthBar;
    private RectTransform rectTransform;

    [SerializeField]
    private AudioClip[] audioClips; //knife, encounter 1, encounter 2, love, hit, walk, victory, chomp, zap

    private KMSelectable resetButton;

    private bool focused;

    private Smell currentSmell;

    private List<Cell> shortestPath;
    private List<Cell> shortestPathSimplified;



    static int ModuleIdCounter = 1;
    private int ModuleId;
    private bool ModuleSolved;
    
    private bool pressable;
    private bool fightingMonster;
    private float monsterHealth;
    private float maxHealth;
    private float currentPercentage;
    private bool printDebugLines = false;
    private bool spacePress;
    private bool tpSpacePress;

    SpriteRenderer barSpriteRenderer;
    float[] greenHit = new float[] { -0.0927f, -0.0815f };
    float[] yellowHit = new float[] { -0.1159f, -0.0586f };



    void Awake()
    {
        colorBlindOn = GetComponent<KMColorblindMode>().ColorblindModeActive;
        barSpriteRenderer = bar.transform.GetComponent<SpriteRenderer>();
        exclamationPoint.SetActive(false);
        rectTransform = currentHealthBar.GetComponent<RectTransform>();
        Audio = GetComponent<KMAudio>();
        ModuleSolved = false;
        ModuleId = ModuleIdCounter++;

        heart.SetActive(false);

        resetButton = GetComponent<KMSelectable>().Children.Last();

        
        resetButton.OnInteract += delegate () { if (pressable && !fightingMonster && !ModuleSolved) { resetButton.AddInteractionPunch(.1f); ResetModule(); } return false; };


        Cell.red = colors[0];
        Cell.orange = colors[1];
        Cell.green = colors[2];
        Cell.blue = colors[3];
        Cell.purple = colors[4];
        Cell.pink = colors[5];
        Cell.yellow = colors[6];

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

        SetSmell(Smell.None);
    }

    bool GenerateMaze()
    {

        //dont have purple spawn on edges

        foreach (Cell c in grid)
        {
            c.SetRandomTile();
        }

        for (int i = 0; i < 8; i++)
        {
            RegenerateCell(grid[0, i]);
            RegenerateCell(grid[5, i]);

            if (i < 6)
            {
                RegenerateCell(grid[i, 7]);
                RegenerateCell(grid[i, 0]);
            }

            
        }

       return FindShortestPath();
    }

    private void SetUpColorBlindText()
    {
        if (!colorBlindOn)
            return;
        foreach (Cell c in grid)
        {
            Transform parentTransform = c.Button.transform;
            c.ColorBlidTextMesh = Instantiate(cbTextPrefab, parentTransform);
            string color = c.GetColor();
            c.ColorBlidTextMesh.text = color == "Pink" ? "I" : "" + color[0];
        }
    }

    void GenerateDebugMaze()
    {

        
        int[,] grid = new int[,]
        {
       {(int)Tile.Pink,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Red ,(int)Tile.Green ,(int)Tile.Pink ,(int)Tile.Blue ,(int)Tile.Pink },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Pink ,(int)Tile.Red ,(int)Tile.Orange ,(int)Tile.Green ,(int)Tile.Pink ,(int)Tile.Green },
        {(int)Tile.Green,(int)Tile.Blue , (int)Tile.Green, (int)Tile.Red ,(int)Tile.Orange ,(int)Tile.Pink ,(int)Tile.Green ,(int)Tile.Blue },
        {(int)Tile.Green,(int)Tile.Green ,(int)Tile.Purple ,(int)Tile.Green ,(int)Tile.Orange ,(int)Tile.Purple ,(int)Tile.Blue ,(int)Tile.Green },
        {(int)Tile.Red,(int)Tile.Blue ,(int)Tile.Red ,(int)Tile.Red ,(int)Tile.Green ,(int)Tile.Green ,(int)Tile.Pink ,(int)Tile.Pink },
        {(int)Tile.Red,(int)Tile.Green ,(int)Tile.Blue ,(int)Tile.Orange ,(int)Tile.Orange ,(int)Tile.Red ,(int)Tile.Green ,(int)Tile.Orange },

        };

        /*
        int[,] grid2 = new int[,]
        {
       {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },

        };*/

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                this.grid[row, col] = new Cell(row, col, buttons[index], (Tile)grid[row, col]);
            }
        }

        SetNeighbors();
        FindShortestPath();
    }

    Cell FindPlayer()
    {
        foreach (Cell c in grid)
        {
            if (c.HasPlayer)
            {
                return c;
            }
        }

        return null;
    }

    IEnumerator SetPlayer(Cell currentCell, bool firstPress, float maxTime)
    {
        foreach (Cell c in grid)
        {
            if (c.HasPlayer)
            {
                c.HasPlayer = false;
                break;
            }
        }
        
        currentCell.HasPlayer = true;
        float elaspedTime = 0f;

        Vector3 finalDestination = currentCell.Button.transform.localPosition;
        Vector3 oldHeartPosition = heart.transform.localPosition;
        
        if (!firstPress)
        {
            if (maxTime == audioClips[5].length)
            {
                Audio.PlaySoundAtTransform(audioClips[5].name, transform);
            }
            while (elaspedTime < maxTime)
            {
                float t = elaspedTime / maxTime;
                Vector3 newPos = Vector3.Lerp(oldHeartPosition, finalDestination, t);
                heart.transform.localPosition = new Vector3(newPos.x, oldHeartPosition.y, newPos.z);
                elaspedTime += Time.deltaTime;
                yield return null;
            }
        }

        else
        {
            heart.transform.localPosition = new Vector3(finalDestination.x, oldHeartPosition.y, finalDestination.z);
        }  
    }

    bool ValidPath(List<Cell> path)
    {
        Smell smell = Smell.None;

        for (int i = 0; i < path.Count; i++)
        {
            Cell c = path[i];
            string color = c.GetColor();
            //cant land on blue smelling like oranges
            if (color == "Blue" && smell == Smell.Orange)
            {
                return false;
            }

            if (color == "Orange")
            {
                smell = Smell.Orange;
            }

            else if (color == "Purple")
            {
                smell = Smell.Lemon;

                if (i != path.Count - 1)
                {
                    Cell next = path[i + 1];

                    //if you on a purple cell, and you are not going the same direction that you wernt before, this is not valid

                    Cell previous = path[i - 1];
                    Cell actualNext = previous.Up == c ? c.Up : previous.Right == c ? c.Right : previous.Down == c ? c.Down : c.Left;

                    if (actualNext != next)
                    {
                        return false;
                    }

                    if (next.GetColor() == "Red")
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    List<Cell> SimplifyAnswer(List<Cell> list)
    {
        List<Cell> newList = new List<Cell>();

        for (int i = 0; i < list.Count; i++)
        {
            Cell current = list[i];
            Cell previous = null;

            if (i != 0)
            {
                previous = list[i - 1];
            }

            newList.Add(current);

            if (previous != null && previous.GetColor() == "Purple")
            {
                newList.Remove(newList.Last());
            }

            if (newList.Any(x => x.Col == 7) || current.Col == 7)
            {
                if (current.Col == 7 && newList.Last().Tile != Tile.Purple)
                {
                    newList.Add(current);
                }
                break;
            }

        }

        return newList.Distinct().ToList();
    }

    void SetNeighbors()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Cell c = grid[row, col];
                c.Up = row - 1 < 0 ? null : grid[row - 1, col];
                c.Down = row + 1 > 5 ? null : grid[row + 1, col];
                c.Left = col - 1 < 0 ? null : grid[row, col - 1];
                c.Right = col + 1 > 7 ? null : grid[row, col + 1];
            }
        }
    }

    Cell GetCell(KMSelectable button)
    {
        foreach (Cell c in grid)
        {
            if (c.Button.Selectable == button)
            {
                return c;
            }
        }

        return null;
    }
    void ResetModule()
    {
        Logging("Resetting module...");
        foreach (Cell c in grid)
        {
            c.HasPlayer = false;

            if (colorBlindOn)
            {
                c.SetColorBlindTextMeshVisisbilty(true);
            }
        }

        heart.SetActive(false);
        SetSmell(Smell.None);
    }

    IEnumerator ButtonPress(KMSelectable button)
    {
        pressable = false;
        Cell selectedCell = GetCell(button);
        Cell playerCell = FindPlayer();

        float walkingTime = audioClips[5].length;
        float runningTime = audioClips[7].length;

        //if the user is not on the grid make sure they press a button in the first column
        if (playerCell == null)
        {
            if (selectedCell.Col != 0)
            {
                pressable = true;
                yield break;
            }

            else
            {
                if (selectedCell.Tile == Tile.Red)
                {
                    pressable = true;
                    yield break;
                }

                if (selectedCell.Tile == Tile.Yellow)
                {
                    yield return HandleYellow(playerCell, selectedCell);
                    pressable = true;
                    yield break;
                }

                if (HasYellowNeigbors(selectedCell))
                {
                    if (colorBlindOn)
                    {
                        selectedCell.SetColorBlindTextMeshVisisbilty(false);
                    }

                    yield return SetPlayer(selectedCell, true, walkingTime);
                    yield return HandleYellow(playerCell, selectedCell);
                    pressable = true;
                    yield break;
                }

                if (selectedCell.Tile == Tile.Orange)
                {
                    SetSmell(Smell.Orange);
                }

                if (colorBlindOn)
                {
                    selectedCell.SetColorBlindTextMeshVisisbilty(false);
                }

                Logging("Pressed " + selectedCell.ToString());
                heart.SetActive(true);
                yield return SetPlayer(selectedCell, true, walkingTime);

                if (selectedCell.Tile == Tile.Green)
                {
                    yield return HandleGreenTile();
                }
                pressable = true;
            }
        }

        else
        {
            //only neighbor cells can be interacted wih

            List<Cell> neighbors = playerCell.Neighbors;

            if (!neighbors.Contains(selectedCell))
            {
                pressable = true;
                yield break;
            }

            
            if (selectedCell.Tile == Tile.Red)
            {
                pressable = true;
                yield break;
            }

            if (colorBlindOn)
            {
                playerCell.SetColorBlindTextMeshVisisbilty(true);
            }

            Logging("Pressed " + selectedCell.ToString());

            switch (selectedCell.Tile)
            {
                case Tile.Orange:
                    SetSmell(Smell.Orange);
                    break;

                case Tile.Blue:
                    //strike if you smell like oranges
                    if (currentSmell == Smell.Orange)
                    {
                        if (colorBlindOn)
                        {
                            selectedCell.SetColorBlindTextMeshVisisbilty(false);
                        }

                        yield return SetPlayer(selectedCell, false, walkingTime);
                        Audio.PlaySoundAtTransform(audioClips[7].name, transform);
                        Strike("Got bit by pirahnas. Moving back to " + playerCell.ToString());
                        yield return SetPlayer(playerCell, false, runningTime);

                        if (colorBlindOn)
                        {
                            playerCell.SetColorBlindTextMeshVisisbilty(false);
                            selectedCell.SetColorBlindTextMeshVisisbilty(true);
                        }
                            
                        pressable = true;
                        yield break;
                    }

                    //strike if yellow is adjacent tile
                    if (HasYellowNeigbors(selectedCell))
                    {
                        if (colorBlindOn)
                        {
                            selectedCell.SetColorBlindTextMeshVisisbilty(false);
                        }

                        yield return SetPlayer(selectedCell, false, walkingTime);
                        yield return HandleYellow(playerCell, selectedCell);

                        pressable = true;
                        yield break;
                    }
                    break;

                case Tile.Yellow:
                    if (colorBlindOn)
                    {
                        selectedCell.SetColorBlindTextMeshVisisbilty(false);
                    }
                    yield return SetPlayer(selectedCell, false, walkingTime);
                    yield return HandleYellow(playerCell, selectedCell);
                    pressable = true;

                    yield break;

                case Tile.Purple:

                    string direction = GetDirection(playerCell, selectedCell);
                    Cell currentCell = playerCell;

                    do
                    {
                        Cell nextCell = GetNewCellViaDirection(currentCell, direction);

                        if (colorBlindOn)
                        {
                            currentCell.SetColorBlindTextMeshVisisbilty(true);
                            nextCell.SetColorBlindTextMeshVisisbilty(false);
                        }

                        yield return SetPlayer(nextCell, false, walkingTime);
                        SetSmell(Smell.Lemon);
                        currentCell = nextCell;

                    } while (currentCell.Tile == Tile.Purple);

                    Logging("Moved to " + currentCell.ToString());

                    if (currentCell.Tile == Tile.Red)
                    {
                        Strike("Slid to a red tile.");
                        ResetModule();
                    }

                    else if (HasYellowNeigbors(currentCell))
                    {
                        yield return HandleYellow(playerCell, currentCell);
                    }

                    //todo check this
                    else if (currentCell.Tile == Tile.Yellow)
                    {
                        yield return HandleYellow(playerCell, currentCell);
                    }

                    else if (currentCell.Tile == Tile.Orange)
                    {
                        SetSmell(Smell.Orange);
                    }

                    else if (currentCell.Tile == Tile.Green)
                    {
                        yield return HandleGreenTile();
                    }

                    if (currentCell.Col == 7)
                    {
                        Solve();
                    }
                    pressable = true;
                    yield break;

                case Tile.Green:

                    if (colorBlindOn)
                    { 
                        selectedCell.SetColorBlindTextMeshVisisbilty(false);
                    }

                    yield return SetPlayer(selectedCell, false, walkingTime);
                    yield return HandleGreenTile();
                    if (FindPlayer().Col == 7)
                    {
                        Solve();
                    }
                    pressable = true;
                    yield break;
            }

            if (colorBlindOn)
            { 
                selectedCell.SetColorBlindTextMeshVisisbilty(false);
            }

            yield return SetPlayer(selectedCell, false, walkingTime);
            pressable = true;


            if (FindPlayer().Col == 7)
            {
                Solve();
            }
        }
    }

    private IEnumerator HandleYellow(Cell playerCell, Cell selectedCell)
    {
        //if this is the first cell and the player is not on the board yet, instantly play the shock sound and strike the player 
        if (playerCell == null && selectedCell.Col == 0)
        {
            //put player on cell
            heart.SetActive(true);
            yield return SetPlayer(selectedCell, true, 0f);
        }

        yield return null;

        //play zap sound
        AudioClip clip = audioClips[8];
        Audio.PlaySoundAtTransform(clip.name, transform);
        yield return new WaitForSeconds(clip.length);
        
        Strike("Stepped on a yellow tile");
        
        //reset module
        ResetModule();

        //what if the player slides on a yellow
    }

    private bool HasYellowNeigbors(Cell cell)
    {
        return cell.Tile == Tile.Blue && cell.Neighbors.Where(c => c != null).Any(c => c.Tile == Tile.Yellow);
    }

    IEnumerator HandleGreenTile()
    {
        fightingMonster = true;
        enemyRenderer.materials = new Material[] { enemyMaterials[Rnd.Range(0, enemyMaterials.Length)] };
        currentHealthBar.rectTransform.anchorMax = new Vector2(1, 1);
        Vector3 heartPos = heart.transform.localPosition;
        exclamationPoint.transform.localPosition = new Vector3(heartPos.x, heartPos.y, heartPos.z + 0.00961666f);
        exclamationPoint.SetActive(true);
        monsterHealth = 9;
        maxHealth = 9;
        currentPercentage = 1f;
        Audio.PlaySoundAtTransform(audioClips[1].name, transform);
        yield return new WaitForSeconds(audioClips[1].length + .1f);
        float flashLength = 0.12f;
        exclamationPoint.SetActive(false);
        Audio.PlaySoundAtTransform(audioClips[2].name, transform);

        for (int i = 0; i < 3; i++)
        {
            heart.SetActive(false);
            yield return new WaitForSeconds(flashLength / 2);
            heart.SetActive(true);
            yield return new WaitForSeconds(flashLength / 2);
        }

        yield return new WaitForSeconds(audioClips[2].length - flashLength - .6f);
        gridGameObject.SetActive(false);
        fightingGameObjects.SetActive(true);

        do 
        {
            yield return MoveBar();

        } while (monsterHealth > 0);

        Audio.PlaySoundAtTransform(audioClips[3].name, transform);
        fightingGameObjects.SetActive(false);
        gridGameObject.SetActive(true);
        fightingMonster = false;
    }

    IEnumerator MoveBar()
    {
        spacePress = false;
        tpSpacePress = false;
        float moveWhiteMaxTime = 1.25f;
        float elaspedTime;


        Vector3 leftPos = new Vector3(-0.1674f, -0.0633f, 0.0541f);
        Vector3 rightPos = new Vector3(-0.0079f, -0.0633f, 0.0541f);

        bar.transform.localPosition = leftPos;

        do
        {
            elaspedTime = 0f;
            while (elaspedTime < moveWhiteMaxTime)
            {
                if (focused && (Input.GetKeyDown(KeyCode.Space) || tpSpacePress))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(leftPos, rightPos, t);
                elaspedTime += Time.deltaTime;

                float barX = bar.transform.localPosition.x;

                if (barX >= greenHit[0] && barX <= greenHit[1])
                {
                    barSpriteRenderer.color = Color.green;
                }

                else if (barX >= yellowHit[0] && barX <= yellowHit[1])
                {
                    barSpriteRenderer.color = Color.yellow;
                }

                else
                {
                    barSpriteRenderer.color = Color.white;
                }
                yield return null;
            }

            if (spacePress)
            {
                break;
            }


            elaspedTime = 0f;
            while (elaspedTime < moveWhiteMaxTime)
            {
                if (focused && (Input.GetKeyDown(KeyCode.Space) || tpSpacePress))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(rightPos, leftPos, t);
                elaspedTime += Time.deltaTime;

                float barX = bar.transform.localPosition.x;

                if (barX >= greenHit[0] && barX <= greenHit[1])
                {
                    barSpriteRenderer.color = Color.green;
                }

                else if (barX >= yellowHit[0] && barX <= yellowHit[1])
                {
                    barSpriteRenderer.color = Color.yellow;
                }

                else
                {
                    barSpriteRenderer.color = Color.white;
                }

                yield return null;
            }

        } while (!spacePress);

        yield return SwingKnife();
        yield return DepleteHealth(monsterHealth / maxHealth);
    }

    IEnumerator SwingKnife()
    {
        animator.SetTrigger("Trigger Knife Swing");
        Audio.PlaySoundAtTransform(audioClips[0].name, transform);
        yield return new WaitForSeconds(audioClips[0].length);

        float barX = bar.transform.localPosition.x;
        if (barX >= greenHit[0] && barX <= greenHit[1])
        {
            monsterHealth -= maxHealth / 2;
        }

        else if (barX >= yellowHit[0] && barX <= yellowHit[1])
        {
            monsterHealth -= maxHealth / 3;
        }

        else
        {
            monsterHealth -= maxHealth / 4;
        }
    }

    IEnumerator DepleteHealth(float newPercentage)
    {
        Audio.PlaySoundAtTransform(audioClips[4].name, transform);
        float elaspedTime;

        elaspedTime = 0f;
        float maxDepleteHealthTime = audioClips[4].length;
        while (elaspedTime < maxDepleteHealthTime)
        {
            float t = elaspedTime / maxDepleteHealthTime;
            rectTransform.anchorMax = new Vector2(Mathf.Lerp(currentPercentage, newPercentage, t), 1f);
            elaspedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchorMax = new Vector2(newPercentage, 1f);
        currentPercentage = newPercentage;
    }

    void Start()
    {
        grid = new Cell[6, 8];
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                grid[row, col] = new Cell(row, col, buttons[index]);
                buttons[index].Selectable.OnInteract += delegate () { buttons[index].Selectable.AddInteractionPunch(.1f); if (pressable && !fightingMonster && !ModuleSolved) StartCoroutine(ButtonPress(buttons[index].Selectable)); return false; };
            }
        }
        if (!debug)
        {
            SetNeighbors();

            bool validMaze = false;

            int count = 0;
            do
            {
                count++;
                validMaze = GenerateMaze();
            } while (!validMaze && count < 100);

            if (count == 100 && !validMaze)
            {
                Logging("Couldn't generate a good maze. Generating default maze...");

                for (int row = 0; row < 6; row++)
                {
                    Tile tile = row == 2 || row == 3 ? Tile.Pink : Tile.Red;

                    for (int col = 0; col < 8; col++)
                    {
                        int index = row * 8 + col;
                        grid[row, col] = new Cell(row, col, buttons[index], tile);
                    }
                }
            }
        }

        else
        {
            GenerateDebugMaze();
            SetNeighbors();
        }

        SetUpColorBlindText();

        gridGameObject.SetActive(true);
        fightingGameObjects.SetActive(false);
        pressable = true;
        fightingMonster = false;
        LogGrid();

        Logging($"Final Answer: {LogList(shortestPath)}");
    }

    /// <summary>
    /// Finds the shortest path trhough the maze
    /// </summary>
    /// <returns>true if a path was found</returns>
    private bool FindShortestPath()
    {
        List<List<Cell>> paths = new List<List<Cell>>();

        for (int startRow = 0; startRow < 6; startRow++)
        {
            for (int endRow = 0; endRow < 6; endRow++)
            {
                paths.Add(PathFinder.FindPath(grid[startRow, 0], grid[endRow, 7]));
            }
        }

        //find the shortest paths (remove empty lists)
        List<Cell> shortestPath = paths.Where(p => p.Count > 0).OrderBy(p => p.Count).FirstOrDefault();

        if (shortestPath != null)
        {
            //if there are multiple paths that have the shortest path, choose the one that has the least amount of green tiles
            List<List<Cell>> shortestPaths = paths.Where(p => p.Count == shortestPath.Count).ToList();

            List<Cell> shortestGreenPath = shortestPaths.OrderBy(p => p.Count).FirstOrDefault().ToList();

            this.shortestPath = shortestGreenPath;
            shortestPathSimplified = SimplifyAnswer(shortestGreenPath);

            return true;
        }

        return false;
    }

    void SetSmell(Smell smell)
    {
        currentSmell = smell;

        switch (smell)
        {
            case Smell.Lemon:
                lemon.SetActive(true);
                orange.SetActive(false);
                break;
            case Smell.Orange:
                lemon.SetActive(false);
                orange.SetActive(true);
                break;
            default:
                lemon.SetActive(false);
                orange.SetActive(false);
                break;
        }
    }

    void RegenerateCell(Cell c)
    {
        while (c.Tile.ToString() == "Purple")
        {
            c.SetRandomTile();
        }
    }

    private string LogList(List<Cell> list)
    {
        return string.Join(" ", list.Select(x => x.ToString()).ToArray());
    }

    private void Logging(string s)
    {
        if (s == "")
        {
            return;
        }

        Debug.Log($"[Papyrus Tiles #{ModuleId}] {s}");
            
    }

    private void Solve()
    {
        Audio.PlaySoundAtTransform(audioClips[6].name, transform);
        Logging("Module solved");
        GetComponent<KMBombModule>().HandlePass();
        ModuleSolved = true;
    }

    private void Strike(string s)
    {
        GetComponent<KMBombModule>().HandleStrike();
        Logging($"Strike! {s}");
    }

    private void LogGrid()
    {
        string s = "";
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                s += grid[row, col].GetColor() == "Pink" ? "I " : grid[row, col].GetColor()[0] + " ";
            }


            Logging(s);
            s = "";

        }

        Logging(s);
    }

    private string GetDirection(Cell c1, Cell c2)
    {
        return c1.Up == c2 ? "up" : c1.Down == c2 ? "down" : c1.Right == c2 ? "right" : "left";
    }

    private Cell GetNewCellViaDirection(Cell c, string direction)
    {
        return direction == "up" ? c.Up :
               direction == "down" ? c.Down :
               direction == "right" ? c.Right : c.Left;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use `!{0} row col` to press the cell with `1 1` being top left. Put a comma between commands to chain them. Use `!{0} reset` to reset the module. If you land on a green tile, the fighting will be done for you.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;


        Command = Command.ToUpper();

        if (Command == "RESET")
        {
            resetButton.OnInteract();
            yield break;
        }

        string[] chainCommands = Command.ToUpper().Trim().Split(',').Select(x => x.Trim()).ToArray();

        foreach (string command in chainCommands) 
        {
            string response = ValidCommand(command);

            if (response != null)
            {
                yield return $"sendtochaterror {response}. Invalid command: `{command}`";
                yield break;
            }
        }

        foreach (string command in chainCommands)
        {
            string[] commands = command.Trim().Split(' ');

            int row = int.Parse(commands[0]);
            int col = int.Parse(commands[1]);

            row--;
            col--;




            KMSelectable button = buttons[row * 8 + col].gameObject.GetComponent<KMSelectable>();
            Cell cell = GetCell(button);
            Cell playerCell = FindPlayer();
            Cell upcomingGreenCell = null;

            if (playerCell == null && col != 0)
            {
                yield return $"sendtochaterror Your first command does not start in the first column. Given: `{command.Join("")}`";
                yield break;
            }


            //if you specifc a cell, and you're on said cell, move on
            if (playerCell == cell)
            { 
                continue;
            }

            if (cell.Tile == Tile.Red)
            {
                yield return $"sendtochat Stopping since trying to move on a red tile: {command}";
                yield break;
            }

            else if (cell.Tile == Tile.Purple)
            {
                //if the tile is purple, check to see where the player is (the player will always be on the grid)
                playerCell = FindPlayer();

                //depending on the direction the player is relative to the purple tile, check that direction one more unit and see if it's either purple or green
                string direction = GetDirection(playerCell, cell);

                Cell nextCell = GetNewCellViaDirection(cell, direction);

                //if it's purple, move in that direction again
                while (nextCell.Tile == Tile.Purple)
                {
                    nextCell = GetNewCellViaDirection(nextCell, direction);
                }

                //if it is green, then hold this cell in upcomingGreenCell
                if (nextCell.Tile == Tile.Green)
                {
                    upcomingGreenCell = nextCell;
                }
            }

            button.OnInteract();
            playerCell = FindPlayer();

            //if upcoming greenCell is not null, wait until the playerCell becomes this cell and call HandleFighting
            if (upcomingGreenCell != null)
            {
                while (playerCell != upcomingGreenCell)
                {
                    playerCell = FindPlayer();
                    yield return null;
                }
            }

            if (playerCell != null && playerCell.Tile == Tile.Green)
            {
                yield return HandleFighting();
            }

            while (!pressable)
            {
                yield return null;
            }
        }
    }

    string ValidCommand(string command)
    {
        string[] commands = command.Trim().Split(' ');

        if (commands.Length != 2)
        {
            return "not enough or too many commands to select cell";
        }

        int row, col;

        if (!int.TryParse(commands[0], out row) || !(row >= 1 && row <= 6))
        {
            return $"`{commands[0]}` is not a valid row";

        }

        if (!int.TryParse(commands[1], out col) || !(col >= 1 && col <= 8))
        {
            return $"`{commands[1]}` is not a valid column";
        }

        return null;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        focused = true;
        yield return ProcessTwitchCommand("Reset");

        foreach (Cell c in shortestPathSimplified)
        {
            string s = c.ToString().Replace("(","").Replace(")", "");
            yield return ProcessTwitchCommand(s);
        }

        while (!ModuleSolved)
        {
            yield return null;
        }
    }

    IEnumerator HandleFighting()
    {
        //idk why but chaining commands causes focused to be false for some reason
        focused = true;
        //wait for fight to be active
        while (gridGameObject.activeSelf)
        {
            yield return null;
        }
        do 
        {
            float barX = bar.transform.localPosition.x;
            if (barX >= greenHit[0] && barX <= greenHit[1])
            {
                tpSpacePress = true;
            }
            yield return null;
        } while (!gridGameObject.activeSelf);
        focused = false;
    }
}
