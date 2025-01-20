RgbFractalGenCs
-The original c sharp version
-relatively fast, and bugfree
-easiest code

RgbFractalGenClr
-Direct rewrite from managed C sharp to managed CLI C++
-slightly faster than the c sharp version
-the gif export might be slower, I might have to look into that
-Was harder to make threading right, but i think it's good now

RgbFrtactalGenCpp
-Rewritten the FractalGenerator class into pure unmanaged C++ in hopes to make it even faster
-however it's actually somehow slower
-and the threading tends to crash sometimes, and makes it significantly slower somehow
-might need help from someone who knows how to do threads in unmanaged C++ correctly

---------------------------------------------------------------------------------------------------------

Settings (in reading direction from top left):

Fractal Select
- a dropdown list selection of different fractal definitions


Angle Definition Select:
-different variants of child angle shifts
-each one will rotate different selections of children, resulting in entirely different fractal structure


Color Definition Select:
-different variants of child color shifts
-each one will color shift different selections of children, resulting in entirely different color structure


Cutfunction Definition Select:
-different variants of CutFunction implementations
-each one will use a different algorithm to take the seed below and use it to cut different subchildren into voids

CutFunction Param Seed (textbox + slider):
- a binary seed for the CutFuntion
- each power of two has one effect, sum of powers combines the effects
- for example with pentaflake: 1 cuts the outer child, 4 cuts the diagonály inner child, 5 cuts both


Resolution Width:
-horizontal render resolution

Resolution Height:
-vertical render resolution

Resolution Accept Checkbox:
-if unchecked the resolution will be 80x80 regardless of the text in the boxes to the left
-useful for toying with the settings and getting fast results, before you settle on full scale render


Zoom Direction:
-toggle between zooming in or out

Default Zoom:
-Beginning zoom, in counted frames
-Will prezoom the fractal in the beginning for this number of frames
-Useful for the "Only Image" option, not really significant for animations


Zoom Spin:
-toggle the way the fractal spins as it zooms in, either no spin, clockwise/counterclockwise, or antispin (children in different directions)
-beware, the antispin option can result in flashing overlapping luminosity in most fractals

Extra Spin:
-Adds as many symmetric rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this

Default Angle:
-Beginning angle, in counted frames
-Will prespin the fractal in the beginning for this number of degrees
-Useful for the "Only Image" option, not really significant for animations


Color Palette / Hue Cycle:
-RGB, BGR: flips the color shift in children, for example a child that turn to green from red would turn to blue and vice versa.
-X->Y: Starting with the X palette like above, but also hue cycles as they go deeper and/or in time

Extra hue cycling speed
-Adds as many full 160° hue rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this

Default Hue Angle:
-Beginning hue shift, in degrees (for example 120 will turn red to green)
-Will preshift the hue of the fractal in the beginning for this number of degrees
-Will result in completely permanently shifted colors for fractals that have the central child the same color as the parent


Void Ambient:
-The intensity of the grey light in the void areas (outside between the fractal dots)
-If 0 it will completely skip computing the void depth and rendering, good to render a bit faster thee fractals without void (for example sierpinski triangle and carpet).


Void Noise:
-The intensity of the random noise within the grey void.
-If 0 it will skip initializing the random generator, slightly improving performance.
-Also 0 is recommended for HD gif you would want to convert into videos.


Super Saturate:
-Artificially boost the saturation of the fractal
-Uesful for those that blend the children color too much so that they appear too gray, like the sierpinski triangle.


Detail:
-How small deep dots need to get before they are rendered
-Lower values are finer, take longer to generate, but might be grey
-Higher values can be more saturated, but can suffer from bad aliasing and the fractal animation flashing dark as there are not enough dots per pixel


Motion Blur:
-Will render multiple smear frames per frame, will take proportionally longer to render
-Not recommended for "Only Image", or if you want the animation super crisp even at the edges.


Parallel generation:
-Will enable threading, significantly boosting performance on multicore CPUs
-Will also make a lot more of your cores work hard, making your PC heat more, and leave less resources for other tasks

Parallelism Type:
-...Of Images: Parallelize generation of each image, making "Only Image" option a lot faster, but possible slightly corrupt like 1/1000000 pixels
-...Of Animation Frames: Batches each animation frame to a different thread, recommended and even faster for animations, but no boost for "Only Image"


Max Threads:
-If you want to leave some resources for other task, you can reduce the maximum allowed number of parallel threads


Delay:
-A delay between animation frames in hundredths of seconds
-Will not restart the generation unless you are Encoding a GIF


Frame Selection (<- and ->):
-stops preview animation and toggles forward or backward between preview frames

Preview Animation:
-toggless if the preview animates or pauses (the same toggle can also be done by clicking on the preview image itself)


Generation Options:
-Only Image: Will only render one still image
-Animation RAM: Will render the animation without encoding a GIF, about 2x faster, but can't export the file afterwards
-Encode GIF: Wil lencode a GIF during the generation, so you could save it when finished.

Help:
-displays this text

Save PNG:
-saves the current displayed preview image frame into a PNG

Save GIF:
-save the finished animation into a GIF
-must have selected "Encode GIF" Generation Option above
-technically the gif gets saved as a gifX.tmp file, and then only renamed and moved when you "Save" it.

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











