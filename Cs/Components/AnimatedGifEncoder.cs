using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


#region .NET Disclaimer/Info
//===============================================================================
//
// gOODiDEA, uland.com
//===============================================================================
//
// $Header :		$  
// $Author :		$
// $Date   :		$
// $Revision:		$
// $History:		$  
//  
//===============================================================================
#endregion
#region Java
/**
 * Class AnimatedGifEncoder - Encodes a GIF file consisting of one or more frames.
 * Modification by Seeker - using byte* as frame input to save some array copying (useful with LockBits), and allowing FileStream again.
 * <pre>
 * Example:
 *    AnimatedGifEncoder e = new AnimatedGifEncoder();
 *    e.Start(outputFileName, width, height); // or with MemorySteam: e.Start(width, height);
 *    e.SetDelay(100);   // 1 frame per sec
 *    unsafe {
 *     // Image:
 *     var bmp1 = new Bitmap(width, height);
 *	   var locked1 = bitmaps[bitmapsGenerated].LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
 *	   byte* p1, imageBytes1 = p = (byte*)(void*)locked.Scan0;
 *	   for (var y = 0 y < height; ++y) for (var x = 0; x < width; ++x) {
 *	    p[0] = (byte)BLUE;    //  BLUE subPixel at x,y
 *		p[1] = (byte)GREEN;   // GREEN subPixel at x,y
 *		p[2] = (byte)RED;     //   RED subPizel at x,y
 *		p += 3;
 *	   }
 *     e.AddFrame(imageBytes1); // modification takes only directly byte* that you already have from LockBits, instead of Image+byte[]
 *     bmp1.UnlockBits(locked1);
 *     ...repeat from "Image:" for more frames 
 *    }
 *    e.Finish(outputFilename, true); // if outputFilename was supplied in Start or here, it will save a file, true closes the MemoryStream
 * </pre>
 * No copyright asserted on the source code of this class.  May be used
 * for any purpose, however, refer to the Unisys LZW patent for restrictions
 * on use of the associated LZWEncoder class.  Please forward any corrections
 * to kweiner@fmsware.com.
 *
 * @author Kevin Weiner, FM Software
 * @version 1.03 November 2003
 *
 */
#endregion
#region Modified By Phil Garcia
/* 
 * Modified by Phil Garcia (phil@thinkedge.com) 
	1. Added support to output the Gif to a MemoryStream (9/2/2005)
*/
#endregion
#region Modified By SkrFractals
/* 
 * Modified by SkrFractals (https://github.com/SkrFractals/RgbFractalGen)
 *  1. Returned support for direct filestream. Supply a filename in the Start function to use this feature, don't use Output if you are using this feature, that's only for the MemoryStream implementation.
 *  2. Added support for Parallelism (either making it's own Tasks, or letting you call AddFrame from multiple parallel tasks from the parent program), use AddFrameParallel instead of AddFrame for this feature
 *  3. Added support for a Cancellation token (useful if running in a thread or using the parallelism and you want to quickly cancel), use the overloaded functions with Cancellation token argument for this feature
 *  4. Added support for unsafe byte* pointer arrays as image input, yout can have these if you generate your own bitmaps pixel by pixel thorugh bitmap.LockBits. Keep it locked until the AddFrame task is finished!
 *  5. Added support for out of order AddFrame calls (DO NOT MIX this feature with the classic automatic ordered calls!), old ordered calls are used if you leave the frameIndex argument -1 or give other negative number 
 */
#endregion

namespace Gif.Components {

	/* Return information from TryWrite function */
	public enum TryWrite : byte {
		Failed = 0,             // Error has occured, and the file was aborted.
		Waiting = 1,            // Next frame was not written because it was either not supplied yet, or it's task is still running.
		FinishedFrame = 2,      // Next frame was successfully written into the stream
		FinishedAnimation = 3   // The stream was sucessfully finished
	}

	/* Return information from TryWrite function */
	public enum ColorTable : byte {
		Local = 0,      // Calculates a color table for each frame
		Global = 2,     // Calculates a single global color table for the whole animation at the end (cannot be parallelized)
		GlobalSingle = 3// Calculates a global color table only from the first frame (assuming the animation has the same colors for each frame)
	}
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class AnimatedGifEncoder {

		#region Variables
		//protected bool[] usedEntry = new bool[256]; // active palette entries, removed because it looks usused to me, ws only written to as one point and never read

		protected int len;                  // byte length (3 * width * height)
		protected int width;                // image width
		protected int height;               // image height
		protected Color
			transparent = Color.Empty;      // transparent color if given
		protected int repeat = -1;          // no repeat
		protected int delay = 0;            // frame delay (hundredths)
		protected bool started = false;     // ready to output frames
		protected MemoryStream ms = null;   // Phil Garcia's memory stream solution

		protected int colorDepth = 8;       // number of bit planes
		protected int palSize = 7;          // color table size (bits-1)
		protected int dispose = -1;         // disposal code (-1 = use default)
		protected bool firstFrame = true;   // is the header of the file not written yet?
		protected int sample = 10;          // default sample interval for quantizer


		// SkrFractals' additions:
		protected EncoderTaskData encodeTaskData;   // The next task struct to add encoding task to
		protected EncoderTaskData writeTaskData;    // The next task structure that hasn't been written to a file yet
		protected FileStream fs = null;             // original file stream solution
		protected Stream stream = null;             // either fs or ms, whichever the user chose to open
		ColorTable table;                           // How to train and write the colortables?
		protected string filename;                  // remember the filename because the Finish will not write it yet, because it must wait for TryWriteFrameIntoFile to finish
		protected bool closeStream;                 // should the stream get closed when finishing?
		protected bool finishedAnimation;           // has Finish been called, then all tasks been finished and the file written?
		protected int finishedFrame;                // how many frames tasks have finished (you can unlock the bits or pixelPtr up until this point)
		protected int addedFrames;
		protected byte[] globalTab = null;          // global colormap neuquant
		protected NeuQuant globalNq = null;			// global NeuQuant

		public bool IsFinished() => finishedAnimation;
		public int FinishedFrame() => finishedFrame;
		public MemoryStream Output() => ms;
		#endregion

		#region Settings
		/**
		 * Sets the delay time between each frame, or changes it for subsequent frames (applies to last frame added).
		 *
		 * @param ms int delay time in hundredths
		 */
		public void SetDelay(int hundredths) {
			delay = hundredths;
		}

		/**
		 * Sets the GIF frame disposal code for the last added frame and any subsequent frames.
		 * Default is 0 if no transparent color has been set, otherwise 2.
		 * 
		 * @param code int disposal code.
		 */
		public void SetDispose(int code) {
			if (code >= 0)
				dispose = code;
		}

		/**
		 * Sets the number of times the set of GIF frames should be played.
		 * Default is 1; 0 means play indefinitely.
		 * Must be invoked before the first image is added. 
		 *
		 * @param iter int number of iterations.
		 * @return
		 */
		public void SetRepeat(int iter) {
			if (iter >= 0)
				repeat = iter;
		}

		/**
		 * Sets frame rate in frames per second.  Equivalent to
		 * <code>setDelay(1000/fps)</code>.
		 *
		 * @param fps float frame rate (frames per second)
		 */
		public void SetFrameRate(float fps) {
			if (fps != 0f)
				delay = (int)Math.Round(100f / fps);
		}

		/**
		 * Sets quality of color quantization (conversion of images
		 * to the maximum 256 colors allowed by the GIF specification).
		 * Lower values (minimum = 1) produce better colors, but slow
		 * processing significantly.  10 is the default, and produces
		 * good color mapping at reasonable speeds.  Values greater
		 * than 20 do not yield significant improvements in speed.
		 *
		 * @param quality int greater than 0.
		 * @return
		 */
		public void SetQuality(int quality) {
			sample = Math.Max(1, quality);
		}

		/**
		 * Sets the transparent color for the last added frame
		 * and any subsequent frames.
		 * Since all colors are subject to modification
		 * in the quantization process, the color in the final
		 * palette for each frame closest to the given color
		 * becomes the transparent color for that frame.
		 * May be set to null to indicate no transparent color.
		 *
		 * @param c Color to be treated as transparent on display.
		 */
		public void SetTransparent(Color c) {
			transparent = c;
		}
		#endregion

		#region Commands
		/**
		 * Initiates GIF file creation on the given stream. The stream is not closed automatically.
		 *
		 * @param os OutputStream on which GIF images are written.
		 * @param w Width
		 * @param h Height
		 * @param colortable Local-analyze each frame, Global-analyze all frames together, GlobalSingle-analyze only the first frame
		 * @return false if initial write failed.
		 */
		public bool Start(Stream os, int w, int h, ColorTable colortable = ColorTable.Local) {
			len = (width = w) * (height = h) * 3;
			if (os == null)
				return false;
			table = colortable;
			bool ok = true;
			stream = os;
			try {
				WriteString("GIF89a"); // header
			} catch (IOException) {
				ok = Abort();
			}

			// i will create the queue for task data here:
			// encodeTaskData = the last taskdata that is not yet used, which i will give the next NeuQuant
			// writeTaskData = the one that I haven't written into file yet
			encodeTaskData = writeTaskData = new EncoderTaskData();
			addedFrames = finishedFrame = 0;
			finishedAnimation = false;
			return started = ok;
		}

		/**
		 * If file == "" - initiates writing of a GIF file to a memory stream.
		 * otherwise  - initiates writing to a filestream of a GIF file with the specified name.
		 *
		 * @param w Width
		 * @param h Height
		 * @param file-filename, opens a memorystream instaed if empty string
		 * @param colortable Local-analyze each frame, Global-analyze all frames together, GlobalSingle-analyze only the first frame
		 * @return false if open or initial write failed.
		 */
		public bool Start(int w, int h, string file = "", ColorTable colortable = ColorTable.Local) {
			bool ok;
			try {
				if (file == "") {
					fs = null;
					ok = Start(ms = new MemoryStream(10 * 1024), w, h, colortable);
				} else {
					ms = null;
					ok = Start(fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None), w, h, colortable);
				}
			} catch (IOException) {
				ok = Abort();
			}
			return ok;
		}

		/* Call at various times after adding frames, it will attempt to write the next ready frame to a file.
		 * Keep calling until you get a Failed or FinishedAnimation, if you locked Bitmap bits and got FinishedFrame, you can unlock the bits of that bitmap (finishedFrame before the call, or finishedFrame-1 after the call)
		 * 
		 * @param parallel set this to true if you are calling this from parallel threads, it will set a monitor lock to preserve data integrity
		 * @return Failed if error, Waiting if next frame task still running or next frame not added yet, FinishedFrame if successfully written next frame to a file, FinishedAnimation if file is finished
		 */
		public TryWrite TryWrite(bool parallel = false) {
			TryWrite r;
			if (parallel) {
				Monitor.Enter(this);
				try {
				r = TryWriteInternal();
				if (r == Components.TryWrite.Failed)
					Abort();
				}finally { Monitor.Exit(this); }
				return r;
			}
			r = TryWriteInternal();
			if (r == Components.TryWrite.Failed)
				Abort();
			return r;
		}
		/**Finished the next encodeTask without assiging a new next one.
		 * This will let the TryWriteFrameIntoFile task finisher know we have finished adding frames and it will then flush any pending data and close output file.
		 * If writing to an OutputStream, the stream is not closed.
		 */
		public bool Finish(CancellationToken token, string filename = "", bool closeStream = true) {
			if (!started)
				return Abort();
			this.filename = filename;
			this.closeStream = closeStream;
			if (table != ColorTable.Local && GetGlobalMap(token))
				return Abort();
			// this will let me know the encodeTaskData (whcih hasn't got started a Task yet, so it's next is null) is just marking the need of the file
			encodeTaskData.finished = true;
			// try a final write (if not parallel)
			while (writeTaskData.finished && writeTaskData.nextTask != null)
				TryWrite();
			return true;
		}
		/**Finished the next encodeTask without assiging a new next one.
		 * This will let the TryWriteFrameIntoFile task finisher know we have finished adding frames and it will then flush any pending data and close output file.
		 * If writing to an OutputStream, the stream is not closed.
		 */
		public bool Finish(string filename = "", bool closeStream = true) {
			if (!started)
				return Abort();
			this.filename = filename;
			this.closeStream = closeStream;
			if (table != ColorTable.Local && GetGlobalMap())
				return Abort();
			// this will let me know the encodeTaskData (whcih hasn't got started a Task yet, so it's next is null) is just marking the need of the file
			encodeTaskData.finished = true;
			// try a final write (if not parallel)
			while(writeTaskData.finished && writeTaskData.nextTask != null)
				TryWrite();
			return true;
		}
		#endregion

		#region AddFrame_Overloads
		/**
		 * Adds next GIF frame.
		 * 
		 * @param Image im - image of the frame to write
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrame(System.Drawing.Image im, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return UntaskedFrame(encodeData, first, token);
		}
		/**
		 * Adds next GIF frame.
		 * 
		 * @param Image im - image of the frame to write
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrame(System.Drawing.Image im, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return UntaskedFrame(encodeData, first);
		}
		/**
		 * Adds next GIF frame.
		 * 
		 * @param byte[] pixels - a byte array of image pixels containing frame to write.
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrame(byte[] pixels, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsArr = pixels; // was this.pixels, moved that into the taskData
			return UntaskedFrame(encodeData, first);
		}
		/**
		 * Adds next GIF frame - cancellable.
		 * 
		 * @param byte[] pixels - a byte array of image pixels containing frame to write.
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrame(byte[] pixels, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsArr = pixels; // was this.pixels, moved that into the taskData
			return UntaskedFrame(encodeData, first, token);
		}
		/**
		 * Adds next GIF frame.
		 * 
		 * @param byte* pixels - a byte pointer array of image pixels containing frame to write.
		 * @param int stride - the number of bytes per scan line (usually 3 * width for BGR, but some widths have few unused bytes at the end, so you should ask BitmapData.Stride)
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public unsafe bool AddFrame(byte* pixels, int stride, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsPtr = pixels; // was this.pixels, moved that into the taskData
			encodeData.stride = stride;
			return UntaskedFrame(encodeData, first);
		}
		/**
		 * Adds next GIF frame - cancellable.
		 * 
		 * @param byte* pixels - a byte pointer array of image pixels containing frame to write.
		 * @param int stride - the number of bytes per scan line (usually 3 * width for BGR, but some widths have few unused bytes at the end, so you should ask BitmapData.Stride)
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public unsafe bool AddFrame(byte* pixels, int stride, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsPtr = pixels; // was this.pixels, moved that into the taskData
			encodeData.stride = stride;
			return UntaskedFrame(encodeData, first, token);
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		 * @param Image im - image of the frame to write
		 * @param Task task - if not null, it will start its own task and write it into the reference, otherwise it should be already ran in parallel task from the program above
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(System.Drawing.Image im, ref Task task, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return MakeFrameTask(encodeData, first, ref task, token); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		 * @param Image im - image of the frame to write
		 * @param Task task - if not null, it will start its own task and write it into the reference, otherwise it should be already ran in parallel task from the program above
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(System.Drawing.Image im, ref Task task, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return MakeFrameTask(encodeData, first, ref task); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		 * @param Image im - image of the frame to write
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(System.Drawing.Image im, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return AddFrameTask(encodeData, first, token); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		* @param Image im - image of the frame to write
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(System.Drawing.Image im, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			SetEncodeBitmap(encodeData, im);
			return AddFrameTask(encodeData, first); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame can be called in parallel, so the previous one might not have been finished yet.
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 * 
		 * @param byte[] pixels - a byte array of image pixels containing frame to write.
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(byte[] pixels, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsArr = pixels; // was this.pixels, moved that into the taskData
			return AddFrameTask(encodeData, first, token); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		 * @param byte[] pixels - a byte array of image pixels containing frame to write.
		 * @param Task task - if not null, it will start its own task and write it into the reference, otherwise it should be already ran in parallel task from the program above
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		public bool AddFrameParallel(byte[] pixels, ref Task task, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsArr = pixels; // was this.pixels, moved that into the taskData
			return MakeFrameTask(encodeData, first, ref task, token); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame can be called in parallel, so the previous one might not have been finished yet.
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 * 
		 * @param byte* pixels - a byte array of image pixels containing frame to write. (from LockBits)
		 * @param int stride - the number of bytes per scan line (usually 3 * width for BGR, but some widths have few unused bytes at the end, so you should ask BitmapData.Stride)
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		unsafe public bool AddFrameParallel(byte* pixels, int stride, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsPtr = pixels; // was this.pixels, moved that into the taskData
			encodeData.stride = stride;
			return AddFrameTask(encodeData, first, token); // here it normally continued with the code for writing to file, but i have moved that to TryWriteFrameIntoFile where it will wait for the first next task to complete
		}
		/**
		 * Adds next GIF frame.
		 * The frame is not written immediately, because this AddFrame will create a parallel task that will not be finished immediately
		 * Make sure to finish writing the animation with TryWriteFrameIntoFile after
		 *
		 * @param byte* pixels - a byte pointer array of image pixels containing frame to write. (from LockBits)
		 * @param int stride - the number of bytes per scan line (usually 3 * width for BGR, but some widths have few unused bytes at the end, so you should ask BitmapData.Stride)
		 * @param Task task - if not null, it will start its own task and write it into the reference, otherwise it should be already ran in parallel task from the program above
		 * @param CancellationToken token - a token that can trigger a cancell from outside.
		 * @param frameIndex - leave empty -1 if you are adding frames sequentially, or input the frameIndex (from 0 to images-1) if you are adding out of order. DO NOT MIX -1 and out of order for the same gif! 
		 * @return true if successful.
		 */
		unsafe public bool AddFrameParallel(byte* pixels, int stride, ref Task task, CancellationToken token, int frameIndex = -1) {
			var encodeData = NextEncodeData(frameIndex, out var first);
			if (encodeData == null)
				return Abort();
			encodeData.pixelsPtr = pixels; // was this.pixels, moved that into the taskData
			encodeData.stride = stride;
			return MakeFrameTask(encodeData, first, ref task, token);
		}
		#endregion

		#region Analyze
		/**
		 * Analyzes image colors and creates color map.
		 */
		unsafe protected bool AnalyzePixels(EncoderTaskData encodeData) {

			// initialize quantizer
			// each instance of NeuQuant will write the data into its own EncoderTaskData
			// so it can be remembered for the later sequential write
			encodeData.nq = encodeData.pixelsPtr == null
				? new NeuQuant(encodeData.pixelsArr, len, sample, width * 3)
				: new NeuQuant(encodeData.pixelsPtr, encodeData.stride, len, sample, width * 3);
			encodeData.colorTab = encodeData.nq.Process(); // create reduced palette
			return false;
		}

		/**
		 * Analyzes image colors and creates color map.
		 */
		unsafe protected bool AnalyzePixelsGlobal(EncoderTaskData encodeData) {

			// initialize quantizer
			// each instance of NeuQuant will write the data into its own EncoderTaskData
			// so it can be remembered for the later sequential write
			encodeData.nq = new NeuQuant(encodeData, len, sample, width * 3);
			encodeData.colorTab = encodeData.nq.Process(); // create reduced palette
			return false;
		}

		/**
		 * Imdexes pixels with a colormap
		 */
		unsafe protected bool GetIndexedPixels(EncoderTaskData encodeData, NeuQuant nq) {
			encodeData.indexedPixels = new byte[len / 3];
			// I didn't find any use of this usedEntry variable anywhere else in the code
			// it only gets set here, and never set or read anywhere else, so I think I could just get rid of it...?
			byte* p = encodeData.pixelsPtr;

			if (p == null) {
				byte[] pa = encodeData.pixelsArr;
				for (int x = width * height, i = 0, pi = 0; x > 0; --x)
					//usedEntry[
					encodeData.indexedPixels[i++] = (byte)nq.Map(pa[pi++], pa[pi++], pa[pi++]);
					//] = true;
			} else {
				for (int y = 0, i = 0; y < height; ++y) {
					byte* pi = p + encodeData.stride * y;
					for (int x = 0; x < width; ++x, pi += 3)
						//usedEntry[
						encodeData.indexedPixels[i++] = (byte)nq.Map(pi[0], pi[1], pi[2]);
					//] = true;
				}
			}

			//colorDepth = 8; // Thiso only gets set to 8 here, never to anythign else, so I guess I could just this out and set it in constructor and not worry about paralellizing this, and could delte it here
			//palSize = 7; // This one as well, I will also just set it in constructor, and delete this line here

			// get closest match to transparent color if specified
			if (transparent != Color.Empty)
				encodeData.transIndex = nq.Map(transparent.B, transparent.G, transparent.R);
			if (encodeData.bitmapData != null)
				encodeData.bitmap.UnlockBits(encodeData.bitmapData); // unlock bits if we were adding a direct image
			encodeData.finished = true; // lets the TryWriteFrameIntoFile know this task was finished so it can write the data into the file
			return false;
		}

		/**
		 * Analyzes image colors and creates color map.
		 */
		unsafe protected bool AnalyzePixels(EncoderTaskData encodeData, CancellationToken token) {

			// initialize quantizer
			// So I will create a pointer queue list of EncoderTaskData
			// each instance of NeuQuant will then write the data into its own EncoderTaskData
			// so it can be remembered for the later sequential write
			encodeData.nq = encodeData.pixelsPtr == null
				? new NeuQuant(encodeData.pixelsArr, len, sample, width * 3, token)
				: new NeuQuant(encodeData.pixelsPtr, encodeData.stride, len, sample, width * 3, token);
			encodeData.colorTab = encodeData.nq.Process(token); // create reduced palette
			return token.IsCancellationRequested;
		}
		/**
		 * Analyzes image colors and creates color map.
		 */
		unsafe protected bool AnalyzePixelsGlobal(EncoderTaskData encodeData, CancellationToken token) {
			// initialize quantizer
			// each instance of NeuQuant will write the data into its own EncoderTaskData
			// so it can be remembered for the later sequential write
			encodeData.nq = new NeuQuant(encodeData, len, sample, width * 3);
			encodeData.colorTab = encodeData.nq.Process(token); // create reduced palette
			return token.IsCancellationRequested;
		}
		/**
		 * Imdexes pixels with a colormap
		 */
		unsafe protected bool GetIndexedPixels(EncoderTaskData encodeData, NeuQuant nq, CancellationToken token) {
			encodeData.indexedPixels = new byte[len / 3];
			// I didn't find any use of this usedEntry variable anywhere else in the code
			// it only gets set here, and never set or read anywhere else, so I think I could just get rid of it...?
			byte* p = encodeData.pixelsPtr;
			if (p == null) {
				byte[] pa = encodeData.pixelsArr;
				for (int y = 0, i = 0, pi = 0; y < height; ++y) {
					if (token.IsCancellationRequested)
						return true;
					for (int x = 0; x < width; ++x)
						//usedEntry[
						encodeData.indexedPixels[i++] = (byte)nq.Map(pa[pi++], pa[pi++], pa[pi++]);
					//] = true;
				}
			} else {
				for (int y = 0, i = 0; y < height; ++y) {
					if (token.IsCancellationRequested)
						return true;
					byte* pi = p + encodeData.stride * y;
					for (int x = 0; x < width; ++x, pi += 3)
						//usedEntry[
						encodeData.indexedPixels[i++] = (byte)nq.Map(pi[0], pi[1], pi[2]);
					//] = true;
				}
			}

			//colorDepth = 8; // Thiso only gets set to 8 here, never to anythign else, so I guess I could just this out and set it in constructor and not worry about paralellizing this, and could delte it here
			//palSize = 7; // This one as well, I will also just set it in constructor, and delete this line here

			// get closest match to transparent color if specified
			if (transparent != Color.Empty)
				encodeData.transIndex = nq.Map(transparent.B, transparent.G, transparent.R);
			if (encodeData.bitmapData != null)
				encodeData.bitmap.UnlockBits(encodeData.bitmapData); // unlock bits if we were adding a direct image
			encodeData.finished = true; // lets the TryWriteFrameIntoFile know this task was finished so it can write the data into the file
			return false;
		}

		#endregion

		#region Tasks
		/** Prepare the next Task - is monitor locked to ensure the integrity of the Task list */
		protected EncoderTaskData NextEncodeData(int index, out bool first) {
			EncoderTaskData encodeTask = null;
			Monitor.Enter(this);
			try {
				first = addedFrames == 0;
				if (!started || (encodeTask = encodeTaskData).failed)
					return null;
				encodeTaskData = encodeTask.nextTask = new EncoderTaskData(); // make a next one for the next call of Addframe to work on
			encodeTask.frameIndex = index < 0 ? addedFrames++ : index;
			} finally { Monitor.Exit(this); }
			return encodeTask;
		}
		/** Just analyze pixels without parallel tasks */
		private bool UntaskedFrame(EncoderTaskData encodeTask, bool first) {
			switch (table) {
				case ColorTable.Local:
					encodeTask.failed = AnalyzePixels(encodeTask) || GetIndexedPixels(encodeTask, encodeTask.nq);
					break;
				case ColorTable.Global: return encodeTask.finished = true;
				case ColorTable.GlobalSingle:
					if (!first)
						return encodeTask.finished = true;
					encodeTask.failed = GetGlobalMap();
					break;
			}
			while (TryWrite() == Components.TryWrite.FinishedFrame) ;
			return Abort(encodeTask.failed || TryWrite() == Components.TryWrite.Failed);
		}
		/** Just analyze pixels without parallel tasks - cancellable */
		private bool UntaskedFrame(EncoderTaskData encodeTask, bool first, CancellationToken token) {
			switch (table) {
				case ColorTable.Local:
					encodeTask.failed = AnalyzePixels(encodeTask, token) || GetIndexedPixels(encodeTask, encodeTask.nq, token);
					break;
				case ColorTable.Global: return encodeTask.finished = true;
				case ColorTable.GlobalSingle:
					if(!first)
						return encodeTask.finished = true;
					encodeTask.failed = GetGlobalMap(token);
					break;
			}
			while (TryWrite() == Components.TryWrite.FinishedFrame) ;
			return Abort(encodeTask.failed || TryWrite() == Components.TryWrite.Failed);
		}
		/** Make a task when called Parallel will supplied Task reference to assign the new Task to - cancellable */
		protected bool MakeFrameTask(EncoderTaskData encodeTask, bool first, ref Task task, CancellationToken token) {
			switch (table) {
				case ColorTable.Local:
					task = encodeTask.thisTask = Task.Run(() => encodeTask.failed = AnalyzePixels(encodeTask, token) || GetIndexedPixels(encodeTask, encodeTask.nq, token), token);
					break;
				case ColorTable.Global: return encodeTask.finished = true;
				case ColorTable.GlobalSingle:
					if (!first)
						return encodeTask.finished = true;
					task = encodeTask.thisTask = Task.Run(() => encodeTask.failed = GetGlobalMap(token), token);
					break;
			}
			return true;
		}
		/** Add task when called Parallel from outside parallel thread - cancellable */
		protected bool AddFrameTask(EncoderTaskData encodeTask, bool first, CancellationToken token) => table switch {
			ColorTable.Local => Abort(encodeTask.failed = AnalyzePixels(encodeTask, token) || GetIndexedPixels(encodeTask, encodeTask.nq, token)),
			ColorTable.GlobalSingle => first ? Abort(encodeTask.failed = GetGlobalMap(token)) : encodeTask.finished = true,
			_ => encodeTask.finished = true,
		};
		/** Make a task when called Parallel will supplied Task reference to assign the new Task to */
		protected bool MakeFrameTask(EncoderTaskData encodeTask, bool first, ref Task task) {
			switch (table) {
				case ColorTable.Local:
					task = encodeTask.thisTask = Task.Run(() => encodeTask.failed = AnalyzePixels(encodeTask) || GetIndexedPixels(encodeTask, encodeTask.nq));
					break;
				case ColorTable.Global: return encodeTask.finished = true;
				case ColorTable.GlobalSingle:
					if (!first)
						return encodeTask.finished = true;
					task = encodeTask.thisTask = Task.Run(() => encodeTask.failed = GetGlobalMap());
					break;
			}
			return true;
		}
		/** Add task when called Parallel from outside parallel thread */
		protected bool AddFrameTask(EncoderTaskData encodeTask, bool first) => table switch {
			ColorTable.Local => Abort(encodeTask.failed = AnalyzePixels(encodeTask) || GetIndexedPixels(encodeTask, encodeTask.nq)),
			ColorTable.GlobalSingle => first ? Abort(encodeTask.failed = GetGlobalMap()) : encodeTask.finished = true,
			_ => encodeTask.finished = true,
		};
		/** Add bitmap to the task */
		protected void SetEncodeBitmap(EncoderTaskData encodeTask, Image im) {
			encodeTask.bitmapData = (encodeTask.bitmap = (Bitmap)im).LockBits(new Rectangle(0, 0, im.Width, im.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
		}
		/**Initiates writing of a GIF file with the specified name.
		 * Call this if you are using a Memory stream, after out called Finish and verified the finish with TryWriteFrameIntoFile
		 *
		 * @return false if open or initial write failed.
		 */
		protected bool Output(string file) {
			if (!finishedAnimation)
				return Abort();
			try {
				fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				fs.Write(ms.ToArray(), 0, (int)ms.Length);
				fs.Close();
				fs = null;
			} catch (IOException) {
				return Abort();
			}
			return true;
		}
		/* Call for aborting the strem if something failed
		 * 
		 * @param fail if false it will not abort and will return true
		 * @return ok - true if not aborted, false if aborted
		 */
		public bool Abort(bool fail = true) {
			if (!fail)
				return true;
			started = false;
			ms?.Close();
			fs?.Close();
			if (fs != null && filename != null && filename != "")
				File.Delete(filename);
			fs = null;
			ms = null;
			finishedAnimation = false;
			addedFrames = finishedFrame = 0;
			encodeTaskData = writeTaskData = null;
			return false;
		}
		protected TryWrite TryWriteInternal() {
			if (finishedAnimation)
				return Components.TryWrite.FinishedAnimation; // already finished
			if (!started || writeTaskData.failed)
				return Components.TryWrite.Failed;  // it has failed or cancelled or not started yet, so i will need to cancel the wlo file writing as well
			var writeTask = writeTaskData; // here I am following the code after AnalyzePixels, anytime when the task of the sequential next frame is finished:
			if (writeTask == null)
				return Components.TryWrite.Waiting; // no frames yet
			if (globalTab == null && writeTask.nextTask != null && table != ColorTable.Local)
				return Components.TryWrite.Waiting; // global map needs to call a finish before trywrites, or global single need to finish making the map
			var ok = true;
			EncoderTaskData prevWrite = null;
			while (writeTask.frameIndex > finishedFrame) {
				if (writeTask.nextTask == null) {
					return writeTask.finished
						? Components.TryWrite.Failed    // You must have called finish while having gaps in supplied out of order frames!
						: Components.TryWrite.Waiting;  // You have not supplied the next out of order frame yet, so the Task has not even started running yet.
				}
				writeTask = (prevWrite = writeTask).nextTask;
			}
			if(writeTask == null)
				return Components.TryWrite.Failed; // This should never happen, but I'm gonna check it anyway. (the last taskdata should always have frameIndex -1, so the while should not go to nullptr from there)
			if (!writeTask.finished)
				return Components.TryWrite.Waiting; // the task for the next sequential frame is not finished yet, try again later
			if (writeTask.nextTask == null) {
				// i always assign to next right before i start its task, so if i set the finished flag without starting a task, that means i have called Finish and there's no next frame
				// The finish normally closed the filestream, but not I will do it here
				// and the next Finish just sets the encodeTaskData.finished = true to trigger this code when all frames wating to be written have been written
				started = false;
				addedFrames = 0;
				try { // Try writing the file end
					stream.WriteByte(0x3b); // gif trailer
					stream.Flush();
					fs?.Close();
					if (ms != null && filename != "")
						ok = Output(filename);
					if (closeStream)
						ms?.Close();
				} catch (Exception) {
					ok = false;
				}
				finishedAnimation = firstFrame = true;
				return ok ? Components.TryWrite.FinishedAnimation : Components.TryWrite.Failed; // Has writing the full file finished or failed?
			}
			// this nextTask is not null, so this task is actually a frame and not an end yet, so write that frame into the file
			if (writeTask.frameIndex < finishedFrame) {
				// You must have messed up supplying the frameIndexes! The should be a queued frame with an index less than what has alraedy been written!
				// This has probablty happened because you either mixed the automatic ordered, and out of order AddFrame.
				// Or you messed out the out of order frame counting and supplied the same index twice.
				return Components.TryWrite.Failed;
			}
			writeTask.thisTask?.Wait(); // join the task if if was created here
			if (table != ColorTable.Local && GetIndexedPixels(writeTask, globalNq))
				return Components.TryWrite.Failed; // failed to make indexes pixels from a global color map
			try { // try writing the frame
				  // so now we know the data for the next frame is ready to write, so let's do it!
				if (firstFrame) {
					WriteLSD(); // logical screen descriptior
					WritePalette(table == ColorTable.Local ? writeTask.colorTab : globalTab); // global color table
					if (repeat >= 0)
						WriteNetscapeExt();  // use NS app extension to indicate reps
				}
				WriteGraphicCtrlExt(writeTask); // write graphic control extension
				bool global = firstFrame || table != ColorTable.Local;
				WriteImageDesc(global); // image descriptor
				if (!global)
					WritePalette(writeTask.colorTab); // local color table
				WritePixels(writeTask); // encode and write pixel data
				++finishedFrame;
			} catch (Exception) {
				ok = false;
			}
			firstFrame = false;
			if (prevWrite == null) {
				if (writeTaskData == null) 
					return Components.TryWrite.Failed;
				// Now that I wrote the frame, I can forget the task data, and move on to the next one.
				writeTaskData = writeTaskData.nextTask;
			} else {
				// The first queued writeTaskData was not the next sequential frame to be written and we were not writing that one...
				// ...so we have to skip and forget the one later in the list that we actually wrote
				prevWrite.nextTask = writeTask.nextTask;
			}
			return ok ? Components.TryWrite.FinishedFrame : Components.TryWrite.Failed; // Has writing this next frame finished or failed?
		}
		protected bool GetGlobalMap(CancellationToken token) {
			if (globalTab != null)
				return false;
			if (table == ColorTable.Global ? AnalyzePixelsGlobal(writeTaskData, token) : AnalyzePixels(writeTaskData, token))
				return true;
			globalTab = writeTaskData.colorTab;
			globalNq = writeTaskData.nq;
			writeTaskData.finished = true;
			return false;
		}
		protected bool GetGlobalMap() {
			if (globalTab != null)
				return false;
			if (table == ColorTable.Global ? AnalyzePixelsGlobal(writeTaskData) : AnalyzePixels(writeTaskData))
				return true;
			globalTab = writeTaskData.colorTab;
			globalNq = writeTaskData.nq;
			writeTaskData.finished = true;
			return false;
		}
		#endregion

		#region Write
		/** Writes Graphic Control Extension */
		protected void WriteGraphicCtrlExt(EncoderTaskData writeData) {
			stream.WriteByte(0x21); // extension introducer
			stream.WriteByte(0xf9); // GCE label
			stream.WriteByte(4); // data block size
			int transp, disp;
			if (transparent != Color.Empty) {
				transp = 1;
				disp = 2; // force clear if using transparent color
			} else transp = disp = 0; // dispose = no action
			if (dispose >= 0)
				disp = dispose & 7; // user override
									// packed fields
			stream.WriteByte(Convert.ToByte(0 | // 1:3 reserved
				(disp <<= 2) | // 4:6 disposal
				0 | // 7   user input - 0 = none
				transp)); // 8   transparency flag

			WriteShort(delay); // delay x 1/100 sec
			stream.WriteByte(Convert.ToByte(writeData.transIndex)); // transparent color index
			stream.WriteByte(0); // block terminator
		}

		/**
		 * Writes Image Descriptor
		 */
		protected void WriteImageDesc(bool global) {
			stream.WriteByte(0x2c); // image separator
			WriteShort(0); // image position x,y = 0,0
			WriteShort(0);
			WriteShort(width); // image size
			WriteShort(height);
			// packed fields
			stream.WriteByte(global ? (byte)0 // no LCT  - GCT is used for first (or only) frame
				:  // specify normal LCT
				Convert.ToByte(0x80 | // 1 local color table  1=yes
					0 | // 2 interlace - 0=no
					0 | // 3 sorted - 0=no
					0 | // 4-5 reserved
					palSize) // 6 - 8 size of color table
			);
		}

		/**
		 * Writes Logical Screen Descriptor
		 */
		protected void WriteLSD() {
			// logical screen size
			WriteShort(width);
			WriteShort(height);
			// packed fields
			stream.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
				0x70 | // 2-4 : color resolution = 7
				0x00 | // 5   : gct sort flag = 0
				palSize)); // 6-8 : gct size
			stream.WriteByte(0); // background color index
			stream.WriteByte(0); // pixel aspect ratio - assume 1:1
		}

		/**
		 * Writes Netscape application extension to define
		 * repeat count.
		 */
		protected void WriteNetscapeExt() {
			stream.WriteByte(0x21); // extension introducer
			stream.WriteByte(0xff); // app extension label
			stream.WriteByte(11); // block size
			WriteString("NETSCAPE" + "2.0"); // app id + auth code
			stream.WriteByte(3); // sub-block size
			stream.WriteByte(1); // loop sub-block id
			WriteShort(repeat); // loop count (extra iterations, 0=repeat forever)
			stream.WriteByte(0); // block terminator
		}

		/**
		 * Writes color table
		 */
		protected void WritePalette(byte[] tab) {
			stream.Write(tab, 0, tab.Length);
			int n = 3 * 256 - tab.Length;
			while (0 <= --n)
				stream.WriteByte(0);
		}

		/**
		 * Encodes and writes pixel data
		 */
		protected void WritePixels(EncoderTaskData writeData) {
			new LZWEncoder(width, height, writeData.indexedPixels, colorDepth).Encode(stream);
		}

		/**
		 *    Write 16-bit value to output stream, LSB first
		 */
		protected void WriteShort(int value) {
			stream.WriteByte(Convert.ToByte(value & 0xff));
			stream.WriteByte(Convert.ToByte((value >> 8) & 0xff));
		}

		/**
		 * Writes string to output stream
		 */
		protected void WriteString(string s) {
			char[] chars = s.ToCharArray();
			for (int i = 0; i < chars.Length; ++i)
				stream.WriteByte((byte)chars[i]);
		}
		#endregion
	}
}