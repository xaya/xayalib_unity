using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace MoverStateCalculator
{

    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        public static extern bool SetDllDirectory(string pathName);

        [DllImport("kernel32.dll")]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
    }


    enum ChainType
    {
        MAIN,
        TEST,
        REGTEST
    }


    public class XayaWrapper
    {


        private delegate string InitialCallback();
        private delegate string ForwardCallback(string oldState, string blockData, string undoData);
        private delegate string BackwardCallback(string newState, string blockData, string undoData);

        InitialCallback initialCallback;
        ForwardCallback forwardCallback;
        BackwardCallback backwardsCallback;

        IntPtr pDll;
        XAYAConnector parent;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SignalStop();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void setInitialCallback(InitialCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void setForwardCallback(ForwardCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void setBackwardCallback(BackwardCallback callback);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CSharp_ConnectToTheDaemon(string gameId, string XayaRpcUrl, int GameRpcPort, int EnablePruning, int chain, string storageType, string dataDirectory, string glogName, string glogDataDir);

        public XayaWrapper(string dataPath, XAYAConnector _parent)
        {
            parent = _parent;

            if(!NativeMethods.SetDllDirectory(dataPath + "\\..\\XayaStateProcessor\\"))
            {
                Debug.Log("Could not set dll directory");
                return;
            }

            pDll = NativeMethods.LoadLibrary(dataPath.Replace("/","\\") + "\\..\\XayaStateProcessor\\libxayawrap.dll");
            

            if (pDll == IntPtr.Zero)
            {
                Debug.Log("Could not load " + dataPath.Replace("/", "\\") + "\\..\\XayaStateProcessor\\libxayawrap.dll" + pDll.ToString());
                return;
            }

            IntPtr pSetInitialCallback = NativeMethods.GetProcAddress(pDll, "setInitialCallback");   
            if(pSetInitialCallback == IntPtr.Zero)
            {
                Debug.Log("Could not load resolve setInitialCallback");
                return;
            }

            IntPtr pSetForwardCallback = NativeMethods.GetProcAddress(pDll, "setForwardCallback");
            if (pSetForwardCallback == IntPtr.Zero)
            {
                Debug.Log("Could not load resolve pSetForwardCallback");
                return;
            }

            IntPtr pSetBackwardCallback = NativeMethods.GetProcAddress(pDll, "setBackwardCallback");
            if (pSetBackwardCallback == IntPtr.Zero)
            {
                Debug.Log("Could not load resolve pSetBackwardCallback");
                return;
            }

            setInitialCallback SetInitialCallback = (setInitialCallback)Marshal.GetDelegateForFunctionPointer(pSetInitialCallback, typeof(setInitialCallback));
            setForwardCallback SetForwardlCallback = (setForwardCallback)Marshal.GetDelegateForFunctionPointer(pSetForwardCallback, typeof(setForwardCallback));
            setBackwardCallback SetBackwardCallback = (setBackwardCallback)Marshal.GetDelegateForFunctionPointer(pSetBackwardCallback, typeof(setBackwardCallback));

            initialCallback = new InitialCallback(CallbackFunctions.initialCallbackResult);
            SetInitialCallback(initialCallback);

            forwardCallback = new ForwardCallback(CallbackFunctions.forwardCallbackResult);
            SetForwardlCallback(forwardCallback);

            backwardsCallback = new BackwardCallback(CallbackFunctions.backwardCallbackResult);
            SetBackwardCallback(backwardsCallback);

        }

        public void Connect(string dataPath, string FLAGS_xaya_rpc_url)
        {
            IntPtr pDaemonConnect = NativeMethods.GetProcAddress(pDll, "CSharp_ConnectToTheDaemon");
            if (pDaemonConnect == IntPtr.Zero)
            {
                Debug.Log("Could not load resolve CSharp_ConnectToTheDaemon");
                return;
            }

            CSharp_ConnectToTheDaemon ConnectToTheDaemon_CSharp = (CSharp_ConnectToTheDaemon)Marshal.GetDelegateForFunctionPointer(pDaemonConnect, typeof(CSharp_ConnectToTheDaemon));

            //Storage type can be: "memory", or "lmdb", or "sqlite"
            //For types other them memory dataDirectory needs to be set

            if (!Directory.Exists(dataPath + "\\..\\XayaStateProcessor\\glogs\\"))
            {
                Directory.CreateDirectory(dataPath + "\\..\\XayaStateProcessor\\glogs\\");
            }

            try
            {
                FLAGS_xaya_rpc_url = FLAGS_xaya_rpc_url.Replace("http://", ""); // not sure why, but curl in xayalib dislikes http prefix
                ConnectToTheDaemon_CSharp("mv", FLAGS_xaya_rpc_url, int.Parse(MoveGUIAndGameController.Instance.tcpport_s), -1, MoveGUIAndGameController.Instance.chain_s, MoveGUIAndGameController.Instance.GetStorageString(MoveGUIAndGameController.Instance.storage_s), dataPath + "\\..\\XayaStateProcessor\\database\\", "XayaGLOG", dataPath + "\\..\\XayaStateProcessor\\glogs\\");
            }
            catch (ThreadAbortException)
            {

                Thread.ResetAbort();
            }

            //This we be blocked by dll until "stop" command is issued
            ShutdownDaemon();
            parent.CheckIfFatalError();
        }

        public void Stop()
        {
            IntPtr pSignalStop = NativeMethods.GetProcAddress(pDll, "SignalStop");
            if (pSignalStop == IntPtr.Zero)
            {
                Debug.Log("Could not load resolve pSignalStop");
                return;
            }

            SignalStop fSIgnalStop = (SignalStop)Marshal.GetDelegateForFunctionPointer(pSignalStop, typeof(SignalStop));
            fSIgnalStop();
        }


        public void ShutdownDaemon()
        {
            if (pDll != IntPtr.Zero)
            {
                Debug.Log("Freeing library...");
                initialCallback = null;
                forwardCallback = null;
                backwardsCallback = null;
                NativeMethods.FreeLibrary(pDll);
                pDll = IntPtr.Zero;
            }
            
            Debug.Log("Done freeing!");
            MoveGUIAndGameController.Instance.xayaConnector.wrapper = null;
        }

    }
}
