using System.Collections;
using UnityEngine;
using CielaSpike;
using System.IO;
using System;
using Newtonsoft.Json;
using BitcoinLib.Responses;
using XAYAMoverGame;
using XAYAWrapper;

public class XAYAConnector : MonoBehaviour
{

    string dPath = "";
    string FLAGS_xaya_rpc_url = "";
    bool fatalCheckPending = false;
    public XayaWrapper wrapper;
    public XAYAClient client;

    public static XAYAConnector Instance;

    public void LaunchMoverStateProcessor()
    {

        Instance = this;
        dPath = Application.dataPath;
        FLAGS_xaya_rpc_url = MoveGUIAndGameController.Instance.rpcuser_s + ":" + MoveGUIAndGameController.Instance.rpcpassword_s + "@" + MoveGUIAndGameController.Instance.host_s + ":" + MoveGUIAndGameController.Instance.hostport_s;
        //Clean last session logs
        if (Directory.Exists(dPath + "\\..\\XayaStateProcessor\\glogs\\"))
        {
            DirectoryInfo di = new DirectoryInfo(dPath + "\\..\\XayaStateProcessor\\glogs\\");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        StartCoroutine(StartEnum());
    }

    /* We run daemon as coroutine on seperate thread, 
     * because else it will block the Unity main thread 
     * completely */
    IEnumerator StartEnum()
    {
        Task task;
        this.StartCoroutineAsync(DaemonAsync(), out task);
        yield return StartCoroutine(task.Wait());

        if (task.State == TaskState.Error)
        {
            MoveGUIAndGameController.Instance.ShowError(task.Exception.ToString());
            Debug.LogError(task.Exception.ToString());
        }

    }

    IEnumerator DaemonAsync()
    {
        string functionResult = "";

        wrapper = new XayaWrapper(dPath + "\\..\\XayaStateProcessor\\", MoveGUIAndGameController.Instance.host_s, MoveGUIAndGameController.Instance.gamehostport_s, ref functionResult, CallbackFunctions.initialCallbackResult, CallbackFunctions.forwardCallbackResult, CallbackFunctions.backwardCallbackResult);

        yield return Ninja.JumpToUnity;
        Debug.Log(functionResult);
        yield return Ninja.JumpBack;

        functionResult = wrapper.Connect(FLAGS_xaya_rpc_url, MoveGUIAndGameController.Instance.gamehostport_s, MoveGUIAndGameController.Instance.chain_s.ToString(), MoveGUIAndGameController.Instance.GetStorageString(MoveGUIAndGameController.Instance.storage_s), "mv", dPath + "\\..\\XayaStateProcessor\\database\\", dPath + "\\..\\XayaStateProcessor\\glogs\\");

        yield return Ninja.JumpToUnity;
        Debug.Log(functionResult);
        yield return Ninja.JumpBack;

        Debug.Log("Check if fatal?");

        CheckIfFatalError();

    }

    public void SubscribeForBlockUpdates()
    {
        StartCoroutine(WaitForChanges());
    }

    IEnumerator WaitForChangesInner()
    {
        while (true)
        {
            if (client.connected && wrapper != null)
            {

                wrapper.xayaGameService.WaitForChange();

                GameStateResult actualState = wrapper.xayaGameService.GetCurrentState();

                if (actualState != null)
                {
                    if (actualState.gamestate != null)
                    {
                        GameState state = JsonConvert.DeserializeObject<GameState>(actualState.gamestate);

                        MoveGUIAndGameController.Instance.state = state;
                        MoveGUIAndGameController.Instance.totalBlock = client.GetTotalBlockCount();
                        var currentBlock = client.xayaService.GetBlock(actualState.blockhash);
                        MoveGUIAndGameController.Instance._sVal = currentBlock.Height;

                        MoveGUIAndGameController.Instance.needsRedraw = true;
                    }
                    else
                    {
                        Debug.LogError("Retuned state is not valid? We had some error with JSON");
                    }

                    yield return null;
                }
                else
                {
                    Debug.LogError("actualState is null");
                }

            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator WaitForChanges()
    {
        /*We need to run this one on seperate thread,
         else waitforchange will block all the input*/
        // using Ninja here for threading help
        Task task;
        this.StartCoroutineAsync(WaitForChangesInner(), out task);
        yield return StartCoroutine(task.Wait());
    }

    /* For some reason, issueing stop command might fail, 
     * so we ar running it in the loop to make sure it 
     * succeedes */
    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        StartCoroutine(TryAndStop());
        Instance = null;
    }

    // We check the glog files to see what the problem was and then we output the error to screen. No error handling in here.
    void WaitForFileAndCheck()
    {
        /* LETS EXTRACT TH INFO WE NEED FROM GLOG FILES*/
        string[] files = Directory.GetFiles(dPath + "\\..\\XayaStateProcessor\\glogs\\");
        for (int s = 0; s < files.Length; s++)
        {
            /* Glog might keep access control,
            *  so we must check using shared
            * access */

            if (files[s].Contains("FATAL"))
            {
                using (var fileStream = new FileStream(files[s], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    var content = textReader.ReadToEnd();
                    string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    MoveGUIAndGameController.Instance.ShowError(lines[3]); // This sends the error message to the Unity error message box.
                    Debug.Log(lines[3]); // Wild guess, but seems like glog will keep error we ned right there always SO FAR // This goes to the Unity console.
                }
            }

        }
    }

    public void CheckIfFatalError()
    {
        //We must to it this way,
        //as we need to check on
        //main Unity thread
        //On the next update cycle
        //Main thread will pick up
        //the check we need
        fatalCheckPending = true; // 
    }

    private void Update()
    {
        if (fatalCheckPending)
        {
            fatalCheckPending = false;
            MoveGUIAndGameController.Instance.LaunchAborted();
            WaitForFileAndCheck();
        }
    }

    IEnumerator TryAndStop()
    {
        if (wrapper != null)
        {
            wrapper.Stop();
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(TryAndStop());
        }
    }

}
