# Overview

This repo contains 3 projects:

1. BitcoinLib
2. XAYAWrapper
3. XAYAUnity

# BitcoinLib

BitcoinLib is forked from [here](https://github.com/cryptean/bitcoinlib).

It is an RPC library for Bitcoin and some altcoins. XAYA has been added to it. 

BitcoinLib is used in several XAYA tutorials and example projects. It is also used in XAYAWrapper.

If you wish to use an app.config in your project, you must unset the `#define UNITY` compiler directive in [CoinParameters.cs](https://github.com/xaya/xayalib_unity/blob/master/BitcoinLib/BitcoinLib/BitcoinLib/CoinParameters/Base/CoinParameters.cs). 

# XAYAWrapper

XAYAWrapper is a C# wrapper for the statically linked libxayagame library and its dependencies. Those binaries can be found in the [XAYAUnity/XayaStateProcessor folder](XAYAUnity/XayaStateProcessor). 

XAYAWrapper references BitcoinLib, so you must build BitcoinLib first. You can access BitcoinLib through `xayaGameService` in XAYAWrapper. This eliminates the need to create another instance of BitcoinLib once you've constructed and connected XAYAWrapper. 

# XAYAUnity

XAYAUnity is an example implementation of the [Mover game found here](https://github.com/xaya/libxayagame/tree/master/mover). Tutorials for XAYAUnity can be found [here](https://github.com/xaya/xaya_tutorials/).

Open the XAYAUnity folder in Unity Editor to get started. 

## Comments for XAYAUnity

If you make any changes to the "BitcoinLib" and "XAYAWrapper" projects, you must recompile them and copy the new DLLs into the proper folders under the XAYAUnity folder. i.e.:

> \XAYAUnity\Assets\Plugins\BitcoinLib.dll
>
> \XAYAUnity\Assets\JsonDotNet\Assemblies\Windows\XAYAWrapper.dll

NOTE: To run XAYAUnity, You should have the XAYA Electron wallet running, but can optionally use the XAYA QT wallet or xayad, provided you run them with the proper flags. 

For developer support, visit our [Development forum here](https://forum.xaya.io/forum/6-development/).




 