# Dissonance-Netick
 An integration of Dissonance voice chat with Netick networking

## Requirements:
- [Netick 2 for Unity](https://github.com/NetickNetworking/NetickForUnity)
- [Dissonance voice chat](https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078)

## How to use:
- Check the Sandbox prefab included in the demo scene for help with setup.
- Add the "Dissonance Comms" component to your network sandbox prefab. Disable it.
- Add the "NetickCommsNetwork" component to your network sandbox prefab. This will also automatically add the "NetickCommsNetworkBase" component to your sandbox prefab.
- Global Voice Chat: Add the "Voice receipt trigger" and "Voice broadcast trigger" components to your network sandbox prefab. Disable both of these components. Set the desired chat room
- Proximity Voice Chat: Add the Voice Proximty Broadcast and receipt triggers to your sandbox prefab. Disable both of these components. set the desired chat room. Add the "Netick Proximity Chat" component to your player prefab.

## Demo Scene:
- Hold V to transmit Proximity voice chat
- Hold B to transmit Global voice chat

## Issues:
- Does not work with sandboxing or multi-server (due to the dissonance limit of having a singular Dissonance Comms object)

## Support
Feel free to ping me in the [Netick official discord](https://discord.com/invite/uV6bfG66Fx)
