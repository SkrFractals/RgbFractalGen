using System;
using System.Drawing;
using System.IO;
using System.Threading;

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
	1. Add support to output the Gif to a MemoryStream (9/2/2005)
 
*/
#endregion

namespace Gif.Components {

	public class AnimatedGifEncoder {
		protected int len;
		protected int width; // image size
		protected int height;
		protected Color transparent = Color.Empty; // transparent color if given
		protected int transIndex; // transparent index in color table
		protected int repeat = -1; // no repeat
		protected int delay = 0; // frame delay (hundredths)
		protected bool started = false; // ready to output frames
		protected MemoryStream ms = null;
		protected FileStream fs = null;
		protected Stream stream = null;
		unsafe protected byte* pixels; // BGR byte array from frame
		protected byte[] indexedPixels; // converted frame indexed to palette
		protected int colorDepth; // number of bit planes
		protected byte[] colorTab; // RGB palette
		protected bool[] usedEntry = new bool[256]; // active palette entries
		protected int palSize = 7; // color table size (bits-1)
		protected int dispose = -1; // disposal code (-1 = use default)
		protected bool firstFrame = true;
		protected int sample = 10; // default sample interval for quantizer

		/**
		 * Sets the delay time between each frame, or changes it
		 * for subsequent frames (applies to last frame added).
		 *
		 * @param ms int delay time in hundredths
		 */
		public void SetDelay(int hundredths) {
			delay = hundredths;
		}

		/**
		 * Sets the GIF frame disposal code for the last added frame
		 * and any subsequent frames.  Default is 0 if no transparent
		 * color has been set, otherwise 2.
		 * @param code int disposal code.
		 */
		public void SetDispose(int code) {
			if (code >= 0) {
				dispose = code;
			}
		}

		/**
		 * Sets the number of times the set of GIF frames
		 * should be played.  Default is 1; 0 means play
		 * indefinitely.  Must be invoked before the first
		 * image is added.
		 *
		 * @param iter int number of iterations.
		 * @return
		 */
		public void SetRepeat(int iter) {
			if (iter >= 0) {
				repeat = iter;
			}
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

		/**
		 * Adds next GIF frame.  The frame is not written immediately, but is
		 * actually deferred until the next frame is received so that timing
		 * data can be inserted.  Invoking <code>finish()</code> flushes all
		 * frames.  If <code>setSize</code> was not invoked, the size of the
		 * first image is used for all subsequent frames.
		 *
		 * @param im BufferedImage containing frame to write.
		 * @return true if successful.
		 */
		unsafe public bool AddFrame(CancellationToken token, byte* pixels) {
			this.pixels = pixels;
			if (!started)
				return false;
			bool ok = true;
			try {
				if (AnalyzePixels(token))    // build color table & map pixels
					return false;           // Cancelled
				if (firstFrame) {
					WriteLSD(); // logical screen descriptior
					WritePalette(); // global color table
					if (repeat >= 0)
						WriteNetscapeExt();  // use NS app extension to indicate reps
				}
				WriteGraphicCtrlExt(); // write graphic control extension
				WriteImageDesc(); // image descriptor
				if (!firstFrame)
					WritePalette(); // local color table
				WritePixels(); // encode and write pixel data
				firstFrame = false;
			} catch (IOException) {
				ok = false;
			}
			return ok;
		}

		/**
		 * Flushes any pending data and closes output file.
		 * If writing to an OutputStream, the stream is not
		 * closed.
		 */
		public bool Finish(string filename = "", bool closeStream = true) {
			if (!started)
				return false;
			bool ok = true;
			started = false;
			try {
				stream.WriteByte(0x3b); // gif trailer
				stream.Flush();
				if (fs != null)
					fs.Close();
				if (ms != null && filename != "")
					Output(filename);
				if (closeStream)
					ms?.Close();
			} catch (IOException) {
				ok = false;
			}
			// reset for subsequent use
			transIndex = 0;
			indexedPixels = null;
			colorTab = null;
			closeStream = false;
			firstFrame = true;
			return ok;
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
			sample = quality = Math.Max(1, quality);
		}

		/**
		 * Initiates GIF file creation on the given stream.  The stream
		 * is not closed automatically.
		 *
		 * @param os OutputStream on which GIF images are written.
		 * @return false if initial write failed.
		 */

		public bool Start(Stream os, int w, int h) {
			len = (width = w) * (height = h) * 3;
			if (os == null)
				return false;
			bool ok = true;
			stream = os;
			try {
				WriteString("GIF89a"); // header
			} catch (IOException) {
				ok = false;
			}
			return started = ok;
		}

		/**
		 * Initiates writing of a GIF file to a memory stream.
		 *
		 * @return false if open or initial write failed.
		 */
		public bool Start(int w, int h) {
			bool ok = true;
			try {
				fs = null;
				ok = Start(ms = new MemoryStream(10 * 1024), w, h);
			} catch (IOException) {
				ok = false;
			}
			return started = ok;
		}

		/**
		 * Initiates writing of a GIF file with the specified name.
		 *
		 * @param file String containing output file name.
		 * @return false if open or initial write failed.
		 */
		public bool Start(string file, int w, int h) {
			bool ok = true;
			try {
				ms = null;
				ok = Start(fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None), w, h);
			} catch (IOException) {
				ok = false;
			}
			return started = ok;
		}

		/**
		 * Initiates writing of a GIF file with the specified name.
		 *
		 * @return false if open or initial write failed.
		 */
		public bool Output(string file) {
			try {
				fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				fs.Write(ms.ToArray(), 0, (int)ms.Length);
				fs.Close();
				fs = null;
			} catch (IOException) {
				return false;
			}
			return true;
		}

		public MemoryStream Output() {
			return ms;
		}

		/**
		 * Analyzes image colors and creates color map.
		 */
		unsafe protected bool AnalyzePixels(CancellationToken token) {
			indexedPixels = new byte[len / 3];
			NeuQuant nq = new NeuQuant(pixels, len, sample);
			// initialize quantizer
			colorTab = nq.Process(token); // create reduced palette
			if (token.IsCancellationRequested)
				return true;
			byte* p = pixels;

			for (int x = 0, i = 0; x < width; ++x) {
				if (token.IsCancellationRequested)
					return true;
				for (int y = 0; y < height; ++y) {
					usedEntry[indexedPixels[i++] = (byte)nq.Map(p[0], p[1], p[2])] = true;
					p += 3;
				}
			}
			colorDepth = 8;
			palSize = 7;
			// get closest match to transparent color if specified
			if (transparent != Color.Empty)
				transIndex = nq.Map(transparent.B, transparent.G, transparent.R);
			return false;
		}

		/**
		 * Writes Graphic Control Extension
		 */
		protected void WriteGraphicCtrlExt() {
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
			stream.WriteByte(Convert.ToByte(transIndex)); // transparent color index
			stream.WriteByte(0); // block terminator
		}

		/**
		 * Writes Image Descriptor
		 */
		protected void WriteImageDesc() {
			stream.WriteByte(0x2c); // image separator
			WriteShort(0); // image position x,y = 0,0
			WriteShort(0);
			WriteShort(width); // image size
			WriteShort(height);
			// packed fields
			stream.WriteByte(firstFrame ? (byte)0 // no LCT  - GCT is used for first (or only) frame
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
		protected void WritePalette() {
			stream.Write(colorTab, 0, colorTab.Length);
			int n = 3 * 256 - colorTab.Length;
			while (0 <= --n)
				stream.WriteByte(0);
		}

		/**
		 * Encodes and writes pixel data
		 */
		protected void WritePixels() {
			new LZWEncoder(width, height, indexedPixels, colorDepth).Encode(stream);
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
	}
}