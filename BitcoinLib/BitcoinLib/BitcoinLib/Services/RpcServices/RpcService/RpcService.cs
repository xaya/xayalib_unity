// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System;
using System.Collections.Generic;
using System.Linq;
using BitcoinLib.Requests.AddNode;
using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Requests.SignRawTransaction;
using BitcoinLib.Responses;
using BitcoinLib.RPC.Connector;
using BitcoinLib.RPC.Specifications;
using BitcoinLib.Services.Coins.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace BitcoinLib.Services
{
    //   Implementation of API calls list, as found at: https://en.bitcoin.it/wiki/Original_Bitcoin_client/API_Calls_list (note: this list is often out-of-date so call "help" in your bitcoin-cli to get the latest signatures)
    public partial class CoinService : ICoinService
    {
        protected readonly IRpcConnector _rpcConnector;

        public CoinService()
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, null, null, null, null, 0);
        }

        public CoinService(bool useTestnet) : this()
        {
            Parameters.UseTestnet = useTestnet;
        }

        public CoinService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword)
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, daemonUrl, rpcUsername, rpcPassword, walletPassword, 0);
        }

        //  this provides support for cases where *.config files are not an option
        public CoinService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds);
        }

        public string AddMultiSigAddress(int nRquired, List<string> publicKeys, string account)
        {
            return account != null
                ? _rpcConnector.MakeRequest<string>(RpcMethods.addmultisigaddress, nRquired, publicKeys, account)
                : _rpcConnector.MakeRequest<string>(RpcMethods.addmultisigaddress, nRquired, publicKeys);
        }

        public void AddNode(string node, NodeAction action)
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.addnode, node, action.ToString());
        }

        public string AddWitnessAddress(string address)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.addwitnessaddress, address);
        }

        public void BackupWallet(string destination)
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.backupwallet, destination);
        }

        public CreateMultiSigResponse CreateMultiSig(int nRquired, List<string> publicKeys)
        {
            return _rpcConnector.MakeRequest<CreateMultiSigResponse>(RpcMethods.createmultisig, nRquired, publicKeys);
        }

        public string CreateRawTransaction(CreateRawTransactionRequest rawTransaction)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.createrawtransaction, rawTransaction.Inputs, rawTransaction.Outputs);
        }

        public DecodeRawTransactionResponse DecodeRawTransaction(string rawTransactionHexString)
        {
            return _rpcConnector.MakeRequest<DecodeRawTransactionResponse>(RpcMethods.decoderawtransaction, rawTransactionHexString);
        }

        public DecodeScriptResponse DecodeScript(string hexString)
        {
            return _rpcConnector.MakeRequest<DecodeScriptResponse>(RpcMethods.decodescript, hexString);
        }

        public string DumpPrivKey(string bitcoinAddress)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.dumpprivkey, bitcoinAddress);
        }

        public void DumpWallet(string filename)
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.dumpwallet, filename);
        }

        public decimal EstimateFee(ushort nBlocks)
        {
            return _rpcConnector.MakeRequest<decimal>(RpcMethods.estimatefee, nBlocks);
        }

        public EstimateSmartFeeResponse EstimateSmartFee(ushort nBlocks)
        {
            return _rpcConnector.MakeRequest<EstimateSmartFeeResponse>(RpcMethods.estimatesmartfee, nBlocks);
        }

        public decimal EstimatePriority(ushort nBlocks)
        {
            return _rpcConnector.MakeRequest<decimal>(RpcMethods.estimatepriority, nBlocks);
        }

        public string GetAccount(string bitcoinAddress)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.getaccount, bitcoinAddress);
        }

        public string GetAccountAddress(string account)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.getaccountaddress, account);
        }

        public GetAddedNodeInfoResponse GetAddedNodeInfo(string dns, string node)
        {
            return string.IsNullOrWhiteSpace(node)
                ? _rpcConnector.MakeRequest<GetAddedNodeInfoResponse>(RpcMethods.getaddednodeinfo, dns)
                : _rpcConnector.MakeRequest<GetAddedNodeInfoResponse>(RpcMethods.getaddednodeinfo, dns, node);
        }

        public List<string> GetAddressesByAccount(string account)
        {
            return _rpcConnector.MakeRequest<List<string>>(RpcMethods.getaddressesbyaccount, account);
        }

        public decimal GetBalance(string account, int minConf, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<decimal>(RpcMethods.getbalance, (string.IsNullOrWhiteSpace(account) ? "*" : account), minConf)
                : _rpcConnector.MakeRequest<decimal>(RpcMethods.getbalance, (string.IsNullOrWhiteSpace(account) ? "*" : account), minConf, includeWatchonly);
        }

        public string GetBestBlockHash()
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.getbestblockhash);
        }

        public GetBlockResponse GetBlock(string hash, bool verbose)
        {
            return _rpcConnector.MakeRequest<GetBlockResponse>(RpcMethods.getblock, hash, verbose);
        }

        public GetBlockchainInfoResponse GetBlockchainInfo()
        {
            return _rpcConnector.MakeRequest<GetBlockchainInfoResponse>(RpcMethods.getblockchaininfo);
        }

        public uint GetBlockCount()
        {
            return _rpcConnector.MakeRequest<uint>(RpcMethods.getblockcount);
        }

        public string GetBlockHash(long index)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.getblockhash, index);
        }

        public GetBlockTemplateResponse GetBlockTemplate(params object[] parameters)
        {
            return parameters == null
                ? _rpcConnector.MakeRequest<GetBlockTemplateResponse>(RpcMethods.getblocktemplate)
                : _rpcConnector.MakeRequest<GetBlockTemplateResponse>(RpcMethods.getblocktemplate, parameters);
        }

        public List<GetChainTipsResponse> GetChainTips()
        {
            return _rpcConnector.MakeRequest<List<GetChainTipsResponse>>(RpcMethods.getchaintips);
        }

        public int GetConnectionCount()
        {
            return _rpcConnector.MakeRequest<int>(RpcMethods.getconnectioncount);
        }

        public double GetDifficulty()
        {
            return _rpcConnector.MakeRequest<double>(RpcMethods.getdifficulty);
        }

        public bool GetGenerate()
        {
            return _rpcConnector.MakeRequest<bool>(RpcMethods.getgenerate);
        }

        [Obsolete("Please use calls: GetWalletInfo(), GetBlockchainInfo() and GetNetworkInfo() instead")]
        public GetInfoResponse GetInfo()
        {
            return _rpcConnector.MakeRequest<GetInfoResponse>(RpcMethods.getinfo);
        }

        public GetMemPoolInfoResponse GetMemPoolInfo()
        {
            return _rpcConnector.MakeRequest<GetMemPoolInfoResponse>(RpcMethods.getmempoolinfo);
        }

        public GetMiningInfoResponse GetMiningInfo()
        {
            return _rpcConnector.MakeRequest<GetMiningInfoResponse>(RpcMethods.getmininginfo);
        }

        public GetNetTotalsResponse GetNetTotals()
        {
            return _rpcConnector.MakeRequest<GetNetTotalsResponse>(RpcMethods.getnettotals);
        }

        public ulong GetNetworkHashPs(uint blocks, long height)
        {
            return _rpcConnector.MakeRequest<ulong>(RpcMethods.getnetworkhashps);
        }

        public GetNetworkInfoResponse GetNetworkInfo()
        {
            return _rpcConnector.MakeRequest<GetNetworkInfoResponse>(RpcMethods.getnetworkinfo);
        }

        public string GetNewAddress(string account)
        {
            return string.IsNullOrWhiteSpace(account)
                ? _rpcConnector.MakeRequest<string>(RpcMethods.getnewaddress)
                : _rpcConnector.MakeRequest<string>(RpcMethods.getnewaddress, account);
        }

        public List<GetPeerInfoResponse> GetPeerInfo()
        {
            return _rpcConnector.MakeRequest<List<GetPeerInfoResponse>>(RpcMethods.getpeerinfo);
        }

        public GetRawMemPoolResponse GetRawMemPool(bool verbose)
        {
            var getRawMemPoolResponse = new GetRawMemPoolResponse
            {
                IsVerbose = verbose
            };

            var rpcResponse = _rpcConnector.MakeRequest<object>(RpcMethods.getrawmempool, verbose);

            if (!verbose)
            {
                var rpcResponseAsArray = (JArray) rpcResponse;

                foreach (string txId in rpcResponseAsArray)
                {
                    getRawMemPoolResponse.TxIds.Add(txId);
                }

                return getRawMemPoolResponse;
            }

            IList<KeyValuePair<string, JToken>> rpcResponseAsKvp = (new EnumerableQuery<KeyValuePair<string, JToken>>(((JObject) (rpcResponse)))).ToList();
            IList<JToken> children = JObject.Parse(rpcResponse.ToString()).Children().ToList();

            for (var i = 0; i < children.Count(); i++)
            {
                var getRawMemPoolVerboseResponse = new GetRawMemPoolVerboseResponse
                {
                    TxId = rpcResponseAsKvp[i].Key
                };

                getRawMemPoolResponse.TxIds.Add(getRawMemPoolVerboseResponse.TxId);

                foreach (var property in children[i].SelectMany(grandChild => grandChild.OfType<JProperty>()))
                {
                    switch (property.Name)
                    {
                        case "currentpriority":

                            double currentPriority;

                            if (double.TryParse(property.Value.ToString(), out currentPriority))
                            {
                                getRawMemPoolVerboseResponse.CurrentPriority = currentPriority;
                            }

                            break;

                        case "depends":

                            foreach (var jToken in property.Value)
                            {
                                getRawMemPoolVerboseResponse.Depends.Add(jToken.Value<string>());
                            }

                            break;

                        case "fee":

                            decimal fee;

                            if (decimal.TryParse(property.Value.ToString(), out fee))
                            {
                                getRawMemPoolVerboseResponse.Fee = fee;
                            }

                            break;

                        case "height":

                            int height;

                            if (int.TryParse(property.Value.ToString(), out height))
                            {
                                getRawMemPoolVerboseResponse.Height = height;
                            }

                            break;

                        case "size":

                            int size;

                            if (int.TryParse(property.Value.ToString(), out size))
                            {
                                getRawMemPoolVerboseResponse.Size = size;
                            }

                            break;

                        case "startingpriority":

                            double startingPriority;

                            if (double.TryParse(property.Value.ToString(), out startingPriority))
                            {
                                getRawMemPoolVerboseResponse.StartingPriority = startingPriority;
                            }

                            break;

                        case "time":

                            int time;

                            if (int.TryParse(property.Value.ToString(), out time))
                            {
                                getRawMemPoolVerboseResponse.Time = time;
                            }

                            break;

                        default:

                            throw new Exception("Unkown property: " + property.Name + " in GetRawMemPool()");
                    }
                }
                getRawMemPoolResponse.VerboseResponses.Add(getRawMemPoolVerboseResponse);
            }
            return getRawMemPoolResponse;
        }

        public string GetRawChangeAddress()
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.getrawchangeaddress);
        }

        public GetRawTransactionResponse GetRawTransaction(string txId, int verbose)
        {
            if (verbose == 0)
            {
                return new GetRawTransactionResponse
                {
                    Hex = _rpcConnector.MakeRequest<string>(RpcMethods.getrawtransaction, txId, verbose)
                };
            }

            if (verbose == 1)
            {
                return _rpcConnector.MakeRequest<GetRawTransactionResponse>(RpcMethods.getrawtransaction, txId, verbose);
            }

            throw new Exception("Invalid verbose value: " + verbose + " in GetRawTransaction()!");
        }

        public decimal GetReceivedByAccount(string account, int minConf)
        {
            return _rpcConnector.MakeRequest<decimal>(RpcMethods.getreceivedbyaccount, account, minConf);
        }

        public decimal GetReceivedByAddress(string bitcoinAddress, int minConf)
        {
            return _rpcConnector.MakeRequest<decimal>(RpcMethods.getreceivedbyaddress, bitcoinAddress, minConf);
        }

        public GetTransactionResponse GetTransaction(string txId, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettransaction, txId)
                : _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettransaction, txId, includeWatchonly);
        }

        public GetTransactionResponse GetTxOut(string txId, int n, bool includeMemPool)
        {
            return _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettxout, txId, n, includeMemPool);
        }

        public GetTxOutSetInfoResponse GetTxOutSetInfo()
        {
            return _rpcConnector.MakeRequest<GetTxOutSetInfoResponse>(RpcMethods.gettxoutsetinfo);
        }

        public decimal GetUnconfirmedBalance()
        {
            return _rpcConnector.MakeRequest<decimal>(RpcMethods.getunconfirmedbalance);
        }

        public GetWalletInfoResponse GetWalletInfo()
        {
            return _rpcConnector.MakeRequest<GetWalletInfoResponse>(RpcMethods.getwalletinfo);
        }

        public string Help(string command)
        {
            return string.IsNullOrWhiteSpace(command)
                ? _rpcConnector.MakeRequest<string>(RpcMethods.help)
                : _rpcConnector.MakeRequest<string>(RpcMethods.help, command);
        }

        public void ImportAddress(string address, string label, bool rescan)
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.importaddress, address, label, rescan);
        }

        public string ImportPrivKey(string privateKey, string label, bool rescan)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.importprivkey, privateKey, label, rescan);
        }

        public void ImportWallet(string filename)
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.importwallet, filename);
        }

        public string KeyPoolRefill(uint newSize)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.keypoolrefill, newSize);
        }

        public Dictionary<string, decimal> ListAccounts(int minConf, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<Dictionary<string, decimal>>(RpcMethods.listaccounts, minConf)
                : _rpcConnector.MakeRequest<Dictionary<string, decimal>>(RpcMethods.listaccounts, minConf, includeWatchonly);
        }

        public List<List<ListAddressGroupingsResponse>> ListAddressGroupings()
        {
            var unstructuredResponse = _rpcConnector.MakeRequest<List<List<List<object>>>>(RpcMethods.listaddressgroupings);
            var structuredResponse = new List<List<ListAddressGroupingsResponse>>(unstructuredResponse.Count);

            for (var i = 0; i < unstructuredResponse.Count; i++)
            {
                for (var j = 0; j < unstructuredResponse[i].Count; j++)
                {
                    if (unstructuredResponse[i][j].Count > 1)
                    {
                        var response = new ListAddressGroupingsResponse
                        {
                            Address = unstructuredResponse[i][j][0].ToString()
                        };

                        decimal balance;
                        if (decimal.TryParse(unstructuredResponse[i][j][1].ToString(), out balance))
                        {
                            response.Balance = balance;
                        }

                        if (unstructuredResponse[i][j].Count > 2)
                        {
                            response.Account = unstructuredResponse[i][j][2].ToString();
                        }

                        if (structuredResponse.Count < i + 1)
                        {
                            structuredResponse.Add(new List<ListAddressGroupingsResponse>());
                        }

                        structuredResponse[i].Add(response);
                    }
                }
            }
            return structuredResponse;
        }

        public string ListLockUnspent()
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.listlockunspent);
        }

        public List<ListReceivedByAccountResponse> ListReceivedByAccount(int minConf, bool includeEmpty, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<List<ListReceivedByAccountResponse>>(RpcMethods.listreceivedbyaccount, minConf, includeEmpty)
                : _rpcConnector.MakeRequest<List<ListReceivedByAccountResponse>>(RpcMethods.listreceivedbyaccount, minConf, includeEmpty, includeWatchonly);
        }

        public List<ListReceivedByAddressResponse> ListReceivedByAddress(int minConf, bool includeEmpty, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<List<ListReceivedByAddressResponse>>(RpcMethods.listreceivedbyaddress, minConf, includeEmpty)
                : _rpcConnector.MakeRequest<List<ListReceivedByAddressResponse>>(RpcMethods.listreceivedbyaddress, minConf, includeEmpty, includeWatchonly);
        }

        public ListSinceBlockResponse ListSinceBlock(string blockHash, int targetConfirmations, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<ListSinceBlockResponse>(RpcMethods.listsinceblock, (string.IsNullOrWhiteSpace(blockHash) ? "" : blockHash), targetConfirmations)
                : _rpcConnector.MakeRequest<ListSinceBlockResponse>(RpcMethods.listsinceblock, (string.IsNullOrWhiteSpace(blockHash) ? "" : blockHash), targetConfirmations, includeWatchonly);
        }

        public List<ListTransactionsResponse> ListTransactions(string account, int count, int from, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? _rpcConnector.MakeRequest<List<ListTransactionsResponse>>(RpcMethods.listtransactions, (string.IsNullOrWhiteSpace(account) ? "*" : account), count, from)
                : _rpcConnector.MakeRequest<List<ListTransactionsResponse>>(RpcMethods.listtransactions, (string.IsNullOrWhiteSpace(account) ? "*" : account), count, from, includeWatchonly);
        }

        public List<ListUnspentResponse> ListUnspent(int minConf, int maxConf, List<string> addresses)
        {
            return _rpcConnector.MakeRequest<List<ListUnspentResponse>>(RpcMethods.listunspent, minConf, maxConf, (addresses ?? new List<string>()));
        }

        public bool LockUnspent(bool unlock, IList<ListUnspentResponse> listUnspentResponses)
        {
            IList<object> transactions = new List<object>();

            foreach (var listUnspentResponse in listUnspentResponses)
            {
                transactions.Add(new
                {
                    txid = listUnspentResponse.TxId, vout = listUnspentResponse.Vout
                });
            }

            return _rpcConnector.MakeRequest<bool>(RpcMethods.lockunspent, unlock, transactions.ToArray());
        }

        public bool Move(string fromAccount, string toAccount, decimal amount, int minConf, string comment)
        {
            return _rpcConnector.MakeRequest<bool>(RpcMethods.move, fromAccount, toAccount, amount, minConf, comment);
        }

        public void Ping()
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.ping);
        }

        public bool PrioritiseTransaction(string txId, decimal priorityDelta, decimal feeDelta)
        {
            return _rpcConnector.MakeRequest<bool>(RpcMethods.prioritisetransaction, txId, priorityDelta, feeDelta);
        }

        public string SendFrom(string fromAccount, string toBitcoinAddress, decimal amount, int minConf, string comment, string commentTo)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.sendfrom, fromAccount, toBitcoinAddress, amount, minConf, comment, commentTo);
        }

        public string SendMany(string fromAccount, Dictionary<string, decimal> toBitcoinAddress, int minConf, string comment)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.sendmany, fromAccount, toBitcoinAddress, minConf, comment);
        }

        public string SendRawTransaction(string rawTransactionHexString, bool? allowHighFees)
        {
            return allowHighFees == null
                ? _rpcConnector.MakeRequest<string>(RpcMethods.sendrawtransaction, rawTransactionHexString)
                : _rpcConnector.MakeRequest<string>(RpcMethods.sendrawtransaction, rawTransactionHexString, allowHighFees);
        }

        public string SendToAddress(string bitcoinAddress, decimal amount, string comment, string commentTo, bool subtractFeeFromAmount)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.sendtoaddress, bitcoinAddress, amount, comment, commentTo, subtractFeeFromAmount);
        }

        public string SetAccount(string bitcoinAddress, string account)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.setaccount, bitcoinAddress, account);
        }

        public string SetGenerate(bool generate, short generatingProcessorsLimit)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.setgenerate, generate, generatingProcessorsLimit);
        }

        public string SetTxFee(decimal amount)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.settxfee, amount);
        }

        public string SignMessage(string bitcoinAddress, string message)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.signmessage, bitcoinAddress, message);
        }

        public SignRawTransactionResponse SignRawTransaction(SignRawTransactionRequest request)
        {
            #region default values

            if (request.Inputs.Count == 0)
            {
                request.Inputs = null;
            }

            if (string.IsNullOrWhiteSpace(request.SigHashType))
            {
                request.SigHashType = SigHashType.All;
            }

            if (request.PrivateKeys.Count == 0)
            {
                request.PrivateKeys = null;
            }

            #endregion

            return _rpcConnector.MakeRequest<SignRawTransactionResponse>(RpcMethods.signrawtransaction, request.RawTransactionHex, request.Inputs, request.PrivateKeys, request.SigHashType);
        }

        public GetFundRawTransactionResponse GetFundRawTransaction(string rawTransactionHex)
        {
            return _rpcConnector.MakeRequest<GetFundRawTransactionResponse>(RpcMethods.fundrawtransaction, rawTransactionHex);
        }

        public string Stop()
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.stop);
        }

        public string SubmitBlock(string hexData, params object[] parameters)
        {
            return parameters == null
                ? _rpcConnector.MakeRequest<string>(RpcMethods.submitblock, hexData)
                : _rpcConnector.MakeRequest<string>(RpcMethods.submitblock, hexData, parameters);
        }

        public ValidateAddressResponse ValidateAddress(string bitcoinAddress)
        {
            return _rpcConnector.MakeRequest<ValidateAddressResponse>(RpcMethods.validateaddress, bitcoinAddress);
        }

        public bool VerifyChain(ushort checkLevel, uint numBlocks)
        {
            return _rpcConnector.MakeRequest<bool>(RpcMethods.verifychain, checkLevel, numBlocks);
        }

        public bool VerifyMessage(string bitcoinAddress, string signature, string message)
        {
            return _rpcConnector.MakeRequest<bool>(RpcMethods.verifymessage, bitcoinAddress, signature, message);
        }

        public string WalletLock()
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.walletlock);
        }

        public string WalletPassphrase(string passphrase, int timeoutInSeconds)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.walletpassphrase, passphrase, timeoutInSeconds);
        }

        public string WalletPassphraseChange(string oldPassphrase, string newPassphrase)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.walletpassphrasechange, oldPassphrase, newPassphrase);
        }

        public override string ToString()
        {
            return Parameters.CoinLongName;
        }

        /// <summary>
        /// XAYA: Updates the value for a name.
        /// </summary>
        /// <param name="name">The name including the namespace, e.g. "p/xaya".</param>
        /// <param name="value">The value to update. Must be valid JSON.</param>
        /// <param name="parameters">Any parameters. Must be sent as an object.</param>
        /// <returns>A transaction ID, i.e. a txid.</returns>
        public string NameUpdate(string name, string value, object parameters)
        {
            return _rpcConnector.MakeRequest<string>(RpcMethods.name_update,name,value,parameters);
        }

        /// <summary>
        /// XAYA: Gets a list of all names in a wallet. 
        /// </summary>
        /// <returns>A list of "GetNameListResponse" objects. </returns>
        public List<GetNameListResponse> GetNameList()
        {
            return _rpcConnector.MakeRequest<List<GetNameListResponse>>(RpcMethods.name_list);
        }

        /// <summary>
        /// XAYA: Registers a name on the XAYA blockchain. 
        /// </summary>
        /// <param name="name">The name to register. It must include the namespace, e.g. "p/".</param>
        /// <param name="value">The value must be valid JSON, e.g. "{}". </param>
        /// <param name="parameters">Any parameters to send. This can be a new/null object.</param>
        /// <returns>A XAYA txid is returned if the registration succeeds. Otherwise, "Failed." is returned.</returns>
        public string RegisterName(string name, string value, object parameters)
        {
            string r = "Failed.";
            // The value must be valid JSON, so we set it to valid JSON if it's empty.
            if (value == string.Empty)
            {
                value = "{}";
            }

            bool valueIsGood = IsValidJson(value);
            bool nameIsGood = IsValidName(name);
            if (valueIsGood && nameIsGood)
            {
                try
                {
                    r = _rpcConnector.MakeRequest<string>(RpcMethods.name_register, name, value, parameters);
                }
                catch
                {
                    r = "Failed.";
                }
            }

            return r;
        }


        /// <summary>
        /// XAYA: Checks to see if a name exists. Text fields show "" (an empty string) if the name does not exist, 
        /// numeric fields return -1 if it does not exist, and boolean fileds return false if the name does not exist. Otherwise, regular data is returned.
        /// </summary>
        /// <param name="name">The name to check. It must be a valid name and include the namespace, e.g. "p/".</param>
        /// <returns>Returns a GetShowNameResponse object. Text fields show an empty string if the name does not exist, 
        /// numeric fields return -1 if it does not exist, and boolean fileds return false if the name does not exist. Otherwise, regular data is returned.</returns> 
        public GetShowNameResponse ShowName(string name)
        {
            GetShowNameResponse response = new GetShowNameResponse();
            // Initialise all fields as empty.
            response.name = "";
            response.name_encoding = "";
            response.name_error = "";
            response.value = "";
            response.value_encoding = "";
            response.txid = "";
            response.vout = -1;
            response.address = "";
            response.ismine = false;
            response.height = -1;

            try
            {
                response = _rpcConnector.MakeRequest<GetShowNameResponse>(RpcMethods.name_show, name);
            }
            catch
            {
                // leave as is
            }

            return response;
        }

        /// <summary>
        /// XAYA: Gets a list of all pending names in the mempool. You can use this method after creating a name when 
        /// you want to find out if it has been mined into the blockchain or not. 
        /// </summary>
        /// <returns>A list of all pending names in the mempool that have not been mined into the blockchain yet.</returns>
        public List<GetNamePendingResponse> NamePending()
        {
            return _rpcConnector.MakeRequest<List<GetNamePendingResponse>>(RpcMethods.name_pending);
        }

        /// <summary>
        /// XAYA: Returns a list of names existing on the XAYA blockchain. The list starts at "name" and continues for "count".
        /// </summary>
        /// <param name="name">The name to start the list from.</param>
        /// <param name="count">The number of names to return from the starting name.</param>
        /// <returns>A List of GetNameScanResponses that contain information about names.</returns>
        public List<GetNameScanResponse> NameScan(string name, int count)
        {
            List<GetNameScanResponse> response = _rpcConnector.MakeRequest<List<GetNameScanResponse>>(RpcMethods.name_scan, name, count);

            return response;
        }

        /// <summary>
        /// XAYA: Send CHI coins to a "name" instead of a traditional crypto address. 
        /// The name is also associated with a regular CHI address. We can send coins to a name as a proxy. 
        /// </summary>
        /// <param name="name">The "name" that will receive the coins. </param>
        /// <param name="amount">The amount of CHI to sent to 8 decimal places. </param>
        /// <returns>A txid.</returns>
        public string SendToName(string name, decimal amount)
        {
            string response = _rpcConnector.MakeRequest<string>(RpcMethods.sendtoname, name, amount);

            return response;
        }

        /// <summary>
        /// XAYA: NOTE - this is not a regular method and requires the daemon to run with the "-namehistory" option. 
        /// The name_history RPC method returns the entire history of a name's values on the blockchain.
        /// </summary>
        /// <param name="name">The name you want to know the history for.</param>
        /// <returns>A List of GetNameHistoryResponses that contain the complete history for a name.</returns>
        public List<GetNameHistoryResponse> NameHistory(string name)
        {
            List<GetNameHistoryResponse> response = _rpcConnector.MakeRequest<List<GetNameHistoryResponse>>(RpcMethods.name_history, name);

            return response;
        }


        /// <summary>
        /// XAYA: This is a blocking operation. It is used in independent threads to wait for new game states to come in.
        /// </summary>
        public void WaitForChange()
        {
            _rpcConnector.MakeRequest<string>(RpcMethods.waitforchange);
        }

        /// <summary>
        /// XAYA: This is called after the WaitForChange method to get the most recent game state. 
        /// </summary>
        /// <returns>The most recent game state.</returns>
        public GameStateResult GetCurrentState()
        {
            return _rpcConnector.MakeRequest<GameStateResult>(RpcMethods.getcurrentstate);
        }


        #region XAYA utilities for error checking, etc.

        /// <summary>
        /// XAYA: Returns whether or not a name is valid.
        /// </summary>
        /// <param name="name">The name including the namespace.</param>
        /// <returns>True if the name is valid. False if the name is not valid.</returns>
        private static bool IsValidName(string name)
        {
            bool result = false;

            var match = Regex.Match(name, @"[a-z]+\/.*");

            if (!match.Success)
                return result;

            // We must measure bytes and NOT characters.
            // https://github.com/xaya/xaya/blob/master/doc/xaya/blockchain.md#name-and-value-restrictions-
            int length = System.Text.ASCIIEncoding.UTF8.GetByteCount(name);

            if (name.Length >= 3 && length <= 256)
            {
                // If the above tests pass, then we have a valid name.
                return true;
            }
            // This is merely a catch all. 
            return result;
        }

        /// <summary>
        /// XAYA: Returns whether or not a string is valid JSON. 
        /// From https://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net
        /// </summary>
        /// <param name="strInput">The string to check.</param>
        /// <returns>True for valid JSON or false for invalid JSON.</returns>
        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For objects
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For arrays
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    // Exception in parsing JSON.
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) // Other exceptions.
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion



    }
}
