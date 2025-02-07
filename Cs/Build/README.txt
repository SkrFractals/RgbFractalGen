Language Implementations:

RgbFractalGenCs
- The original c sharp version
- Relatively fast, and bugfree, and most likely to be the first to have new versions and features
- Easiest code

RgbFractalGenClr
- Direct rewrite from managed C sharp to managed CLI C++
- Slightly faster than the c sharp version
- The gif export might be slower, I might have to look into that
- Was harder to make threading right, but i think it's good now

RgbFrtactalGenCpp
- Rewritten the whole FractalGenerator class into pure unmanaged C++ in hopes to make it even faster
- However it's actually somehow slower
- And the threading tends to crash sometimes, and makes it significantly slower somehow
- Might need help from someone who knows how to do threads in unmanaged C++ correctly

---------------------------------------------------------------------------------------------------------

Settings (in reading direction from top left):

Fractal Select
- A dropdown list selection of different fractal definitions


Angle Definition Select:
- Different variants of child angle shifts
- Each one will rotate different selections of children, resulting in entirely different fractal structure


Color Definition Select:
- Different variants of child color shifts
- Each one will color shift different selections of children, resulting in entirely different color structure


Cutfunction Definition Select:
- Different variants of CutFunction implementations
- Each one will use a different algorithm to take the seed below and use it to cut different subchildren into voids

CutFunction Param Seed (textbox + slider):
- A binary seed for the CutFuntion
- Each power of two has one effect, sum of powers combines the effects
- For example with pentaflake: 1 cuts the outer child, 4 cuts the diagonály inner child, 5 cuts both


Resolution Width:
- Horizontal custom render resolution

Resolution Height:
- Vertical custom render resolution

Resolution Select
- Select resolution to render
- The second option is your custom resolution typed in the boxes to the left


Zoom Direction:
- Toggle between zooming in or out

Default Zoom:
- Beginning zoom, in counted frames
- Will prezoom the fractal in the beginning for this number of frames
- Useful for the "Only Image" option, not really significant for animations


Zoom Spin:
- Toggle the way the fractal spins as it zooms in, either no spin, clockwise/counterclockwise, or antispin (children in different directions)
- Beware, the antispin option can result in flashing overlapping luminosity in most fractals

Extra Spin:
- Adds as many symmetric rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this

Default Angle:
- Beginning angle, in counted frames
- Will prespin the fractal in the beginning for this number of degrees
- Useful for the "Only Image" option, not really significant for animations


Color Palette / Hue Cycle:
- RGB, BGR: flips the color shift in children, for example a child that turn to green from red would turn to blue and vice versa.
- X->Y: Starting with the X palette like above, but also hue cycles as they go deeper and/or in time

Extra hue cycling speed
- Adds as many full 160° hue rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this

Default Hue Angle:
- Beginning hue shift, in degrees (for example 120 will turn red to green)
- Will preshift the hue of the fractal in the beginning for this number of degrees
- Will result in completely permanently shifted colors for fractals that have the central child the same color as the parent


Void Ambient:
- The intensity of the grey light in the void areas (outside between the fractal dots)
- If 0 it will completely skip computing the void depth and rendering, good to render a bit faster thee fractals without void (for example sierpinski triangle and carpet).
- If -1 the exported GIF will have a transparent background

Void Noise:
- The intensity of the random noise within the grey void.
- If 0 it will skip initializing the random generator, slightly improving performance.
- Also 0 is recommended for HD gif you would want to convert into videos.


Super Saturate:
- Artificially boost the saturation of the fractal
- Useful for those that blend the children color too much so that they appear too gray, like the sierpinski triangle.


Detail:
- How small deep dots need to get before they are rendered
- Lower values are finer, take longer to generate, but might be grey
- Higher values can be more saturated, but can suffer from bad aliasing and the fractal animation flashing dark as there are not enough dots per pixel


Bloom:
- Will blur out the fractal light dots, making it brighter and more contrasting especially if very thin.
- But large values will also make it look very blurry with thick black borders, and will significantly slow down the generation!

Motion Blur:
- Will render multiple smear frames per frame, will take proportionally longer to render
- Not recommended for "Only Image", or if you want the animation super crisp even at the edges.


Brightness:
- The brightness of the main RGB fractal. In percentage.
- 100 is 100% max brightness. 300 is 3x overeposure over maximum.

Void Noise Scale:
- Scales up the noise in the void in bilinear fashion.
- Recommended for high resolutions, or if you want to compress into a video.


Parallelism Type:
- Of Animation Frames: Batches each animation frame to a different thread, recommended and even faster for animations, but no boost for "Only Image"
- Of Depth: Parallelize generation of each image at an ideal precomputed depth, making "Only Image" option a lot faster, but possible slightly corrupt like 1/1000000 pixels
- Of Recursion: Older version of "Of Depth", but just tries to parallelize each recursive call down until a depth. More buggy, and probably not as good as "Of Depth" 
- (as od now, OfDepth has been deprecated)

Max Threads:
- If you want to leave some resources for other task, you can reduce the maximum allowed number of parallel threads


Abort Delay:
- How long it takes for the generator to restart after you change something.

Delay:
- A delay between animation frames in hundredths of seconds
- Will not restart the generation unless you are Encoding a GIF


Frame Selection (<- and ->):
- Stops preview animation and toggles forward or backward between preview frames

Preview Animation:
- Toggles if the preview animates or pauses (the same toggle can also be done by clicking on the preview image itself)

RESTART
- Will restart the generator (only useful if something goes wrong or you have random settings enabled)


Generation Options:
- Only Image: Will only render one still image
- Animation RAM: Will render the animation without encoding a GIF, about 2x faster, but can't export the file afterwards
- Encode GIF: Wil lencode a GIF during the generation, so you could save it when finished.


Help:
- Displays this text, you might already know this though...

Save PNG:
- Saves the current displayed preview image frame into a PNG

Save GIF:
- Save the finished animation into a GIF
- Must have selected "Encode GIF" Generation Option above
- Technically the gif gets saved as a gifX.tmp file, and then only renamed and moved when you "Save" it.

---------------------------------------------------------------------------------------------------------

Nice fractal setting combinations:
-as I've combined all the definition choises to allow every combination, not every choise really is supposed to work with every choise of different definitions.
-so here are those that are supposed to work together
-any other combination is at your own "risk" of turning out looking bad
-R_X - rotation Definition
-C_X - color Definition
-F_X - CutFunction Definition

TriTree:
-R_BeamTree_Beams + F_NoBeam
-R_BeamTree_OuterJoint + F_OuterJoint
-R_BeamTree_InnerJoint + F_InnerJoint

TetraTriflake nice seeds:
NoChild: 10 12 13 14 21 48 49 53 56! 57! 58! 62 69 71 72 74 75 76 81 85 88 96 97 98! 99! 101 113 132 149! 157 164 172 176 177 179 192 209 224 225! 226!
	227 270 272 273 288 289 305 320 336 352 354 388 401 416! 480
RadHoles:
CornerHoles: 1 2 3 4 8 33 34 35 64 66 100
TriangleHoles: 3 7 8 12 16 24










