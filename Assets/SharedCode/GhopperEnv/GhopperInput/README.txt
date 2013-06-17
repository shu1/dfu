README
######
GhopperInput
Copyright Grasshopper NYC 2013
######
Integrate TUIO support (for table), FingerGestures, and InputMaster into your project. Makes input less painful
######

DEPENDENCIES:

- UtiliKit : Base classes for most of GhopperInput. (Already included if using GhopperEnv)

SETUP:

1. Add the GhopperInput prefab to the initial scene. (It is persistant across
scenes.) This prefab contains the InputMaster script, which handles the input
globally.

2. Add the InputCamera script to whatever cameras you want to use as input
cameras, typically the main camera & GUI camera. Then sent the "Input Raycast Layers" public variable to whatever layer you want to use as the raycast layer, i.e. the
layers on which you want to have ITouchable objects. Set a camera to GUI to have WorldTouch events suppressed when touching elements under it.

USAGE:

1. TO MAKE AN OBJECT TOUCHABLE: Add a script which implements the ITouchable
interface to that object. Make sure the object is the closest object to the
input camera (at least on the input camera's raycast layers). Implement
ITouchable's StartTouch, UpdateTouch, and EndTouch methods, which get called
when the object is touched.

2. TO POLL FOR TOUCH STATUS: Use InputMaster's static touches, or methods GetTouches and
GetTouch.

3. TO GET NOTIFIED WHEN WORLD TOUCHES HAPPEN: Register your event listeners
with InputMaster's static events WorldTouchStarted, WorldTouchUpdate,
WorldTouchEnded. These events happen for all touches not on GUI cameras (while seperateGUIInput is true)

TOUCH INFO:

This is sent with all ITouchable methods, GetTouch polls, and with touch events. It provides information about each touch, like screen position and time started. It also holds the result of the ray casts done for that touch. Most fields are self explanatory, but a couple of useful things are:
- startCam : Camera where touch started on
- screenRay & hitInfo : Last ray (in world) cast, and info about what it hit(if anything)
- touching : What object is being touched now. Null if ray casts hit nothing
- startedOver : What ITouchables(if any) touch started on
- int CastTouch(LayerMask, out RaycastInfo) : Redo the ray cast on different layer(with all inputCameras). Useful if you don't want touches to start on a certain layer, but want to see where a touch hits them late. Returns index of camera containing the hit, or -1 if no hit
- GetTouchOnPlaneContaining(Vector3) : Returns the world position of a touch, on the plane containing the given point and with a normal towards the startCamera. EG: If you give it the origin, and the camera points straight down, it will give you the world position of the touch on the ground plane, without worrying about ray casts.