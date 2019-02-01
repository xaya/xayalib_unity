using System.Collections;
using UnityEngine;
using CielaSpike;
using System.IO;
using System;
using Newtonsoft.Json;
using BitcoinLib.Responses;
using XAYAWrapper;
using XAYAMoverGame;

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
        
        // Clean up the log files from the last session. This is from glog.
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

    // We run the daemon as coroutine on a seperate thread, 
    // because it will block the Unity main thread completely
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

        wrapper = new XayaWrapper(dPath, MoveGUIAndGameController.Instance.host_s, MoveGUIAndGameController.Instance.gamehostport_s, ref functionResult, CallbackFunctions.initialCallbackResult, CallbackFunctions.forwardCallbackResult,CallbackFunctions.backwardCallbackResult);

        yield return Ninja.JumpToUnity;
        Debug.Log(functionResult);
        yield return Ninja.JumpBack;

        functionResult = wrapper.Connect(dPath, FLAGS_xaya_rpc_url, MoveGUIAndGameController.Instance.gamehostport_s, MoveGUIAndGameController.Instance.chain_s.ToString(), MoveGUIAndGameController.Instance.GetStorageString(MoveGUIAndGameController.Instance.storage_s), "mv", dPath + "\\..\\XayaStateProcessor\\database\\", dPath + "\\..\\XayaStateProcessor\\glogs\\" );

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
                            Debug.LogError("Returned state is not valid? We had a JSON error.");
                        }

                    yield return null;
                }
                else
                {
                    Debug.LogError("actualState is null.");
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
        // We need to run this one on seperate thread because waitforchange will block all input
        // We are using Ciela Spike's Thread Ninja here for threading help.
        Task task;
        this.StartCoroutineAsync(WaitForChangesInner(), out task);
        yield return StartCoroutine(task.Wait());
    }

    // Issuing the Stop command might fail, so we run it in a loop to make sure it succeedes 
    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        StartCoroutine(TryAndStop());
        Instance = null;
    }

    // We check the glog files to see what the problem was and then we output the error to the screen. 
    // No error handling in here.
    void WaitForFileAndCheck()
    {
        // We extract the info we need from the glog files.
        string[] files = Directory.GetFiles(dPath + "\\..\\XayaStateProcessor\\glogs\\");
        for (int s = 0; s < files.Length; s++)
        {
            // Glog might keep access control, so we must check using shared access.
            if (files[s].Contains("FATAL"))
            {
                using (var fileStream = new FileStream(files[s], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    var content = textReader.ReadToEnd();
                    string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    MoveGUIAndGameController.Instance.ShowError(lines[3]); // This sends the error message to the Unity error message box.
                    Debug.Log(lines[3]); // Glog seems to keep the error we need right here at '3'. 
                }
            }

        }
    }

    public void CheckIfFatalError()
    {
        // We must do it this way, as we need to check on the Main Unity thread.
        // On the next update cycle, the Main thread will pick up the check we need.
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
