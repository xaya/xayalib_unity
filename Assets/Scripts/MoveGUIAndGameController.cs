﻿using MoverStateCalculator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveGUIAndGameController : MonoBehaviour {


    public RectTransform gameCanvas;
    public RectTransform sizeFitterHolder;
    public RectTransform bgImage;

    public GameObject moverObjectPrefab;
    public GameObject settings;
    public GameObject errorMessage;

    public Text errorText;
    public Text btnLaunchText;
    public Text btnMoveText;
    public Text blockSynchCount;

    public XAYAClient xayaClient;
    public XAYAConnector xayaConnector;

    public InputField host;
    public InputField hostport;
    public InputField tcpport;
    public InputField rpcuser;
    public InputField rpcpassword;

    public Dropdown storage;
    public Dropdown chain;
    public Dropdown playernamelist;
    public Dropdown movedir;
    public Dropdown moveditance;

    [HideInInspector]
    public string host_s;
    [HideInInspector]
    public string hostport_s;
    [HideInInspector]
    public string tcpport_s;
    [HideInInspector]
    public string rpcuser_s;
    [HideInInspector]
    public string rpcpassword_s;
    [HideInInspector]
    public int storage_s;
    [HideInInspector]
    public int chain_s;

    [HideInInspector]
    public GameState state;
    [HideInInspector]
    public bool needsRedraw = false;

    public static MoveGUIAndGameController Instance;

    private string nameSelected = "";
    private string directionSelected = "r";
    private string distanceSelected = "1";
    private int totalBlock = 0;
    private bool needToConnectClient = false;
    private bool updateTotalBlockCount = false;
    private bool updateSynchCounter = false;
    private int _sVal = 0;
    private int _sPVal = 0;
    List<string> nameList;
    bool redrawNeverHappened = true; //when using database to store, not memory, need to redraw on first tick

    List<GameObject> moverObjects = new List<GameObject>();

    // Use this for initialization
    void Start ()
    {
        Instance = this;

        FillSettingsFromPlayerPrefs();

        host.text = host_s;
        hostport.text = hostport_s;
        tcpport.text = tcpport_s;
        rpcuser.text = rpcuser_s;
        rpcpassword.text = rpcpassword_s;

        storage.value = storage_s;
        chain.value = chain_s;
    }

    void RedrawGameClient()
    {
        Debug.Log("Redrawing GUI");

        for(int s = 0; s < moverObjects.Count;s++)
        {
            Destroy(moverObjects[s]);
        }

        moverObjects.Clear();

        float topY = -800;
        float bottomY = 800;
        float leftX = -800;
        float rightX = 800;
        float pWidth = 40; //According to prefab image size
        foreach (KeyValuePair<string, PlayerState> pDic in state.players)
        {
            GameObject player = GameObject.Instantiate<GameObject>(moverObjectPrefab);
            MoverObject pComponent = player.GetComponent<MoverObject>();
            pComponent.moverName.text = pDic.Key;

            moverObjects.Add(player);
            player.transform.SetParent(gameCanvas);
            player.transform.localScale = Vector3.one;
            player.GetComponent<RectTransform>().anchoredPosition = new Vector3(pWidth * pDic.Value.x, pWidth * pDic.Value.y, 0);


            //Those infinite canvas size calculations I did in a hurry
            //And they need to be redone properly later if
            //want it to have all pretty

            if(topY < pDic.Value.y * pWidth)
            {
                topY = pDic.Value.y * pWidth;
            }

            if (bottomY > pDic.Value.y * pWidth)
            {
                bottomY = pDic.Value.y * pWidth;
            }

            if (leftX < pDic.Value.x * pWidth)
            {
                leftX = pDic.Value.x * pWidth;
            }

            if (rightX > pDic.Value.x * pWidth)
            {
                rightX = pDic.Value.x * pWidth;
            }
        }

        float canvaHeight = (Math.Abs(topY) + bottomY) * 3;
        float canvaWidth = (Math.Abs(leftX) + rightX) * 3;

        if (canvaHeight < 800)
        {
            canvaHeight = 800;
        }
        if (canvaWidth < 800)
        {
            canvaWidth = 800;
        }

        sizeFitterHolder.sizeDelta = new Vector2(canvaWidth, canvaHeight);
        bgImage.sizeDelta = new Vector2(canvaWidth, canvaHeight);

    }

    void FillSettingsFromPlayerPrefs()
    {
        host_s = PlayerPrefs.GetString("host", "http://127.0.0.1");
        hostport_s = PlayerPrefs.GetString("hostport", "8396");
        tcpport_s = PlayerPrefs.GetString("tcpport", "8900");
        rpcuser_s = PlayerPrefs.GetString("rpcuser", "xayagametest");
        rpcpassword_s = PlayerPrefs.GetString("rpcpassword", "xayagametest");
        storage_s = PlayerPrefs.GetInt("storage", 0);
        chain_s = PlayerPrefs.GetInt("chain", 0);
    }

    public void ShowError(string txt)
    {
        errorText.text = txt;
        errorMessage.SetActive(true);
    }

    public void OnButton_Exit()
    {
        Application.Quit();
    }

    public void OnButton_ErrorClose()
    {
        errorMessage.SetActive(false);
    }

    public void OnButton_SettingsSave()
    {
        PlayerPrefs.SetString("host",host.text);
        PlayerPrefs.SetString("hostport", hostport.text);
        PlayerPrefs.SetString("tcpport", tcpport.text);
        PlayerPrefs.SetString("rpcuser", rpcuser.text);
        PlayerPrefs.SetString("rpcpassword", rpcpassword.text);
        PlayerPrefs.SetInt("storage", storage.value);
        PlayerPrefs.SetInt("chain", chain.value);
        PlayerPrefs.Save();

        FillSettingsFromPlayerPrefs();
    }

    public void OnButton_DaemonLaunch()
    {
        /* This is simplified example;
         * Ideally we need additional
         * checks to make sure we are
         * not in middle of launching
         * or stopping when calling 
         * those
         */

        if (btnLaunchText.text != "STOP")
        {
            xayaConnector.LaunchMoverStateProcessor();
            btnLaunchText.text = "STOP";
        }
        else
        {   
            xayaConnector.Disconnect();
        }
    }


    public void UpdateBlockSynch(int curVal)
    {

        if(totalBlock == 0 && !xayaClient.connected)
        {
            //Lets grab client data
            //We have to run this on main thread;
            needToConnectClient = true;
        }

        if(curVal > totalBlock)
        {
            //We have to run this on main thread;
            updateTotalBlockCount = true;
            
        }

        updateSynchCounter = true;
        _sVal = curVal;
    }

    public void LaunchAborted()
    {
        btnLaunchText.text = "Launch";
    }

    public void OnDistanceValueChanged(int newval)
    {
        distanceSelected = moveditance.options[newval].text;
    }

    public void OnDirectionValueChanged(int newval)
    {
        directionSelected = movedir.options[newval].text;
    }

    public void OnPlayerNameSelected(int newval)
    {
        nameSelected = nameList[newval];
    }


    public void FillNameList()
    {
        nameList = xayaClient.GetNameList();

        playernamelist.ClearOptions();
        playernamelist.AddOptions(nameList);

        if(nameList.Count > 0 && nameSelected.Length <= 1)
        {
            nameSelected = nameList[0];
        }
    }

    public string GetStorageString(int storagenum)
    {
        if (storagenum == 0) return "memory";
        if (storagenum == 1) return "sqlite";
        if (storagenum == 2) return "lmdb";

        return "";
    }

    public string DirectionDropdownToMoverDir(string direction)
    {
        if (direction == "RIGHT") return "l";
        if (direction == "LEFT") return "h";
        if (direction == "UP") return "k";
        if (direction == "DOWN") return "j";
        if (direction == "RIGHT_UP") return "u";
        if (direction == "LEFT_UP") return "y";
        if (direction == "LEFT_DOWN") return "b";
        if (direction == "RIGHT_DOWN") return "n";

        return "";
    }

    public bool ConnectClient()
    {
        if (!xayaClient.connected)
        {
            try
            {
                if (xayaClient.Connect())
                {
                    btnMoveText.text = "MOVE!";
                    FillNameList();
                    return true;
                }
            }
            catch (Exception e)
            {
                ShowError(e.ToString());
            }
        }
        else
        {
            if (nameSelected.Length > 1)
            {
                ShowError(xayaClient.ExecuteMove(nameSelected, DirectionDropdownToMoverDir(directionSelected), distanceSelected));
            }
            else
            {
                ShowError("No name selected");
            }
        }

        return false;
    }

    void Update()
    {
        if(needToConnectClient && !xayaClient.connected)
        {
            if(ConnectClient())
            {
                needToConnectClient = false;
            }
        }
        if(updateTotalBlockCount)
        {
            updateTotalBlockCount = false;
            totalBlock = xayaClient.GetTotalBlockCount();
        }

        if(updateSynchCounter)
        {
            updateSynchCounter = false;
            _sPVal = 0;

            if(state != null)
            {
                if(state.players != null)
                {
                    _sPVal = state.players.Count;
                }
            }

            if(redrawNeverHappened)
            {
                redrawNeverHappened = false;
                needsRedraw = true;
            }

            blockSynchCount.text = _sVal + "/" + totalBlock + "(Total players on the map: " + _sPVal + ")";
        }

        if(needsRedraw)
        {
            needsRedraw = false;
            RedrawGameClient();
        }
    }

    public void OnButtonPlayer_Move()
    {
        ConnectClient();
    }
	
}
