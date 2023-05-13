# StoredMomentum--2D-Platformer-Template

This project started off as a 2D Platformer game for a game jam but I ended up running out of time before I could get the game to a point where I wanted to submit it. Now, I've converted the project to a 2-D Platformer template which could serve as a general purpose tool for creating a platformer. So far, the project includes a working character controller that makes use of unity's built-in physics system. The controller has a variety of settings that can be tweaked to change the way controlling the player feels. 

One thing I do want to make clear is that the code written for the character controller was not made entirely off the top of my head.
Initially, I started off by basing the character controller of this repository: https://github.com/Dawnosaur/platformer-movement. I've since made a decent amount of changes, but you may see some similarities between certain sections of code/variable names. That being said, it's also possible that the repository linked above has also changed.

So far, the character controller supports the following features:
- Varying jump height based on how long the jump-key is held
- Walking on sloped surfaces
- Coyote Time: A sort of forgiveness feature that allows you to jump a specified time after walking off a ledge.
- Jump Buffering: If you press the jump key and you can't currently jump, you will jump when possible (in a specified time interval after the press)
- Physics interaction: Because the character controller is based on the unity physics system, you can interact with physics-based objects.

I'd also like to give credit to iHeartGameDev: https://www.youtube.com/@iHeartGameDev for his tutorials on state-machines, which I used to
help create the character controller statemachine.

Another note, the title for the game was originally called "Stored Momentum" because I wanted to create a platformer where you could
store the player's current momentum and then release it as though it were a force onto yourself or other objects. I created an early version of this
mechanic while working on the jam, but it was buggy and I felt the code was a bit unclean. Remanants of this code remain in the repository but I will likely
remove it, note it out, or move it into its own branch.

If you'd like to see some animations working with the character controller, check out the animation testing branch!
