README (Bezel)


The Bezel portion of the Grasshopper focuses on controlling overlays that appear "above" the game and are for the game. These may be text overlays, alert messages, score-related overlays.



=====================
General Bezel Methods
=====================

CUING TEXT OVERLAYS:

For big text overlays, call GhopperEnv.bezel.Splat( text, angle, time ). Only one can be shown at a time--subsequent calls will queue.

For small text overlays, call GhopperEnv.bezel.Tell( text, angle, time ). Multiple small text overlays can be shown at a time.

In the calls above:
- 'text' is the string you want to display. Newline characters are supported.
- 'angle' is an integer representing the orientation of the text (note: 0 will oriented to the player sitting at the "bottom" of the table, most likely player 0. Depending the angle/orientation of your camera, this may be different from what you'd expect.)
- 'time' is a float representing the duration (in seconds) for the text to be visible.


TRIGGERING POP-UP ALERTS:

If you want to know when the alert is closed, call GhopperEnv.bezel.Alert( text, angle, callback ).

If you don't care, call GhopperEnv.bezel.Alert( text, angle ).

In the calls above:
- 'text' is the string you want to display. Newline characters are supported.
- 'angle' is an integer representing the orientation of the alert (see note in 'cuing text overlays', above, regarding orientation.)
- 'callback' is an optional callback that expects no arguments & returns nothing.



=====================
Using Bezel Templates
=====================

GENERAL INFO

A bezel template is a kind of game overlay, providing a consistent visual style & saving you the trouble of setting up common HUD. Bezel templates should be instantiated & loaded before configuring them, and should generally be configured before showing them. Each bezel template can be configured differently. 

Note 1: Only one template/instance can be shown at a time, but you can quickly switch between loaded instances by calling Show on the one you'd like to make active.

Note 2: Loading a template/instance may not be immediate, but you are free to make calls to configure/show/etc. in the meantime--these will be applied after loading is complete.


ADDING A TEMPLATE TO THE SCREEN (LOADING & SHOWING)

Every bezel template will need to be loaded & shown. (In general, you'll probably want to configure the instance before showing it, but that's up to you.)

1. Instantiate
PlayerScoreBezel myPlayerScoreBezel = new PlayerScoreBezel();

2. Load
GhopperEnv.bezel.LoadTemplate( myPlayerScoreBezel );

3. Show
myPlayerScoreBezel.Show();


SHOWING A TEMPLATE

myTemplate.Show()


HIDING A TEMPLATE

myTemplate.Hide()



======================
Using PlayerScoreBezel
======================

The PlayerScoreBezel provides a configurable strip for each player along the rim of the table. Each strip has a configurable background color, three small text fields ("Name," "Score," and "Info"), and an additional indicator to show whether or not the player is active.


SETUP:

1. After loading, just set the number of players. (Always do this first, before setting anything else.)
myPlayerScoreBezel.SetNumberOfPlayers( players.Count );


SETTING A PLAYER'S COLOR:

myPlayerScoreBezel.SetPlayerColor( playerNumber, Color.red );


SETTING A PLAYER'S NAME:

myPlayerScoreBezel.SetPlayerName( playerNumber, player.GetFirstName() );


SETTING A PLAYER'S SCORE:

myPlayerScoreBezel.SetPlayerScore( playerNumber, "100" );


SETTING A PLAYER'S "INFO":

myPlayerScoreBezel.SetPlayerInfo( playerNumber, "1st Place"  );


SHOWING WHETHER A PLAYER IS "ACTIVE":

myPlayerScoreBezel.SetPlayerActive( playerNumber, true );



