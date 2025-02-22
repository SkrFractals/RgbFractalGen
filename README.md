# RgbFractalGen
RGB Self-Similar Fractal GIF Loop Generator

---------------------------------------------------------------------------------------------------------
Known issues:

Missing Ffmpeg.exe
- The project now includes FFmpeg.exe for saving the animations as mp4, but the file is too large to fit on github
- so you'll need to download it and put it into the project directory yourself (Cs/RgbFractalGenCs/ffmpeg.exe)
- https://ffmpeg.org/download.html
- (or don't, the projectcan compile without it, but then you will get an error message "ffmpeg.exe not found" if you click the "Save Mp4" button)

General:
- Rare unexpected crashes/halts in generation

Clr:
- has some trouble encoding/saving GIFs

Cpp:
- only recently has somewhat started working, might need more testing

---------------------------------------------------------------------------------------------------------

- Exe can be found in the Build folder

Language Implementations:

RgbFractalGenCs
- The original c sharp version
- Relatively fast, and bugfree, and most likely to be the first to have new versions and features
- Easiest code

RgbFractalGenCpp
- Direct rewrite from managed C sharp to managed CLI C++ and pure C++
- It should be faster, but I'm not so sure about that yet, test have so far said it's a little slower
- It also ends to be a bit more unstable and buggy, since it's younger and C++ is harder to do perfectly
- FractalGenerator has managed CLR and pure C++ implementation in a single file, switchable with a CLR define directive

---------------------------------------------------------------------------------------------------------

Terminology:

- Parent-Child: A parent fractal is made of smaller versions of itself - it's children.
- Dot: The deepest smallest children that are only effectively points.
- CutFunction: A special function that determines to cut some specific children making more complex patterns.
- Period: The number of images it takes for an animation to match a child to a parent or vice versa.
- Palette: Sequence of colors that the fractal cycles through.
- Hue: Interpolated color between the ones on the palette.
- Void: Empty space where there are no fractal dots. Where the parent doesn't have children, or the CutFunction cut them off.
- Zoom: Scaling up and down.
- Spin: rotating around.
- Antispin: Children are rotating in opposite direction of their parents.
- Parallelism: Splitting the generation work between more CPU threads to improve generator speed.

----------------------------------------------------------------------------------------------------------

Settings:
- in reading direction from top left
- rows are separated by double new lines


Fractal Select:
- A dropdown list selection of different fractal definitions

EDIT:
- Switches between generator and editor mode


Angle Definition Select:
- Different variants of child angle shifts
- Each one will rotate different selections of children, resulting in entirely different fractal structure


Color Definition Select:
- Different variants of child color shifts
- Each one will color shift different selections of children, resulting in entirely different color structure


Cutfunction Definition Select:
- Different variants of CutFunction implementations
- Each one will use a different algorithm to take the seed below and use it to cut different subchildren into voids

CutFunction Param Seed:
- A binary seed for the CutFuntion
- Each power of two has one effect, sum of powers combines the effects
- For example with pentaflake: 1 cuts the outer child, 4 cuts the diagonály inner child, 5 cuts both
- Some CutFunction have multiple seeds with the same outcome, those get skipped so the binary sum might not always apply
- If -1 it will be random after each start


Resolution Width:
- Horizontal custom render resolution

Resolution Height:
- Vertical custom render resolution

Resolution Select
- Select resolution to render
- The second option is your custom resolution typed in the boxes to the left


Palette Select:
- Select the colors over which the fractals will keep cycling.
- If you select RGB and the Color Definitions with a 3/2 postfix, then each child that switches colors will step 1.5 steps
- So in RGB + 3/2 blue might become yellow. But Hue Shift would then cycle between Blue-Yellow, Red-Cyan and Green-Magenta
- If you instead select a Blue-Yellow palette and regular step Color Definitions, but a Hue Shift would only flip to Yellow-Blue

Delete Palette (X):
- Removes the selected palette from the list, erasing if custom, if it's one of the defaults, it will be restored next launch.

Add Palette (+):
- Make your own custom palette, keep picking colors until you are finished, then cancel the next pick.
- If you make a mistake you could always delete it and try again.
- It will be saved to settings.txt to be reloaded in the next launch, unless you delete it.

Default Hue:
- What hue shift will the fractal start at? With static Hue Shift, this default is permanent over the whole animation
- 1 would shift RGB to GBR, 2 would shift RGB to BRG, with a palette with X colors, an X default hue would loop back to the original
- If -1 it will be random after each start


Period
- The number of frames to zoom from a parent to a child or vice versa
- The final number of frames can be higher if the child is not identical to the parent, but some deeper child is

Period Multiplier:
- Multiplies the period length, while keeping the spins, but with the same period of spin and hue shifting stretched across the multiple
- So it effectively slows down the rotation and hue shifting


Zoom Direction:
- Toggle between zooming in or out

Default Zoom:
- Beginning zoom, in counted frames
- Will prezoom the fractal in the beginning for this number of frames
- Useful for the "Only Image" option, not really significant for animations
- If -1 it will be random after each start


Hue Shift:
- Static: The colors of each child will stay the same through the animation
- ->: The palettes will continuously shift forward in time, for example RGB would go GBR and then BRG and then back to RGB
- ->: tends to appear as if the colors are spilling out from children to parents (when usual small Children Colors steps)
- <-: Like -> but shifting backwards. RGB would become BGR, then GBR and then back to RGB
- <-: tends to appear as if the colors were retreating inside from the parents into children (when usual small Color Definition steps)
- If you select Color Definitions that use reverse color steps (that are larger than the number of colors in the palette, like 4 in RGB) then the hue shifts will appear to flow in opposite directions.
- If the pick combines both, then the colors will flow in both directions in these different parts.


Extra hue cycling speed
- Adds as many full 160° hue rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this


Zoom Spin:
- Toggle the way the fractal spins as it zooms in, either no spin, clockwise/counterclockwise, or antispin (children in different directions)
- Beware, the antispin option can result in flashing overlapping luminosity in most fractals

Default Angle:
- Beginning angle, in counted frames
- Will prespin the fractal in the beginning for this number of degrees
- Useful for the "Only Image" option, not really significant for animations
- If -1 it will be random after each start

Extra Spin Speed:
- Adds as many symmetric rotations to the loop as you type in. If your period is too long and spin too slow, you can speed it up like this


Void Ambient:
- The intensity of the grey light in the void areas (outside between the fractal dots)
- If 0 it will completely skip computing the void depth and rendering, good to render a bit faster thee fractals without void (for example sierpinski triangle and carpet).
- If -1 the exported GIF will have a transparent background


Void Noise:
- The intensity of the random noise within the grey void.
- If 0 it will skip initializing the random generator, slightly improving performance.
- Also 0 is recommended for HD gif you would want to convert into videos.


Void Noise Scale:
- Scales up the noise in the void in bilinear fashion.
- Recommended for high resolutions, or if you want to compress into a video.


Detail:
- How small deep dots need to get before they are rendered
- Lower values are finer, take longer to generate, but might be grey
- Higher values can be more saturated, but can suffer from bad aliasing and the fractal animation flashing dark as there are not enough dots per pixel


Super Saturate:
- Artificially boost the saturation of the fractal
- Useful for those that blend the children color too much so that they appear too gray, like the sierpinski triangle.


Brightness:
- The brightness of the main RGB fractal. In percentage.
- 100 is 100% max brightness. 300 is 3x overexposure over maximum.


Bloom:
- Will blur out the fractal light dots, making it brighter and more contrasting especially if very thin.
- But large values will also make it look very blurry with thick black borders, and will significantly slow down the generation!


Motion Blur:
- Will render multiple smear frames per frame, will take proportionally longer to render
- Not recommended for "Only Image", or if you want the animation super crisp even at the edges.


Zoom Child:
- Which child to zoom into? Default 0 is the center one
- Zooming into others is a new feature and might not work correctly in all cases yet, if you find any bugs please let me know


Parallelism Type:
- Of Animation Frames: Batches each animation frame to a different thread, recommended and even faster for animations, but no boost for "Only Image"
- Of Depth: Parallelize generation of each image at an ideal precomputed depth, making "Only Image" option a lot faster, but possible slightly corrupt like 1/1000000 pixels
- Of Recursion: Older version of "Of Depth", but just tries to parallelize each recursive call down until a depth. More buggy, and probably not as good as "Of Depth" 
- (as od now, OfDepth has been deprecated)

Max Threads:
- If you want to leave some resources for other task, you can reduce the maximum allowed number of parallel threads


Delay:
- A delay between animation frames in hundredths of seconds
- Used for GIF export (and preview)


Framerate:
- A framerate per second
- Used for MP4 export (and preview, whichever you set the last will update the preview delay/framerate)


Abort Delay:
- How long it takes for the generator to restart after you change something.


Frame Selection (<- and ->):
- Stops preview animation and toggles forward or backward between preview frames

Preview Animation:
- Toggles if the preview animates or pauses (the same toggle can also be done by clicking on the preview image itself)

RESTART
- Will restart the generator (only useful if something goes wrong or you have random settings enabled)


Generation Options:
- Only Image: Will only render one still image
- Animation RAM: Will render the animation without encoding a GIF, about 2x faster, but can't export the file afterwards
- Local GIF: Will encode a GIF during the generation, so you could save it when finished.
- Global GIF: Will encode a GIF while only analyzing the first frame's colors. Not recommended when shifting hues.
- Mp4: Will encode an Mp4. (not currently available)
- All Param: Will generate all the CutFunction seeds instead of a zoom.
- Hash Param: Like All param, but it's used to export all the unique seeds into a file.


Help:
- Displays this text, you might already know this though...

Export:
- Will export whatever you have selected in the select box to the right.

Export Select:
- Current PNG: Saves the current displayed preview image frame into a PNG
- GIF: Saves the finished animation to a GIF. You need to have LocalGIF, GlobalGIF, or AllSeeds generation type selected for this to be available. Technically the gif gets saved as a gifX.tmp file, and then only renamed and moved when you "Save" it.
- GIF->MP4: Convert the temporary or exported GIF into an MP4. You need to have LocalGIF, GlobalGIF, or AllSeeds generation type selected for this to be available. You can use it before of after saving the GIF.
- MP4: Will Export the animation as PNG series and covert those intohigh quality MP4. If you select MP4 generation type, it will export the PNGs faster in parallel.


Debug Log:
- Will show a state list of CPU threads and images.

---------------------------------------------------------------------------------------------------------

Nice fractal setting combinations:
-as I've combined all the definition choices to allow every combination, not every choice really is supposed to work with every choice of different definitions.
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
Symmetric: 10(In,Div) 12(In,Div) 13(In) 14(In!,Div) 21(In) 36(Div) 38(In) 48 49 53 56! 57! 58! 62 65(Div) 68(Div,In) 69(In) 70(In) 71 72 74 75 76 80(Div) 81 85 88 96 97 98(In!,Div) 99! 100(Div) 101(In) 113 132(In,Div) 149(In) 157 164(Div) 166(Div,In) 172(In) 173(In!) 174(In) 175(In) 176 177 179 180(Div) 192(Div,In) 196(Div) 209 224 225! 226! 227 270 272 273 288 289 305 320 336 352 354 388 401 416! 480
RadHoles: 4(Div) 8 12(Div,In) 17(Div,Out) 32 96
CornerHoles: 0 1 2(In,Div) 3! 4(Div,In) 8 33(In) 34(Div,In) 35 64 66 100
TriangleHoles: 3(In) 7 8(In) 12 16(Div,In) 24


---------------------------------------------------------------------------------------------------------

How it works:

The app has a fractal generator, that can put up to maxTasks threads to work, each will do one of the steps for one frame, each frame is also slightly zoomed/rotated to create the animation.
These steps to create the image are:

1. Generating Fractal Dots - Creates a large shape in the center and recursively splits it into smaller different colored shapes, until they are smaller than a pixel, and then it paints the color of those minishapes into the buffer.
2. Generating Dijkstra Void - All points in the buffer with any fractal colors in them, and all border points are assigned depth 0, and then a Dijkstra algorithm breadth first searches how deep (far away from the fractal) each pixel outside of the fractal is. The deeper, the lighter grey it will then be. It will also remember the maximum depth to normalize the grey levels.
3. Drawing Bitmap - Generates the void noise, and then line by line it draws the pixels into a bitmap, either the buffer when there are fractal dots, or the void grey with sampled noise when outside the fractal
4. Encoding GIF/MP4 - Encodes the image into a GIF or MP4 frame. This step is skipped if the encoding is not desired.
5. Writing - Writes the GIF/MP4 frame into the file. This step is skipped if the encoding is not desired. 

Image states:
1. Queued - no free task to start processing this image yet.
2. Generating Fractal Dots has started - step 1
3. Generating Dijkstra Void has started - step 2
4. Drawing Bitmap has started - step 3
5. Drawing Finished - step 3 is complete, waiting to start step 4
6. Unlocked RAM - encoding is not desired, so the bitmap got unlocked and is now available to see in the preview
7. Encoding - Encoding of GIF/MP4 has started - step 4
8. Encoding Finished - step 4 is complete, waiting to start step 5, or step 5 already started
9. Bitmap finished - step 5 is complete
10. Unlocked Encoded - everything is finished, so the bitmap got unlocked and is now available to see in the preview

Task states:
1. "Image state" - the task is performing the step the image state is associated with
2. Writing - the task is performing the step 5

