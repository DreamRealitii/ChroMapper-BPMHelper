# What is BPM Helper?
A mod for ChroMapper, intended to be a replacement for using AudioVortex ahead of time on songs with a constant BPM, and a replacement for plugging numbers into a math formula for songs with a variable BPM.
# How do I install it?
Download BPMHelper.dll file from Releases and place it in `chromapper/Plugins` folder.
# Anything I should do before using it?
- If you are mapping a song from a rhythm game, check the internet to see if there is official BPM info that you can use instead.
- There's no need to make the audio line up with the song's initial BPM if you don't want to, but if you want your map to be rankable on either leaderboard, make sure the audio you are mapping to has at least 1.5 seconds before where you plan on placing the first note, and 2.0 seconds after the last note. Use Audacity to add silence if needed.
- The map's global BPM value can be set either before or after you use BPM Helper place BPM changes in the map.
- Go to Settings -> Graphics -> Spectrogram settings and set `Sample Size` to 256, `Sample Quality` to 8, and `Logarithmic Shift` to at least 2. This will make the audio graph at the side easier to look at.
# How do I open it?
1. Open a map in ChroMapper.
2. Open the ChroMapper sidebar (normally done by pressing Tab).
3. Click the button with the tooltip "Open BPM Helper".
# How do I use it?
1. Place editor cursor at the beginning and click `Add Initial BPM`. This will place a temporary 1000 BPM change allowing you to scroll very precisely.
2. Place editor cursor at the first sound or note of the song, set `Number of Beats` to whatever you want (but preferably a multiple of the song's beats per measure), and click `Add Middle BPM`. This will change the temporary BPM change to the correct value and place another 1000 BPM change at your cursor.
3. If your song has an entirely consistent BPM, go to Step 4. If your song has a specific BPM change at a specific moment in time, move the cursor there, enter the number of beats between them, and press `Add Middle BPM` again. If your song has no consistent BPM, pick a small `Number of Beats` value and press `Add Middle BPM` for each set of beats.
4. Once you reach the end of the song, enter the number of beats one more time and press `Add Final BPM`. This does the same thing as `Add Middle BPM` but without placing a temporary BPM change at the cursor.
5. Go to Settings -> Audio -> Volume, turn on the Metronome, and listen to the song to see if it sounds right.
# In what ways is it not finished yet?
- Could probably use an `Adjust BPM` button that just changes nearby existing BPM changes such that the closest one gets moved to the cursor.
- BPM changes are not yet implemented as `Action`s, so you can't undo/redo them.
- `Number of Beats` box lets you type anything into it and doesn't stop typing from affecting other stuff (like selecting left/right notes, bombs, and walls).
- Button to open menu is just a blank sprite for now.
- UI is fugly.
