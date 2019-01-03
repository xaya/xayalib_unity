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
    
		public static string initialCallbackResult(out int height, out string hashHex)
		{

            if(MoveGUIAndGameController.Instance.chain_s == 0)
            {
                height = 125000;
                hashHex = "2aed5640a3be8a2f32cdea68c3d72d7196a7efbfe2cbace34435a3eef97561f2";
            }
            else if (MoveGUIAndGameController.Instance.chain_s == 1)
            {
                height = 10000;
                hashHex = "73d771be03c37872bc8ccd92b8acb8d7aa3ac0323195006fb3d3476784981a37";
            }
            else
            {
                height = 0;
                hashHex = "6f750b36d22f1dc3d0a6e483af45301022646dfc3b3ba2187865f5a7d6d83ab1";
            }

                return "";         
		}

		public static string forwardCallbackResult(string oldState, string blockData, string undoData, out string newData)
		{
            GameState state = JsonConvert.DeserializeObject<GameState>(oldState);
            dynamic blockDataS = JsonConvert.DeserializeObject(blockData);
            Dictionary<string, PlayerUndo> undo = new Dictionary<string, PlayerUndo>();
                     
			if(blockData.Length <= 1)
			{
                newData = "";
                return "";
            }

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
            }
                    
			undoData = JsonConvert.SerializeObject(undo);
            newData = JsonConvert.SerializeObject(state);
            return undoData;
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
			}

            foreach (string nm in playersToRemove)
            {
                state.players.Remove(nm);
            }

            return JsonConvert.SerializeObject(state);

        }
	}
}
