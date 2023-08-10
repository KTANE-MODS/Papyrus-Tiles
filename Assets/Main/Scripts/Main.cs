﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Reflection;
using NUnit.Framework.Constraints;
using System.Xml;

public class Main : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    Cell[,] grid;

    KMSelectable[] buttons;

    [SerializeField]
    Material[] materials; // red, orange, green, blue, purple, pink

    [SerializeField]
    GameObject orange;

    [SerializeField]
    GameObject lemon;

    [SerializeField]
    GameObject heart;

    [SerializeField]
    GameObject gridGameObject;

    [SerializeField]
    Material[] enemyMaterials;

    Smell currentSmell;

    bool recursionAtGoal;
    private Cell recursionCurrentCell;
    private List<Cell> recursionCellList;
    private List<Cell> recursionCellListSimplified;


    List<string> recursionDirections;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;
    bool debug = false;
    private bool pressable;
    private bool fightingMonster;


    void Awake()
    {

        ModuleSolved = false;
        ModuleId = ModuleIdCounter++;

        heart.SetActive(false);
        buttons = GetComponent<KMSelectable>().Children;

        Cell.redMaterial = materials[0];
        Cell.orangeMaterial = materials[1];
        Cell.greenMaterial = materials[2];
        Cell.blueMaterial = materials[3];
        Cell.purpleMaterial = materials[4];
        Cell.pinkMaterial = materials[5];

        grid = new Cell[6, 8];
        if (!debug)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    grid[row, col] = new Cell(row, col, buttons[index]);

                    buttons[index].OnInteract += delegate () { if(pressable && !fightingMonster) StartCoroutine(ButtonPress(buttons[index])); return false; };
                }
            }

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
                Logging("Couldn't generate a good maze. Generate default maze...");

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

        GetThroughMaze();

       return AtGoal();
    }

    void GenerateDebugMaze()
    {
        int[,] grid1 = new int[,]
        {
            { 2, 5, 0, 1, 0, 3, 0, 3},
            { 5, 1, 4, 4, 4, 4, 3, 2},
            { 5, 2, 2, 5, 5, 1, 2, 2},
            { 0, 3, 1, 3, 4, 4, 0, 5},
            { 0, 1, 4, 3, 2, 3, 1, 3},
            { 0, 2, 1, 1, 5, 5, 2, 0},
        };

        int[,] grid2 = new int[,]
        {
            { 0, 3, 3, 0, 3, 2, 3, 5},
            {1, 5, 5, 5, 3, 0, 2, 1},
            { 0, 0, 2, 2, 4, 2, 3, 5},
            { 0, 2, 2, 1, 1, 3, 2, 0},
            { 3, 3, 1, 5, 4, 3, 4, 1},
            { 1, 1, 2, 1, 2, 0, 1, 2},
        };

        int[,] grid3 = new int[,]
        {
            { 1, 0, 0, 1, 3, 0, 5, 2},
            { 1, 4, 3, 3, 1, 5, 4, 3},
            { 2, 3, 1, 0, 2, 4, 0, 1},
            { 0, 5, 4, 4, 0, 1, 0, 2},
            { 5, 2, 3, 1, 0, 5, 2, 5},
            { 0, 5, 2, 2, 1, 1, 2, 4},
        };

        int[,] grid4 = new int[,]
        {
            { 3, 0, 1, 5, 0, 1, 1, 3},
            { 1, 4, 5, 3, 4, 4, 1, 2},
            { 5, 1, 4, 2, 2, 2, 2, 3},
            { 3, 0, 3, 4, 0, 2, 4, 5},
            { 2, 3, 0, 2, 2, 2, 4, 3},
            { 2, 3, 0, 1, 1, 2, 1, 5},
        };

        int[,] grid5 = new int[,]
        {
            { 1, 1, 1, 5, 1, 5, 0, 3},
            { 5, 2, 1, 3, 1, 4, 2, 0},
            { 3, 3, 1, 1, 2, 4, 4, 3},
            { 1, 1, 5, 2, 0, 3, 3, 2},
            { 3, 0, 0, 1, 4, 0, 4, 5},
            { 3, 5, 2, 0, 3, 3, 2, 3},
        };

        int[,] grid6 = new int[,]
        {
        { 1, 2, 2, 2, 1, 0, 1, 5},
        { 2, 5, 0, 5, 1, 4, 1, 5},
        { 1, 2, 1, 2, 5, 4, 4, 3},
        { 5, 0, 5, 3, 4, 3, 4, 5},
        { 2, 1, 3, 5, 4, 5, 4, 2},
        { 0, 1, 2, 0, 2, 1, 2, 0},
        };

        /*
        int[,] grid2 = new int[,]
        {
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },

        };*/

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                grid[row, col] = new Cell(row, col, buttons[index], (Tile)grid6[row, col]);
            }
        }

        SetNeighbors();

        GetThroughMaze();
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
            }
        }
        
        currentCell.HasPlayer = true;
        float elaspedTime = 0f;

        Vector3 finalDestiantion = currentCell.Button.transform.position;
        Vector3 oldHeartPosition = heart.transform.position;
        
        if (!firstPress)
        {
            while (elaspedTime < maxTime)
            {
                float t = elaspedTime / maxTime;
                Vector3 newPos = Vector3.Lerp(oldHeartPosition, finalDestiantion, t);
                heart.transform.position = new Vector3(newPos.x, oldHeartPosition.y, newPos.z);
                elaspedTime += Time.deltaTime;
                yield return null;
            }
        }

        else
        {
            heart.transform.position = new Vector3(finalDestiantion.x, oldHeartPosition.y, finalDestiantion.z);
        }  
    }


    void GetThroughMaze()
    {
        List<Cell> startingCells = new List<Cell>();
        List<Cell> endingCells = new List<Cell>();

        for (int i = 0; i < 6; i++)
        {
            if (grid[i, 0].Tile.ToString() != "Red")
            {
                startingCells.Add(grid[i, 0]);
            }

            if (grid[i, 7].Tile.ToString() != "Red")
            {
                endingCells.Add(grid[i, 7]);
            }
        }

        bool foundPath = false;

        for (int startIndex = 0; startIndex < startingCells.Count; startIndex++)
        {
            for (int endIndex = 0; endIndex < endingCells.Count; endIndex++)
            {
                Cell start = startingCells[startIndex];
                Cell end = endingCells[endIndex];

                foundPath = FindPathRecursion(start, end);

                if (foundPath)
                {
                    break;
                }
            }

            if (foundPath)
            {
                break;
            }
        }

        if (foundPath)
        {
            recursionCellListSimplified = SimplifyAnswer(recursionCellList);
            Logging($"Final Answer: " + LogList(recursionCellList));
            if (recursionCellListSimplified.Count == recursionCellList.Count)
            {
                Logging($"Final Answer (Simplified): " + LogList(recursionCellListSimplified));
            }
            Debug.Log($"#{ModuleId} {(recursionCellListSimplified.Count == recursionCellList.Count ? "Lists are the same" : "Lists are different")}");
        }

        else
        {
            Debug.Log("Couldn't find a path to complete this maze");
        }
    }

    bool FindPathRecursion(Cell start, Cell end)
    {
        recursionDirections = new List<string>();
        recursionCellList = new List<Cell>();
        foreach (Cell c in grid)
        {
            c.Visited = false;
            c.Valid = c.Tile != Tile.Red;
        }

        recursionCurrentCell = start;
        recursionCellList.Add(start);

        if (MoveNorth(end) || MoveEast(end) || MoveSouth(end) || MoveWest(end))
        {
            return true;
        }

        else
        {
            Debug.Log($"Could not find path from {start} to {end}");
            return false;
        }

    }

    bool MoveNorth(Cell end)
    {
        //if we can move up, and we didnt move done before, go up

        Cell next = recursionCurrentCell.Up;


        Debug.Log($"Attempting to move up to {next}\n");

        bool validPathMovingNorth = false;
        bool checkValidity = false;
        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "DOWN") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Up;
            recursionDirections.Add("UP");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingNorth = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingNorth)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go up if path is valid
            if (validPathMovingNorth)
            {
                recursionAtGoal = MoveNorth(end);

                //if movig north doesn't work, move east
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveEast(end);

                    //if moving east doesn't work, move south
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveSouth(end);

                        //if moving south doesn't work, move west
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveWest(end);

                            //if moving west doesn't work, mark this position as
                            //unavailable and move back south
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Up doesn't lead anywhere. Moving back down.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Down;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back down and say you couldnt go north
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Down;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving up lead to an invalid path. Going back down to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveEast(Cell end)
    {
        //if we can move east, and we didnt move west before, go east
        Cell next = recursionCurrentCell.Right;
        Debug.Log($"Attempting to move right to {next}\n");


        bool validPathMovingEast = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "LEFT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Right;
            recursionDirections.Add("RIGHT");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingEast = ValidPath(recursionCellList);
            checkValidity = true;

        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingEast)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go east if path is valid
            if (validPathMovingEast)
            {
                recursionAtGoal = MoveEast(end);

                //if movig east doesn't work, move south
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveSouth(end);

                    //if moving south doesn't work, move west
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveWest(end);

                        //if moving west doesn't work, move north
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveNorth(end);

                            //if moving east doesn't work, mark this position as
                            //unavailable and move back west
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Right doesn't lead anywhere. Moving back Left.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Left;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back weast and say you couldnt go east
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Left;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving right lead to an invalid path. Going back left to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveSouth(Cell end)
    {
        //if we can move south, and we didnt move north before, go south
        Cell next = recursionCurrentCell.Down;
        Debug.Log($"Attempting to move down to {next}\n");
        bool validPathMovingSouth = false;
        bool checkValidity = false;


        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "UP") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Down;
            recursionDirections.Add("DOWN");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingSouth = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingSouth)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go south if path is valid
            if (validPathMovingSouth)
            {
                recursionAtGoal = MoveSouth(end);

                //if movig south doesn't work, move west
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveWest(end);

                    //if moving west doesn't work, move north
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveNorth(end);

                        //if moving north doesn't work, move east
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveEast(end);

                            //if moving east doesn't work, mark this position as
                            //unavailable and move back north
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Down doesn't lead anywhere. Moving back up.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Up;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back north and say you couldnt go south
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Up;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving down lead to an invalid path. Going back up to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveWest(Cell end)
    {
        //if we can move west, and we didnt move east before, go west
        Cell next = recursionCurrentCell.Left;
        Debug.Log($"Attempting to move left to {next}\n");
        bool validPathMovingWest = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "RIGHT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Left;
            recursionDirections.Add("LEFT");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingWest = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingWest)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go west if path is valid
            if (validPathMovingWest)
            {
                recursionAtGoal = MoveWest(end);

                //if movig west doesn't work, move north
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveNorth(end);

                    //if moving north doesn't work, move east
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveEast(end);

                        //if moving east doesn't work, move south
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveSouth(end);

                            //if moving south doesn't work, mark this position as
                            //unavailable and move back east
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Left doesn't lead anywhere. Moving back right.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Right;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back east and say you couldnt go west
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Right;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving left lead to an invalid path. Going back right to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool UnVistedNonPurpleCell(Cell c)
    {
        if (!recursionCellList.Contains(c))
        {
            return true;
        }

        if (recursionCellList.Contains(c) && c.Tile == Tile.Purple)
        {
            return true;
        }

        return false;
    }

    bool AtGoal(Cell end)
    {
        return recursionCellList.Contains(end);
    }

    bool AtGoal()
    {
        return recursionCellList.Any(x => x.Col == 7);
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
                if (current.Col == 7)
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
            if (c.Button == button)
            {
                return c;
            }
        }

        return null;
    }
    void ResetModule()
    {
        foreach (Cell c in grid)
        {
            c.HasPlayer = false;
        }

        heart.SetActive(false);
        SetSmell(Smell.None);
    }

    IEnumerator ButtonPress(KMSelectable button)
    {
        pressable = false;
        button.AddInteractionPunch(.1f);
        Cell selectedCell = GetCell(button);
        Cell playerCell = FindPlayer();

        float walkingTime = .5f;
        float runningTime = .2f;

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

                if (selectedCell.Tile == Tile.Orange)
                {
                    SetSmell(Smell.Orange);
                }

                Logging("Pressed " + selectedCell.ToString());
                heart.SetActive(true);
                yield return SetPlayer(selectedCell, true, walkingTime);
                pressable = true;
            }
        }

        else
        {
            //only neighbor cells can be interacted wih

            List<Cell> neighbors = playerCell.Neighbors;

            if (!neighbors.Contains(selectedCell))
            {
                yield break;
            }

            else
            {
                if (selectedCell.Tile == Tile.Red)
                {
                    pressable = true;
                    yield break;
                }

                Logging("Pressed: " + selectedCell.ToString());
                switch (selectedCell.Tile)
                {
                    case Tile.Orange:
                        SetSmell(Smell.Orange);
                        break;
                    case Tile.Blue:
                        if (currentSmell == Smell.Orange)
                        {

                            //yield return SetPlayer(selectedCell, false, walkingTime);
                            ////todo have a chomping noise play
                            //Strike("Strike! Got bit by pirahnas. Moving back to " + playerCell.ToString());
                            //yield return SetPlayer(playerCell, false, runningTime);
                            //pressable = true;
                            //yield break;
                        }
                        break;
                    case Tile.Purple:
                        //string direction = playerCell.Up == selectedCell ? "up" :
                        //                   playerCell.Down == selectedCell ? "down" :
                        //                   playerCell.Right == selectedCell ? "right" : "left";

                        //Cell currentCell = playerCell;
                        
                        //do
                        //{
                        //    Cell nextCell = direction == "up" ? currentCell.Up :
                        //                    direction == "down" ? currentCell.Down :
                        //                    direction == "right" ? currentCell.Right : currentCell.Left;

                        //    yield return SetPlayer(nextCell, false, walkingTime);
                        //    SetSmell(Smell.Lemon);
                        //    currentCell = nextCell;

                        //} while (currentCell.Tile == Tile.Purple);

                        //Logging("Moved to " + currentCell.ToString());

                        //if (currentCell.Tile == Tile.Red)
                        //{
                        //    Strike("Strike! Slid to a red tile. Restarting module...");
                        //    ResetModule();
                        //}

                        //else if (currentCell.Tile == Tile.Orange)
                        //{
                        //    SetSmell(Smell.Orange);
                        //}

                        //else if (currentCell.Tile == Tile.Green)
                        //{
                        //    //todo add code for monster fighting
                        //}
                        //pressable = true;
                        //yield break;

                    case Tile.Green:
                        HandleGreenTiles();
                        break;
                }

                    yield return SetPlayer(selectedCell, false, walkingTime);

                pressable = true;
            }
        }
    }

    void HandleGreenTiles()
    {

    }

    void Start()
    {
        pressable = true;
        fightingMonster = false;
    }

    void Update()
    {

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

        Debug.LogFormat($"[Papyrus Tiles #{ModuleId}] {s}");
            
    }

    private void Strike(string s)
    {
        GetComponent<KMBombModule>().HandleStrike();
        Logging(s);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
