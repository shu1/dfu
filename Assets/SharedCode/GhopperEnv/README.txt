README (GhopperEnv)


GhopperEnv provides core scripts & prefabs for Unity Grasshopper games, allowing the table to talk to your game & vice versa.


======
SETUP:
======

0. Activate the submodules
If you've cloned GhopperEnv into your project from Bitbucket, first make sure to init ('git submodule init') and update ('git submodule update') the submodules. If you don't, utilikit & input materials will be missing from your project, and your build probably won't work.

1. Set up your project
In Unity, from the menu bar, select: Grasshopper > Setup for Grasshopper.
(This configures various settings for the final build, like resolution & whether to display the default settings dialogue on startup.)

2. Add Core Grasshopper scripts to the scene
Add the GhopperEnv prefab to your scene. (This prefab is found in the root of the repo.)

3. Set up GrasshopperInput
For setting up GhopperInput, see its readme.


===========
SUBMODULES: (don't forget to init & update submodules after cloning!)
===========

- GhopperInput : Input-related scripts & prefabs for Unity. Also provides core C# OSC classes used in Daemon communications.

- UtiliKit : Assorted base classes & some debug stuff used throughout.


======
USAGE:
======

BASIC IMPLEMENTATION:

At a minimum, your game must (A) override GhopperEnv.OnReady, (B) call GhopperEnv.Show to start, and (C) call GhopperEnv.End to end. When you drag in the GhopperEnv prefab, the DefaultGameEnv script that comes attached will do the first two for you, so all *you* really need to do is C (call End).


GETTING PLAYER INFO:

1.
Wait for GhopperEnv.OnReady to be called. (This is called when the table has sent all starting info, which includes the player info.)
Note: 

2.
Call GhopperEnv.GetPlayers() to get an array of GhopperPlayers, which currently have accessor methods to get first/last/full name, color, angle (0-bottom), and id.


GETTING EVENTS FROM THE TABLE:

1.
Create a class that extends GhopperEnv. (You can just copy & paste DefaultGameEnv to use it as a template, if you want.)

2.
Override the virtual methods that represent events you care about (e.g. "OnReady"). These will be called when the matching event occurs.

3.
Replace the DefaultGameEnv script (attached to the GhopperEnv prefab) with the class you created.


CLOSING THE TABLE LOADER:

Call GhopperEnv.Show().

(The default GhopperEnv prefab will do this for you, courtesy of DefaultGameEnv.)


QUITTING / RETURNING TO THE SHELL:

Call GhopperEnv.End().


CUING TEXT OVERLAYS, TRIGGERING POP-UP ALERTS, & USING BEZEL "TEMPLATES":

See the Readme file in the Bezel directory.