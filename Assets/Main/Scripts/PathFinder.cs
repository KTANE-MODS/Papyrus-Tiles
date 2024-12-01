using System.Collections.Generic;
using System.Linq;

public class PathFinder {

	public static List<Cell> FindPath(Cell startingCell, Cell goalCell)
	{
		Smell currentSmell = Smell.None;

        //check the smell is going to be
        if (startingCell.Tile == Tile.Purple)
        {
            currentSmell = Smell.Lemon;
        }

        else if (startingCell.Tile == Tile.Orange)
        {
            currentSmell = Smell.Orange;
        }
        //chcek if the starting state is valid
        GameState startingState = new GameState(startingCell, currentSmell, null);
        if(!startingState.IsValid)
        {
            return new List<Cell>();
        }

        List<GameState> vistedStates = new List<GameState>();
		Queue<GameState> queue = new Queue<GameState>();

        
        queue.Enqueue(startingState);

        //continue while the goal has not been found
        while (queue.Count > 0 && !vistedStates.Any(c => c.playerCell == goalCell))
        {
            GameState currentState = queue.Dequeue();

            //get all the valid neighbors
            List<GameState> currentStateNieghbors = currentState.GetAvaiableGameStatesNieghbors();

            foreach (GameState neighbor in currentStateNieghbors)
            {
                //don't check states we already visted
                if (vistedStates.Any(v => GameState.StatesAreSame(v, neighbor)))
                {
                    continue;
                }

                vistedStates.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        //find the shortest path
        GameState endNode = vistedStates.FirstOrDefault(s => s.playerCell == goalCell);

        //a path to the goal cell could not be found
        if (endNode == null)
        { 
            return new List<Cell>();
        }
        
        List<Cell> path = new List<Cell>();
        GameState current = endNode;

        while (current != null)
        {
            //add it to list
            path.Add(current.playerCell);

            //set new current state
            current = current.parentGameState;
        }

        path.Reverse();

        return path;
    }

}
