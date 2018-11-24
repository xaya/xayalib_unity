﻿using System.Collections;
using UnityEngine;
using CielaSpike;
using System.IO;
using System;

namespace MoverStateCalculator
{

    public class XAYAConnector : MonoBehaviour
    {

        string dPath = "";
        string FLAGS_xaya_rpc_url = "";
        bool fatalCheckPending = false;
        public XayaWrapper wrapper;

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
        }

        IEnumerator DaemonAsync()
        {
            //We keep DDLs inside Asset folder for Editor,
            //hence here we resolve the path for it
            wrapper = new XayaWrapper(dPath, this);
            wrapper.Connect(dPath, FLAGS_xaya_rpc_url);
            yield return new WaitForSeconds(0.1f);
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


        void WaitForFileAndCheck()
        {
            /* LETS EXTRAT TH INFO WE NEED FROM GLOG FILES*/
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
                        MoveGUIAndGameController.Instance.ShowError(lines[3]);
                        Debug.Log(lines[3]); // Wild guess, but seems like glog will keep error we ned right there always SO FAR
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
            fatalCheckPending = true;
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
}