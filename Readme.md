# Turbo Gravity Lemming Raketti Force

Three of the best games combined into one!

# How To Build
## Build under windows
- Install MonoGame for Visual Studio http://www.monogame.net/2015/04/29/monogame-3-4/
- Build & Run

## Build under linux
- Install MonoGame by following the instructions on http://www.monogame.net/documentation/?page=Setting_Up_MonoGame_Linux
- Open the project in MonoDevelop
- Open in the TurboPort.Launch project Content/Content.mgcb with the pipeline tool
- Build using the pipeline tool (ParticleEffect.fx will fail, but that can be ignored)
- Build & Run


# TODO
The issues I'm looking into are:
- Add network play (seems doable; requires above to be fixed)
- Clean stuff up!

For someone else (Stef?):
- Fix the level parsing and building correct collision geometry
- Fix audio (content cooking and playback)
- Integrate content builder to solution (see MonoGame NuGet docs)
- Clean stuff up!

Oh and make it fun \^_^
