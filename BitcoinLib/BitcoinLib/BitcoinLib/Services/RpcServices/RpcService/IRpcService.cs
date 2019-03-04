// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System.Collections.Generic;
using BitcoinLib.Requests.AddNode;
using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Requests.SignRawTransaction;
using BitcoinLib.Responses;

namespace BitcoinLib.Services.RpcServices.RpcService
{
    public interface IRpcService
    {
        #region Blockchain

        string GetBestBlockHash();
        GetBlockResponse GetBlock(string hash, bool verbose = true);
        GetBlockchainInfoResponse GetBlockchainInfo();
        uint GetBlockCount();
        string GetBlockHash(long index);
        //  getblockheader
        //  getchaintips
        double GetDifficulty();
        List<GetChainTipsResponse> GetChainTips();
        GetMemPoolInfoResponse GetMemPoolInfo();
        GetRawMemPoolResponse GetRawMemPool(bool verbose = false);
        GetTransactionResponse GetTxOut(string txId, int n, bool includeMemPool = true);
        //  gettxoutproof["txid",...] ( blockhash )
        GetTxOutSetInfoResponse GetTxOutSetInfo();
        bool VerifyChain(ushort checkLevel = 3, uint numBlocks = 288); //  Note: numBlocks: 0 => ALL

        #endregion

        #region Control

        GetInfoResponse GetInfo();
        string Help(string command = null);
        string Stop();

        #endregion

        #region Generating

        //  generate numblocks
        bool GetGenerate();
        string SetGenerate(bool generate, short generatingProcessorsLimit);

        #endregion

        #region Mining

        GetBlockTemplateResponse GetBlockTemplate(params object[] parameters);
        GetMiningInfoResponse GetMiningInfo();
        ulong GetNetworkHashPs(uint blocks = 120, long height = -1);
        bool PrioritiseTransaction(string txId, decimal priorityDelta, decimal feeDelta);
        string SubmitBlock(string hexData, params object[] parameters);

        #endregion

        #region Network

        void AddNode(string node, NodeAction action);
        //  clearbanned
        //  disconnectnode
        GetAddedNodeInfoResponse GetAddedNodeInfo(string dns, string node = null);
        int GetConnectionCount();
        GetNetTotalsResponse GetNetTotals();
        GetNetworkInfoResponse GetNetworkInfo();
        List<GetPeerInfoResponse> GetPeerInfo();
        //  listbanned
        void Ping();
        //  setban

        #endregion

        #region Rawtransactions

        string CreateRawTransaction(CreateRawTransactionRequest rawTransaction);
        DecodeRawTransactionResponse DecodeRawTransaction(string rawTransactionHexString);
        DecodeScriptResponse DecodeScript(string hexString);
        //  fundrawtransaction
        GetRawTransactionResponse GetRawTransaction(string txId, int verbose = 0);
        string SendRawTransaction(string rawTransactionHexString, bool? allowHighFees = false);
        SignRawTransactionResponse SignRawTransaction(SignRawTransactionRequest signRawTransactionRequest);
        GetFundRawTransactionResponse GetFundRawTransaction(string rawTransactionHex);

        #endregion

        #region Util

        CreateMultiSigResponse CreateMultiSig(int nRquired, List<string> publicKeys);
        decimal EstimateFee(ushort nBlocks);
        EstimateSmartFeeResponse EstimateSmartFee(ushort nBlocks);
        decimal EstimatePriority(ushort nBlocks);
        //  estimatesmartfee
        //  estimatesmartpriority
        ValidateAddressResponse ValidateAddress(string bitcoinAddress);
        bool VerifyMessage(string bitcoinAddress, string signature, string message);

        #endregion

        #region Wallet

        //  abandontransaction
        string AddMultiSigAddress(int nRquired, List<string> publicKeys, string account = null);
        string AddWitnessAddress(string address);
        void BackupWallet(string destination);
        string DumpPrivKey(string bitcoinAddress);
        void DumpWallet(string filename);
        string GetAccount(string bitcoinAddress);
        string GetAccountAddress(string account);
        List<string> GetAddressesByAccount(string account);
        decimal GetBalance(string account = null, int minConf = 1, bool? includeWatchonly = null);
        string GetNewAddress(string account = "");
        string GetRawChangeAddress();
        decimal GetReceivedByAccount(string account, int minConf = 1);
        decimal GetReceivedByAddress(string bitcoinAddress, int minConf = 1);
        GetTransactionResponse GetTransaction(string txId, bool? includeWatchonly = null);
        decimal GetUnconfirmedBalance();
        GetWalletInfoResponse GetWalletInfo();
        void ImportAddress(string address, string label = null, bool rescan = true);
        string ImportPrivKey(string privateKey, string label = null, bool rescan = true);
        //  importpubkey
        void ImportWallet(string filename);
        string KeyPoolRefill(uint newSize = 100);
        Dictionary<string, decimal> ListAccounts(int minConf = 1, bool? includeWatchonly = null);
        List<List<ListAddressGroupingsResponse>> ListAddressGroupings();
        string ListLockUnspent();
        List<ListReceivedByAccountResponse> ListReceivedByAccount(int minConf = 1, bool includeEmpty = false, bool? includeWatchonly = null);
        List<ListReceivedByAddressResponse> ListReceivedByAddress(int minConf = 1, bool includeEmpty = false, bool? includeWatchonly = null);
        ListSinceBlockResponse ListSinceBlock(string blockHash = null, int targetConfirmations = 1, bool? includeWatchonly = null);
        List<ListTransactionsResponse> ListTransactions(string account = null, int count = 10, int from = 0, bool? includeWatchonly = null);
        List<ListUnspentResponse> ListUnspent(int minConf = 1, int maxConf = 9999999, List<string> addresses = null);
        bool LockUnspent(bool unlock, IList<ListUnspentResponse> listUnspentResponses);
        bool Move(string fromAccount, string toAccount, decimal amount, int minConf = 1, string comment = "");
        string SendFrom(string fromAccount, string toBitcoinAddress, decimal amount, int minConf = 1, string comment = null, string commentTo = null);
        string SendMany(string fromAccount, Dictionary<string, decimal> toBitcoinAddress, int minConf = 1, string comment = null);
        string SendToAddress(string bitcoinAddress, decimal amount, string comment = null, string commentTo = null, bool subtractFeeFromAmount = false);
        string SetAccount(string bitcoinAddress, string account);
        string SetTxFee(decimal amount);
        string SignMessage(string bitcoinAddress, string message);
        string WalletLock();
        string WalletPassphrase(string passphrase, int timeoutInSeconds);
        string WalletPassphraseChange(string oldPassphrase, string newPassphrase);

        #endregion

        #region XAYA

        /// <summary>
        /// XAYA: Updates the value for a name.
        /// </summary>
        /// <param name="name">The name including the namespace, e.g. "p/xaya".</param>
        /// <param name="value">The value to update. Must be valid JSON.</param>
        /// <param name="parameters">Any parameters. Must be sent as an object.</param>
        /// <returns>A transaction ID, i.e. a txid.</returns>
        string NameUpdate(string name, string value, object parameters);
        List<GetNameListResponse> GetNameList();

        /// <summary>
        /// XAYA: Registers a name on the XAYA blockchain. 
        /// </summary>
        /// <param name="name">The name to register. It must include the namespace, e.g. "p/".</param>
        /// <param name="value">The value must be valid JSON, e.g. "{}". </param>
        /// <param name="parameters">Any parameters to send. This can be a new/null object.</param>
        /// <returns>A XAYA txid is returned if the registration succeeds. Otherwise, "Failed." is returned.</returns>
        string RegisterName(string name, string value, object parameters);

        /// <summary>
        /// XAYA: Checks to see if a name exists. Text fields show "Does not exist." if the name does not exist, 
        /// numeric fields return -1 if it does not exist, and boolean fileds return false if the name does not exist. Otherwise, regular data is returned.
        /// </summary>
        /// <param name="name">The name to check. It must be a valid name and include the namespace, e.g. "p/".</param>
        /// <returns>Returns a GetShowNameResponse object. Text fields show "Does not exist." if the name does not exist, 
        /// numeric fields return -1 if it does not exist, and boolean fileds return false if the name does not exist. Otherwise, regular data is returned.</returns>   
        GetShowNameResponse ShowName(string name);

        /// <summary>
        /// XAYA: Gets a list of all pending names in the mempool. You can use this method after creating a name when 
        /// you want to find out if it has been mined into the blockchain or not. 
        /// </summary>
        /// <returns>A list of all pending names in the mempool that have not been mined into the blockchain yet.</returns>
        List<GetNamePendingResponse> NamePending();

        /// <summary>
        /// XAYA: Returns a list of names existing on the XAYA blockchain. The list starts at "name" and continues for "count".
        /// </summary>
        /// <param name="name">The name to start the list from.</param>
        /// <param name="count">The number of names to return from the starting name.</param>
        /// <returns>A List of GetNameScanResponses that contain information about names.</returns>
        List<GetNameScanResponse> NameScan(string name, int count);

        /// <summary>
        /// XAYA: Send CHI coins to a "name" instead of a traditional crypto address. 
        /// The name is also associated with a regular CHI address. We can send coins to a name as a proxy. 
        /// </summary>
        /// <param name="name">The "name" that will receive the coins. </param>
        /// <param name="amount">The amount of CHI to sent to 8 decimal places. </param>
        /// <returns>A txid.</returns>
        string SendToName(string name, decimal amount);

        /// <summary>
        /// XAYA: NOTE - this is not a regular method and requires the daemon to run with the "-namehistory" option. 
        /// The name_history RPC method returns the entire history of a name's values on the blockchain.
        /// </summary>
        /// <param name="name">The name you want to know the history for.</param>
        /// <returns>A List of GetNameHistoryResponses that contain the complete history for a name.</returns>
        List<GetNameHistoryResponse> NameHistory(string name);

        /// <summary>
        /// XAYA: This is a blocking operation. It is used in independent threads to wait for new game states to come in.
        /// </summary>
        void WaitForChange();

        /// <summary>
        /// XAYA: This is called after the WaitForChange method to get the most recent game state. 
        /// </summary>
        /// <returns>The most recent game state.</returns>
        GameStateResult GetCurrentState();
        #endregion
    }
}