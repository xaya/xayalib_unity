using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MoverStateCalculator
{

    /* Because we run this in different thread,
     * all errors are just fail in Unity Editor
     * silently, which is no good, we need to
     * provide error feedback and Debug.log
     * later on
     */
     
	public class CallbackFunctions
	{
    
		public static string initialCallbackResult()
		{
			return "";         
		}

		public static string forwardCallbackResult(string oldState, string blockData, string undoData)
		{
            GameState state = JsonConvert.DeserializeObject<GameState>(oldState);
            dynamic blockDataS = JsonConvert.DeserializeObject(blockData);
            Dictionary<string, PlayerUndo> undo = new Dictionary<string, PlayerUndo>();
                     
			if(blockData.Length <= 1)
			{
                return "" + "~" + undoData;
            }

            MoveGUIAndGameController.Instance.UpdateBlockSynch((int)blockDataS["block"]["height"]);

			if (state == null)
			{
				state = new GameState();
			}

			if(state.players == null)
			{
				state.players = new Dictionary<string, PlayerState>();
			}

            foreach (var m in blockDataS["moves"])
            {

                string name = m["name"].ToString();

				JObject obj = JsonConvert.DeserializeObject<JObject>(m["move"].ToString());

                Direction dir = Direction.UP;
                Int32 steps = 0;

                if(!HelperFunctions.ParseMove(ref obj, ref dir, ref steps))
                {
                    continue;
                }

                PlayerState p;
                bool isNotNew = state.players.ContainsKey(name);

                if (isNotNew)
                {
                    p = state.players[name];
                }
                else
                {
                    p = new PlayerState();
					state.players.Add(name, p);
                }

                PlayerUndo u = new PlayerUndo();
                undo.Add(name, u);
                if (!isNotNew)
                {
                    u.is_new = true;
                    p.x = 0;
                    p.y = 0;
                }
                else
                {
                    u.previous_dir = p.dir;
                    u.previous_steps_left = p.steps_left;
                }

                p.dir = dir;
                p.steps_left = steps;

                MoveGUIAndGameController.Instance.needsRedraw = true;
            }

            foreach (var mi in state.players)
            {
                string name = mi.Key;
                PlayerState p = mi.Value;


                if (p.dir == Direction.NONE)
                {
                    continue;
                }

                if (p.steps_left <= 0)
                {
                    continue;
                }

                Int32 dx = 0, dy = 0;
                HelperFunctions.GetDirectionOffset(p.dir, ref dx, ref dy);
                p.x += dx;
                p.y += dy;

                p.steps_left -= 1;

                if (p.steps_left == 0)
                {
                    PlayerUndo u;

                    if (undo.ContainsKey(name))
                    {
                        u = undo[name];
                    }
                    else
                    {
                        u = new PlayerUndo();
                        undo.Add(name, u);
                    }

                    u.finished_dir = p.dir;
                    p.dir = Direction.NONE;

                }

                MoveGUIAndGameController.Instance.needsRedraw = true;
            }
                    
			undoData = JsonConvert.SerializeObject(undo);

            //We can redraw our screen now, as we do it in main thread,
            //Lets just fill vars and let GUI to pick up
            MoveGUIAndGameController.Instance.state = state;

            //In c++, we explode results using '~' as delimeter
            //This is potential trouble if actual JSON data
            //has it, so later we need to rewrite this part
            //to properly marshall out strings
            return JsonConvert.SerializeObject(state) + "~" + undoData;
		}


		public static string backwardCallbackResult(string newState, string blockData, string undoData)
		{

			GameState state = JsonConvert.DeserializeObject<GameState>(newState);
			UndoData undo = JsonConvert.DeserializeObject<UndoData>(undoData);

			List<string> playersToRemove = new List<string>();

			foreach (var mi in state.players)
			{
				string name = mi.Key;
				PlayerState p = mi.Value;
				PlayerUndo u;

				bool undoIt = undo.players.ContainsKey(name);

                if(undoIt)
				{
					u = undo.players[name];

					if(u.is_new)
					{
						playersToRemove.Add(name);
						continue;
					}
				}

				if (undoIt)
				{
					u = undo.players[name];

					if (u.finished_dir != Direction.NONE)
                    {
                        
						if(p.dir == Direction.NONE && p.steps_left == 0)
						{
							p.dir = u.finished_dir;
						}

                    }					
				}

				if(p.dir != Direction.NONE)
				{
					p.steps_left += 1;
					Int32 dx = 0, dy = 0;
                    HelperFunctions.GetDirectionOffset(p.dir, ref dx, ref dy);
                    p.x -= dx;
                    p.y -= dy;
				}

				if (undoIt)
				{
					u = undo.players[name];

					if(u.previous_dir != Direction.NONE)
					{
						p.dir = u.previous_dir;
					}

					if(u.previous_steps_left != 99999999)
					{
						p.steps_left = u.previous_steps_left;
					}
				}

				foreach(string nm in playersToRemove)
				{
					state.players.Remove(nm);
				}

                //We can redraw our screen now, as we do it in main thread,
                //Lets just fill vars and let GUI to pick up
                MoveGUIAndGameController.Instance.state = state;
                MoveGUIAndGameController.Instance.needsRedraw = true;

                return JsonConvert.SerializeObject(state);
			}
 
			return "";
		}
	}
}
