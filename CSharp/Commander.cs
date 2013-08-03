using System.Web.UI;
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
		private List<Command> _cmds; 

        public Commander()
        {
            _currentBoard = new BoardStatus();
        }

        // Do not alter/remove this method signature
        public List<Command> GiveCommands()
        {
           
			//comment

			foreach (var vesselStatus in _currentBoard.MyVesselStatuses)
			{
				_myVessels.Add(vesselStatus);
			}

			ReorderVessels();

			foreach (var vessel in _myVessels)
			{
				if (!vessel.CounterMeasuresLoaded && vessel.CounterMeasures > 0)
				{
					_cmds.Add(new Command { vesselid = vessel.Id, action = "load_countermeasures" });
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
								_cmds.Add(new Command { vesselid = vessel.Id, action = "move:east" });
								doneX = true;
							}
							if (location.X == _currentBoard.BoardMaxCoordinate.X)
							{
								_cmds.Add(new Command { vesselid = vessel.Id, action = "move:west" });
								doneX = true;
							}
						}
						if (!doneY)
						{
							if (location.Y == _currentBoard.BoardMinCoordinate.Y)
							{
								_cmds.Add(new Command { vesselid = vessel.Id, action = "move:south" });
								doneY = true;
							}
							if (location.Y == _currentBoard.BoardMaxCoordinate.Y)
							{
								_cmds.Add(new Command { vesselid = vessel.Id, action = "move:north" });
								doneY = true;
							}
						}
					}
				}
			}

			ParseSonarReport();
	        foreach (var item in _listObjects)
	        {
		        if (item.Count() > 1)
		        {
			        Shoot(item[0]);
		        }
	        }
	        foreach (var vessel in _myVessels)
	        {
		        var onBottomEdge = false;
		        foreach (var location in vessel.Location)
		        {
			        if (location.Y == _currentBoard.BoardMaxCoordinate.Y)
						onBottomEdge = true;
		        }
				if (!onBottomEdge)
					_cmds.Add(new Command { vesselid = vessel.Id, action = "move:north" });
				else
				{
					_cmds.Add(new Command { vesselid = vessel.Id, action = "move:south" });
				}
	        }

            // Add Commands Here.
            // You can only give as many commands as you have un-sunk vessels. Powerup commands do not count against this number. 
            // You are free to use as many powerup commands at any time. Any additional commands you give (past the number of active vessels) will be ignored.
			_cmds.Add(new Command { vesselid = 1, action = "fire", coordinate = new Coordinate { X = 1, Y = 1 } });

            return _cmds;
        }

	    private void Shoot(Coordinate item)
	    {
		    foreach (var vessel in _myVessels)
		    {
			    if(withinRange(vessel, item))
					_cmds.Add(new Command { vesselid = vessel.Id, action = "fire", coordinate = item });
		    }
	    }

	    private bool withinRange(VesselStatus vessel, Coordinate item)
	    {
		    foreach (var location in vessel.Location)
		    {
			    var minX = location.X - vessel.MissileRange;
				var maxX = location.X + vessel.MissileRange;
				var minY = location.Y - vessel.MissileRange;
				var maxY = location.Y + vessel.MissileRange;
				if (item.Y > minY && item.Y < maxY && item.X > minX && item.X < maxX)
				//if (Math.Abs(item.X - location.X) <= vessel.MissileRange || Math.Abs(item.Y - location.Y) <= vessel.MissileRange)
			    {
				    return true;
			    }

		    }

		    return false;
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

		private void ReorderVessels()
		{
			var reorderedVesselList = new List<VesselStatus>();
			reorderedVesselList.Add(_myVessels.Find(v => v.Size == 4));
			reorderedVesselList.Add(_myVessels.Find(v => v.Size == 3));
			reorderedVesselList.Add(_myVessels.Find(v => v.Size == 5));

			_myVessels = reorderedVesselList;
		}

		private void ParseSonarReport()
		{
			_SonarReport = new List<Coordinate>();
			foreach (var vessel in _myVessels)
			{
				foreach (var sonarCoordinate in vessel.SonarReport)
				{
					if (_SonarReport.IndexOf(sonarCoordinate) > -1)
						_SonarReport.Add(sonarCoordinate);
				}
			}
			_SonarReport.Sort();
			RemoveSelfFromSonar();
		}

		private void RemoveSelfFromSonar()
		{
			foreach (var vessel in _myVessels)
			{
				foreach (var location in vessel.Location)
				{
					if (_SonarReport.IndexOf(location) > -1)
						_SonarReport.Remove(location);
				}
			}
			SortSonarCoordinates();
		}

		private void SortSonarCoordinates()
		{
			_listObjects = new List<List<Coordinate>>();
			_listObjects.Add(new List<Coordinate>());
			_listObjects[0].Add(_SonarReport[0]);
			foreach (var coord in _SonarReport)
			{
				var grouped = false;
				if (!grouped)
				{
					foreach (var obj in _listObjects)
					{
						if (isAdjacent(obj, coord))
						{
							obj.Add(coord);
							grouped = true;
						}
					}
				}
			}
		}

		private bool isAdjacent(List<Coordinate> obj, Coordinate coord)
		{
			foreach (var thing in obj)
			{
				if (thing.X == (coord.X + 1) || thing.X == (coord.X - 1) || thing.Y == (coord.Y + 1) || thing.Y == (coord.Y - 1))
					return true;
			}
			return false;
		}
    }
}