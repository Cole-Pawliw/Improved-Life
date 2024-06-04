using Godot;
using System;

public partial class GameOfLife : Node2D {
	
	Vector2I[] atlas_tiles = {new Vector2I(1, 1), new Vector2I(3, 2)}; // Locations of colours in atlas
	int tile_size = 16; // Size of tiles in the TileMap
	Vector2I last_cell = new Vector2I(119, 67); // Location of the bottom right cell

	TileMap game_board; // Stores the TileMap
	Label label; // Stores the Label

	int draw_mode = 1; // Determines the draw mode, 1 for draw, 0 for erase
	Vector2I mouse_coords; // The current tile that the mouse is hovering
	bool pause = true; // Stores if the game is paused or not

	double time = 0; // Tracks the time since the last update
	double update_time = 0.25; // The period for every update
	
	// Runs once at the start of the game
	public override void _Ready() {
		game_board = GetNode<TileMap>("Board");
		label = GetNode<Label>("Info");
	}

	// Runs once every frame
	public override void _Process(double delta) {
		// If start button is pressed, pause/play
		if (Input.IsActionJustPressed("start")) {
			pause = !pause;
			UpdateLabel();
		}
		// Toggles the mode to either draw or erase
		if (Input.IsActionJustPressed("toggle_erase")) {
			draw_mode = (draw_mode == 1) ? 0 : 1;
			UpdateLabel();
		}
		// Increment time
		if (Input.IsActionJustPressed("speed_up")) {
			update_time = Math.Round(update_time + 0.05, 2);
			UpdateLabel();
		}
		// Decrement time
		if (Input.IsActionJustPressed("speed_down")) {
			update_time = Math.Round(Math.Max(update_time - 0.05, 0.05), 2);
			UpdateLabel();
		}
		// Draw cells where the mouse is
		if (Input.IsActionPressed("draw") && pause) {
			game_board.SetCell(0, mouse_coords, 1, atlas_tiles[draw_mode]);
		}
		
		// If game is live, count time and update cells every update_time seconds
		if (!pause) {
			time += delta;
			if (time >= update_time) {
				time = time % update_time;
				UpdateBoard();
			}
		}
	}
	
	// Used to track mouse movement
	public override void _Input(InputEvent @event) {
		// Update the cell the mouse is hovering over when it moves
		if (@event is InputEventMouseMotion eventMouseMotion) {
			mouse_coords = new Vector2I((int)Math.Ceiling(eventMouseMotion.Position.X / tile_size),
					(int)Math.Ceiling(eventMouseMotion.Position.Y / tile_size)) - new Vector2I(1,1);
		}
	}
	
	// Calculate if each cell should be alive or dead the next day and update the TileMap
	public void UpdateBoard() {
		game_board.AddLayer(1);
		Vector2I current_cell = new Vector2I(0, 0);
		
		for (; current_cell.X <= last_cell.X; current_cell.X++) {
			for (; current_cell.Y <= last_cell.Y; current_cell.Y++) {
				int neighbors = NumNeighbors(current_cell);
				
				// Set the cell to alive or dead for the next day
				if (neighbors < 2 || neighbors > 3) { // if neighbors <2 or >3, the cell will always be dead next day
					game_board.SetCell(1, current_cell, 1, atlas_tiles[0]);
				} else if (neighbors == 3) { // if neighbors is 3, the cell will always be alive next day
					game_board.SetCell(1, current_cell, 1, atlas_tiles[1]);
				} else { // if neighbors is 2, the cell remains in the same state
					game_board.SetCell(1, current_cell, 1, game_board.GetCellAtlasCoords(0, current_cell));
				}
			}
			current_cell.Y = 0;
		}
		
		// Remove the old layer with outdated cells
		game_board.RemoveLayer(0);
	}
	
	// Determines the number of live cells neighboring the cell at a given location
	public int NumNeighbors(Vector2I location) {
		int neighbors = 0;
		Vector2I neighbor;
		
		// Iterate over the cell neighbors and count the living ones
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				neighbor = new Vector2I(location.X + i, location.Y + j);
				// Skip the cell at the given location
				if (neighbor == location) {
					continue;
				}
				
				// If the current cell is alive, increment neighbors
				if (game_board.GetCellAtlasCoords(0, neighbor) == atlas_tiles[1]) {
					neighbors++;
				}
			}
		}
		
		return neighbors;
	}
	
	// Updates the label in the top right corner
	public void UpdateLabel() {
		string[] strs = {"Draw","Paused"};
		if (draw_mode == 0) {
			strs[0] = "Erase";
		}
		
		if (pause == false) {
			strs[1] = "Live";
		}
		
		// Label updated using variables to reduce if-else bloat
		label.Text = $"Mode: {strs[0]}\nGame: {strs[1]}\nPeriod: {update_time}s";
	}
}
