using CruiseControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CruiseControl
{
    public class Commander
    {
        public BoardStatus _currentBoard;
		private List<VesselStatus> _myVessels;
		private string[] _log = new string[50];
		private List<Coordinate> _SonarReport;
		private List<List<Coordinate>> _listObjects;

        public Commander()
        {
            _currentBoard = new BoardStatus();
        }

        // Do not alter/remove this method signature
        public List<Command> GiveCommands()
        {
            var cmds = new List<Command>();
			//comment
			foreach (var vesselStatus in _currentBoard.MyVesselStatuses)
			{
				_myVessels.Add(vesselStatus);
			}
			var vesselCount = _myVessels.Count();


			if (_currentBoard.TurnsUntilBoardShrink == 2)
			{
				foreach (var vessel in _myVessels)
				{
					foreach (var location in vessel.Location)
					{
						if (location.X == _currentBoard.BoardMinCoordinate.X &&
							(location.Y == _currentBoard.BoardMinCoordinate.Y || location.Y == _currentBoard.BoardMaxCoordinate.Y))
						{
							cmds.Add(new Command { vesselid = vessel.Id, action = "move:east" });
							_log[_log.Count()] = "left corners";
						}
						if (location.X == _currentBoard.BoardMaxCoordinate.X &&
							(location.Y == _currentBoard.BoardMinCoordinate.Y || location.Y == _currentBoard.BoardMaxCoordinate.Y))
						{
							cmds.Add(new Command { vesselid = vessel.Id, action = "move:west" });
							_log[_log.Count()] = "right corners";
						}
					}
				}
			}
			if (_currentBoard.TurnsUntilBoardShrink == 1)
			{
				foreach (var vessel in _myVessels)
				{
					bool doneX = false, doneY = false;
					foreach (var location in vessel.Location)
					{
						if (!doneX)
						{
							if (location.X == _currentBoard.BoardMinCoordinate.X)
							{
								cmds.Add(new Command { vesselid = vessel.Id, action = "move:east" });
								doneX = true;
								_log[_log.Count()] = "left edge";
							}
							if (location.X == _currentBoard.BoardMaxCoordinate.X)
							{
								cmds.Add(new Command { vesselid = vessel.Id, action = "move:west" });
								doneX = true;
								_log[_log.Count()] = "right edge";
							}
						}
						if (!doneY)
						{
							if (location.Y == _currentBoard.BoardMinCoordinate.Y)
							{
								cmds.Add(new Command { vesselid = vessel.Id, action = "move:south" });
								doneY = true;
								_log[_log.Count()] = "top edge";
							}
							if (location.Y == _currentBoard.BoardMaxCoordinate.Y)
							{
								cmds.Add(new Command { vesselid = vessel.Id, action = "move:north" });
								doneY = true;
								_log[_log.Count()] = "bottom edge";
							}
						}
					}
				}
			}

            // Add Commands Here.
            // You can only give as many commands as you have un-sunk vessels. Powerup commands do not count against this number. 
            // You are free to use as many powerup commands at any time. Any additional commands you give (past the number of active vessels) will be ignored.
			cmds.Add(new Command { vesselid = 1, action = "fire", coordinate = new Coordinate { X = 1, Y = 1 } });

            return cmds;
        }

        // Do NOT modify or remove! This is where you will receive the new board status after each round.
        public void GetBoardStatus(BoardStatus board)
        {
            _currentBoard = board;
        }

        // This method runs at the start of a new game, do any initialization or resetting here 
        public void Reset()
        {

        }
    }
}