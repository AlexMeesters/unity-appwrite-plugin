## Unofficial & non affiliated [Appwrite](https://github.com/appwrite/appwrite) plugin for Unity

**Still in early development. Not yet battle tested.**

So far this plugin only supports some of the account features & storing of cookies.  
Logins are automatically stored on disk for ease of use.

Current features
 - Login
 - Get user information
 - Get JWT token
 - Register user
 - Send verification mail
 - Store cookies on disk automatically
 - Simple AES Encryption for stored data

Dependencies
 - [UniTask](https://github.com/Cysharp/UniTask) for lower allocation counts & WebGL support
 - JSON . Net for JSON support
  
## Included menu
  
![Menu Example](https://github.com/AlexMeesters/unity-appwrite-plugin/blob/main/menu_example.png)

## Getting started


### Install UniTask

This package relies on UniTask, a package that uses a lot less allocations for C# Tasks.  
It also makes deploying to WebGL easier as regular Tasks cause errors with WebGL.  
To install UniTask go here: https://github.com/Cysharp/UniTask#upm-package

### Install JSON. Net

JSON. Net is used to generate JSON for the requests, as well as deserialize responses from Appwrite.  
Reason I did not choose for the JsonUtility was because the underlying queries could still change,   
and this allowed me to just fetch specific parameters.

### To install JSON. Net

Add the following to the manifest.json in /YourProjectFolder/Packages/manifest.json  
"com.unity.nuget.newtonsoft-json": "3.0.2",

### Install TextMeshPro

The example menu makes use of TextMeshPro.
Go to the package manager and locate TextMeshPro and install it.


### Easy install using the Unity Package Manager
```
https://github.com/AlexMeesters/unity-appwrite-plugin.git?path=Assets/Plugins/Lowscope/Appwrite
```

## 

Once the project is installed and no errors occur. Run the game, and a file called "Appwrite Config" should appear in the Resources folder.  
Configure it to connect to your Appwrite server.

![Config Example](https://github.com/AlexMeesters/unity-appwrite-plugin/blob/main/config_example.png)
