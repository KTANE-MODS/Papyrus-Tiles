using System.Collections.Generic;
using System.Linq;

public class GameState {

	public Cell playerCell { get; private set; } //wheret the player currently is
	private Smell currentSmell; //the smell of the player
	private List<GameState> nextGameStates; //the next states that the player can go from here
    public GameState parentGameState { get; private set; } //the previous state that got us here

    //state is only valid if all of the following are true

    public bool IsValid { get 
		{
			return
                //the player is not on a yellow square
                playerCell.Tile != Tile.Yellow &&
                //the player is not on a red square
                playerCell.Tile != Tile.Red &&
				//the player is not on a blue square surrounded by yellow squares
				!(playerCell.Tile == Tile.Blue && playerCell.Neighbors.Where(c => c != null).Any(c => c.Tile == Tile.Yellow)) &&
				//the player is not on a blue square smelling like oranges
				!(playerCell.Tile == Tile.Blue && currentSmell == Smell.Orange) &&
				//valid purple path
				ValidPurplePath();

                }
    }

	public GameState(Cell playerCell, Smell currentSmell, GameState parentGameState)
	{ 
		this.playerCell = playerCell;
		this.currentSmell = currentSmell;
		this.parentGameState = parentGameState;
	}

	//the player was previously on the purple square and went the same direction (if the went left to step on the purple square, they should continue to move left)
	private bool ValidPurplePath()
	{
		//if prev the cell is not purple, it's valid
		if (parentGameState == null || parentGameState.playerCell.Tile != Tile.Purple)
		{
			return true;
		}

		Cell previousCell = parentGameState.playerCell;
		Cell previousCell2 = parentGameState.parentGameState.playerCell; //the previous cell from two states ago

        //if it's purple, check that the flow is the same
		//if went right, continue right
        if (previousCell2.Right == previousCell && previousCell.Right == playerCell)
		{
			return true;
		}

        //if went down, continue down
        if (previousCell2.Down == previousCell && previousCell.Down == playerCell)
        {
            return true;
        }
        //if went up, continue up
        if (previousCell2.Up == previousCell && previousCell.Up == playerCell)
        {
            return true;
        }
        //if went left, continue left
        if (previousCell2.Left == previousCell && previousCell.Left == playerCell)
        {
            return true;
        }

		//if none of the statements above are true, this can't be a valid state
		return false;
    }

    public static bool StatesAreSame(GameState s1, GameState s2)
	{
		return s1.playerCell == s2.playerCell && s1.currentSmell == s2.currentSmell;
	}

	public List<GameState> GetAvaiableGameStatesNieghbors()
	{ 
		List<GameState> nextStates = new List<GameState>();

		//get all the non null neigbors
		List<Cell> neighbors = playerCell.Neighbors.Where(c => c != null).ToList();

		foreach (Cell newPos in neighbors)
		{
			Smell newSmell;
			//check the smell is going to be
			if (newPos.Tile == Tile.Purple)
			{
				newSmell = Smell.Lemon;
			}

			else if (newPos.Tile == Tile.Orange)
			{
				newSmell = Smell.Orange;
			}

			else
			{
				newSmell = currentSmell;
			}

            nextStates.Add(new GameState(newPos, newSmell, this));
        }

		//only keep states that are valid
		return nextStates.Where(s => s.IsValid).ToList();
		
	}
}
