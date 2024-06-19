# Dissonance-Netick
 An integration of Dissonance voice chat with Netick networking

Requirements:
- [Netick 2 for Unity](https://github.com/NetickNetworking/NetickForUnity)
- [Dissonance voice chat](https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078)

How to use:
- add the "dissonance comms" component to your network sandbox prefab
- add the "NetickCommsNetwork" component to your network sandbox prefab. this will also automatically add the "NetickCommsNetworkBase" component to your sandbox prefab.
- add a "voice receipt trigger" and "voice broadcast trigger" component to your network sandbox prefab, and set the desired chat room

Issues:
- Does not work with sandboxing or multi-server
- project files and structure need to be cleaned up
