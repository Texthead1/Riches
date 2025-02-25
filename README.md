# Riches
A simple GUI application to instantly max out the gold to 65535 on all compatible Skylanders figures, in the Unity Engine, for Windows.

This application is able to modify all Skylanders figures that can possess money/gold, including core Skylanders, Giants, Trap Masters etc... SWAP Force Skylanders, as well as Senseis and Creation Crystals, can also be modified without issue.

Multiple Skylanders can be handled at once on a single Portal of Power without any errors. However, only a single Portal of Power should be used at any given time.

This application is powered by Portal-To-Unity, an in-progress framework for interfacing with the Skylanders' Portals of Power in the Unity Engine. Note that Wireless Portals of Power are not correctly supported and may lead to strange behaviours due to the wireless dongle.

## Usage
A correct salt.txt is required at "Assets/StreamingAssets/" to decrypt the Skylanders figures. The contents of the aforemention file is used as part of the MD5 hash to generate the encryption key. Without it, the application will not execute correctly.

This application expects the Portals of Power to be using the `libusbK` driver. Please make sure you install the driver first, which can be done via [Zadig](https://zadig.akeo.ie/), and apply it to any Portal of Power you wish to use.