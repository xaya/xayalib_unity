using UnityEngine;
using BitcoinLib.Services.Coins.Bitcoin;
using System.Collections.Generic;
using BitcoinLib.Responses;
using MoverStateCalculator;
using System.Collections;

public class XAYAClient : MonoBehaviour 
{
    /* We can send RPC calls via
     * libxayagame, but I made
     * this library because Game
     * State Processor does not
     * need to be integrated into
     * Unity at all, and when it is
     * not, we need some in-house
     * RPC implementation, which this
     * class actually represents
     */ 

    public bool connected = false;

    public XAYAConnector connector;

    [HideInInspector]
    public IXAYAService xayaService;

    public bool Connect()
    {
       
        xayaService = new XAYAService(MoveGUIAndGameController.Instance.host_s + ":" + MoveGUIAndGameController.Instance.hostport_s + "/wallet/game.dat", MoveGUIAndGameController.Instance.rpcuser_s, MoveGUIAndGameController.Instance.rpcpassword_s, "", 10);
    
        if (xayaService.GetConnectionCount() > 0)
        {
            /* We are not tracking connection drop or anything
             * here for the same of simplicity, we just assume
             * that once we are connect, then we are always fine
             */

            connector.SubscribeForBlockUpdates();

            connected = true;
            return true;
        }
        else
        {
            Debug.LogError("Failed to connect with XAYAService");
        }
        return false;
    }

    public int GetTotalBlockCount()
    {
        if (xayaService == null) return 0;
        return (int)xayaService.GetBlockCount();
    }

    public List<string> GetNameList()
    {
        List<string> allMyNames = new List<string>();

        /* We are not doing error checking here
         * for simplicity assuming that all
         * goes fine
         */

        List<GetNameListResponse> nList = xayaService.GetNameList();

        foreach(var nname in nList)
        {
            if (nname.ismine == true)
            {
                allMyNames.Add(nname.name);
            }
        }

        return allMyNames;
    }

    public string ExecuteMove(string playername, string direction, string distance)
	{   
         return xayaService.NameUpdate(playername, "{\"g\":{\"mv\":{\"d\":\"" + direction + "\",\"n\":" + distance + "}}}", new object()); 		
	}
}
