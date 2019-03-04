// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using BitcoinLib.CoinParameters.Base;

namespace BitcoinLib.CoinParameters.XAYA
{
    public static class XAYAConstants
    {
        public sealed class Constants : CoinConstants<Constants>
        {
            public readonly int OneCHIInChitoshis = 100000000;
            public readonly decimal OneChitoshiInCHI = 0.00000001M;
            public readonly int ChitoshisPerChi = 100000000;
            public readonly string Symbol = "CHI";

            #region Custom constructor example - commented out on purpose

            //private static readonly Lazy<Constants> Lazy = new Lazy<Constants>(() => new Constants());

            //public static Constants Instance
            //{
            //    get
            //    {
            //        return Lazy.Value;
            //    }
            //}

            //private Constants()
            //{
            //  //  custom logic here
            //}

            #endregion
        }
    }
}