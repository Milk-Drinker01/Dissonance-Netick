# Dissonance-Netick
 An integration of Dissonance voice chat with Netick networking

## Requirements:
- [Netick 2 for Unity](https://github.com/NetickNetworking/NetickForUnity)
- [Dissonance voice chat](https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078)

## How to use:
- Add the "dissonance comms" component to your network sandbox prefab
- Add the "NetickCommsNetwork" component to your network sandbox prefab. this will also automatically add the "NetickCommsNetworkBase" component to your sandbox prefab.
- Add a "voice receipt trigger" and "voice broadcast trigger" components to your network sandbox prefab, and set the desired chat room

## Issues:
- Does not work with sandboxing or multi-server (due to the dissonance limit of having a singular Dissonance Comms object)
- Project files and structure need to be cleaned up

## Support
Feel free to ping me in the [Netick official discord](https://discord.com/invite/uV6bfG66Fx)
