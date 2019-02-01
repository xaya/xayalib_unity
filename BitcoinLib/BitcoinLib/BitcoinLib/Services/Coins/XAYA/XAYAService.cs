// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using BitcoinLib.CoinParameters.XAYA;

namespace BitcoinLib.Services.Coins.XAYA
{
    public class XAYAService : CoinService, IXAYAService
    {
        public XAYAService(bool useTestnet = false) : base(useTestnet)
        {
        }

        public XAYAService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword)
        {
        }

        public XAYAService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds)
        {
        }

        public XAYAConstants.Constants Constants => XAYAConstants.Constants.Instance;
    }
}