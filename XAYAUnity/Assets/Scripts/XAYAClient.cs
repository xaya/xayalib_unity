using UnityEngine;
using BitcoinLib.Services.Coins.XAYA;
using System.Collections.Generic;
using BitcoinLib.Responses;
using System.Collections;

public class XAYAClient : MonoBehaviour 
{
    // We could send RPC calls via libxayagame.However, we're presenting this class here 
    // because the Game State Processor does not need to be integrated into Unity at all, 
    // and when it isn't, we need an in-house RPC implementation, e.g. this XAYAClient class.

    public bool connected = false;

    public XAYAConnector connector;

    [HideInInspector]
    public IXAYAService xayaService;

    public bool Connect()
    {
        xayaService = new XAYAService(MoveGUIAndGameController.Instance.host_s + ":" + MoveGUIAndGameController.Instance.hostport_s + "/wallet/game.dat", MoveGUIAndGameController.Instance.rpcuser_s, MoveGUIAndGameController.Instance.rpcpassword_s, "", 10);
    
        if (xayaService.GetConnectionCount() > 0)
        {
            // We are not tracking connection drops or anything
            // here for the same of simplicity.We just assume
            // that once we are connected, then we are always fine.

           connector.SubscribeForBlockUpdates();

           connected = true;
           return true;
        }
        else
        {
            Debug.LogError("Failed to connect with XAYAService.");
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

        // We are not doing any error checking here for the sake of simplicity.
        // We just assume that all goes well.

       List < GetNameListResponse> nList = xayaService.GetNameList();

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
