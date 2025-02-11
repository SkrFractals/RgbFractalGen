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
/* NeuQuant Neural-Net Quantization Algorithm
 * ------------------------------------------
 *
 * Copyright (c) 1994 Anthony Dekker
 *
 * NEUQUANT Neural-Net quantization algorithm by Anthony Dekker, 1994.
 * See "Kohonen neural networks for optimal colour quantization"
 * in "Network: Computation in Neural Systems" Vol. 5 (1994) pp 351-367.
 * for a discussion of the algorithm.
 *
 * Any party obtaining a copy of these files from the author, directly or
 * indirectly, is granted, free of charge, a full and unrestricted irrevocable,
 * world-wide, paid up, royalty-free, nonexclusive right and license to deal
 * in this software and documentation files (the "Software"), including without
 * limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons who receive
 * copies from any such party to do so, with the only requirement being
 * that this copyright notice remain intact.
 */

// Ported to Java 12/00 K Weiner
#endregion

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using static Gif.Components.AnimatedGifEncoder;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;

namespace Gif.Components {

	/** A struct containing data for each analyzis task, and also acting like a pointer list of the tasks. */
	public class EncoderTaskData {
		public int transIndex;          // transparent index in color table
		public byte[] indexedPixels;    // converted frame indexed to palette
		public byte[] colorTab;         // RGB palette
		public unsafe byte*
			pixelsPtr = null;           // BGR bytes (pointer array) from frame
		public int stride;				// Number of bytes per scanline if using pixelsPtr
										// (usually 3 * width, but some widths can have few empty bytes at the end of each line, so you should request BitmapData.stride instead)
		public byte[] pixelsArr;        // BGR bytes (classic array) from frame
		public bool finished = false;   // Is this task finished? Will let TryWriteFrameIntoFile to write this frame, and release the data
		public bool failed = false;     // Has something failed in this task?
		public NeuQuant nq = null;		// This task's instance of NeuQuant
		public Task thisTask = null;    // If you add task referrence in AddFrame, it will crate its own task, and save it for both that reference and this
		public int frameIndex;          // Frame index within the animation
		public BitmapData
			bitmapData = null;          // LockBits when added direct image
		public Bitmap bitmap = null;    // Direct image
		public EncoderTaskData
			nextTask = null;            // The task for next frame
										// (if its null, the previous task was the last frame and the animation shoudl complete upon it's finish)
		public MemoryStream ms;
	}

	public class NeuQuant {
		protected static readonly int netsize = 256; /* number of colours used */
		/* four primes near 500 - assume no image has a length so large */
		/* that it is divisible by all four primes */
		protected static readonly int prime1 = 499;
		protected static readonly int prime2 = 491;
		protected static readonly int prime3 = 487;
		protected static readonly int prime4 = 503;
		protected static readonly int minpicturebytes = 3 * prime4;
		/* minimum size for input image */
		/* Program Skeleton
		   ----------------
		   [select samplefac in range 1..30]
		   [read image from input file]
		   pic = (unsigned char*) malloc(3*width*height);
		   initnet(pic,3*width*height,samplefac);
		   learn();
		   unbiasnet();
		   [write output image header, using writecolourmap(f)]
		   inxbuild();
		   write output image using inxsearch(b,g,r)      */

		/* Network Definitions
		   ------------------- */
		protected static readonly int maxnetpos = netsize - 1;
		protected static readonly int netbiasshift = 4; /* bias for colour values */
		protected static readonly int ncycles = 100; /* no. of learning cycles */

		/* defs for freq and bias */
		protected static readonly int intbiasshift = 16; /* bias for fractions */
		protected static readonly int intbias = 1 << intbiasshift;
		protected static readonly int gammashift = 10; /* gamma = 1024 */
		protected static readonly int gamma = 1 << gammashift;
		protected static readonly int betashift = 10;
		protected static readonly int beta = intbias >> betashift; /* beta = 1/1024 */
		protected static readonly int betagamma = intbias << (gammashift - betashift);

		/* defs for decreasing radius factor */
		protected static readonly int initrad = netsize >> 3; /* for 256 cols, radius starts */
		protected static readonly int radiusbiasshift = 6; /* at 32.0 biased by 6 bits */
		protected static readonly int radiusbias = 1 << radiusbiasshift;
		protected static readonly int initradius = initrad * radiusbias; /* and decreases by a */
		protected static readonly int radiusdec = 30; /* factor of 1/30 each cycle */

		/* defs for decreasing alpha factor */
		protected static readonly int alphabiasshift = 10; /* alpha starts at 1.0 */
		protected static readonly int initalpha = 1 << alphabiasshift;

		protected int alphadec; /* biased by 10 bits */

		/* radbias and alpharadbias used for radpower calculation */
		protected static readonly int radbiasshift = 8;
		protected static readonly int radbias = 1 << radbiasshift;
		protected static readonly int alpharadbshift = alphabiasshift + radbiasshift;
		protected static readonly int alpharadbias = 1 << alpharadbshift;

		/* Types and Global Variables
		-------------------------- */

		protected EncoderTaskData encode = null;
		unsafe protected byte* pixelsPtr = null; /* the input image itself as a byte pointer array */
		protected int thestride;
		protected byte[] pixelsArr; /* the input image itself as a byte classic array */

		protected int lengthcount; /* lengthcount = H*W*3 */

		protected int samplefac; /* sampling factor 1..30 */

		protected int factor;
		protected int w3;

		//   typedef int pixel[4];                /* BGRc */
		protected int[][] network; /* the network itself - [netsize][4] */

		protected int[] netindex = new int[256];
		/* for network lookup - really 256 */

		protected int[] bias = new int[netsize];
		/* bias and freq arrays for learning */
		protected int[] freq = new int[netsize];
		protected int[] radpower = new int[initrad];
		/* radpower for precomputation */

		/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
		   ----------------------------------------------------------------------- */
		unsafe public NeuQuant(EncoderTaskData encodeTask, int frameBytes, int sample, int width3) {
			encode = encodeTask;
			w3 = width3;
			lengthcount = frameBytes;
			samplefac = sample;
			network = new int[netsize][];
			for (int i = 0; i < netsize; ++i)
				InitNetwork(i);
		}

		/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
		   ----------------------------------------------------------------------- */
		unsafe public NeuQuant(byte[] thepic, int frameBytes, int sample, int width3) {
			pixelsArr = thepic;
			lengthcount = frameBytes;
			samplefac = sample;
			w3 = width3;
			network = new int[netsize][];
			for (int i = 0; i < netsize; ++i)
				InitNetwork(i);
		}

		/* Initialise network in range (0,0,0) to (255,255,255) and set parameters - cancellable
		   ----------------------------------------------------------------------- */
		unsafe public NeuQuant(byte[] thepic, int frameBytes, int sample, int width3, CancellationToken token) {
			pixelsArr = thepic;
			lengthcount = frameBytes;
			samplefac = sample;
			w3 = width3;
			network = new int[netsize][];
			for (int i = 0; i < netsize; ++i) {
				if (token.IsCancellationRequested)
					return;
				InitNetwork(i);
			}
			FactorizeLength(token, lengthcount);
		}

		/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
		   ----------------------------------------------------------------------- */
		unsafe public NeuQuant(byte* thepic, int stride, int frameBytes, int sample, int width3) {
			pixelsPtr = thepic;
			lengthcount = frameBytes;
			samplefac = sample;
			w3 = width3;
			thestride = stride;
			network = new int[netsize][];
			for (int i = 0; i < netsize; ++i) 
				InitNetwork(i);
		}

		/* Initialise network in range (0,0,0) to (255,255,255) and set parameters - cancellable
		   ----------------------------------------------------------------------- */
		unsafe public NeuQuant(byte* thepic, int stride, int len, int sample, int width3, CancellationToken token) {
			pixelsPtr = thepic;
			lengthcount = len;
			samplefac = sample;
			thestride = stride;
			w3 = width3;
			network = new int[netsize][];
			for (int i = 0; i < netsize; ++i) {
				if (token.IsCancellationRequested)
					return;
				InitNetwork(i);
			}
			FactorizeLength(token, lengthcount);
		}

		protected void InitNetwork(int i) {
			int[] p = network[i] = new int[4];
			p[0] = p[1] = p[2] = (i << (netbiasshift + 8)) / netsize;
			freq[i] = intbias / netsize; /* 1/netsize */
			bias[i] = 0;
		}

		protected void FactorizeLength(CancellationToken token, int len) {
			int samplepixels = len / (3 * samplefac);
			factor = 1;
			for (int t = 2, xt = factor * t, st = samplepixels / t; xt - st < samplepixels - factor; xt = factor * t, st = samplepixels / t) {
				if (token.IsCancellationRequested)
					return;
				if ((samplepixels % t) == 0) {
					factor = xt;
					samplepixels = st;
					continue;
				}
				++t;
			}
		}

		public byte[] ColorMap() {
			byte[] map = new byte[3 * netsize];
			int[] index = new int[netsize];
			int k;
			for (k = 0; k < netsize; ++k)
				index[network[k][3]] = k;
			int i = k = 0;
			while (i < netsize) {
				int j = index[i++];
				map[k++] = (byte)network[j][0];
				map[k++] = (byte)network[j][1];
				map[k++] = (byte)network[j][2];
			}
			return map;
		}
		protected void SwitchPQ(ref int[] p, ref int[] q) {
			(p[0], q[0]) = (q[0], p[0]);
			(p[1], q[1]) = (q[1], p[1]);
			(p[2], q[2]) = (q[2], p[2]);
			(p[3], q[3]) = (q[3], p[3]);
		}

		/* Insertion sort of network and building of netindex[0..255] (to do after unbias)
		   ------------------------------------------------------------------------------- */
		public bool Inxbuild() {

			int i, j, smallpos, smallval, previouscol = 0, startpos = 0;
			int[] p, q;
			for (i = 0; i < netsize; ++i) {
				smallval = (p = network[smallpos = i])[1]; /* index on g */
				/* find smallest in i..netsize-1 */
				for (j = i + 1; j < netsize; ++j) {
					if ((q = network[j])[1] < smallval) { /* index on g */
						smallpos = j;
						smallval = q[1]; /* index on g */
					}
				}
				q = network[smallpos];
				/* swap p (i) and q (smallpos) entries */
				if (i != smallpos) 
					SwitchPQ(ref p, ref q);
				/* smallval entry is now in position i */
				if (smallval != previouscol) {
					netindex[previouscol] = (startpos + i) >> 1;
					for (j = previouscol + 1; j < smallval; ++j)
						netindex[j] = i;
					previouscol = smallval;
					startpos = i;
				}
			}
			netindex[previouscol] = (startpos + maxnetpos) >> 1;
			for (j = previouscol + 1; j < 256; ++j)
				netindex[j] = maxnetpos; /* really 256 */
			return false;
		}

		/* Insertion sort of network and building of netindex[0..255] (to do after unbias)
		   ------------------------------------------------------------------------------- */
		public bool Inxbuild(CancellationToken token) {

			int i, j, smallpos, smallval, previouscol = 0, startpos = 0;
			int[] p, q;
			for (i = 0; i < netsize; ++i) {
				if (token.IsCancellationRequested)
					return true;

				smallval = (p = network[smallpos = i])[1]; /* index on g */
				/* find smallest in i..netsize-1 */
				for (j = i + 1; j < netsize; ++j) {
					if ((q = network[j])[1] < smallval) { /* index on g */
						smallpos = j;
						smallval = q[1]; /* index on g */
					}
				}
				q = network[smallpos];
				/* swap p (i) and q (smallpos) entries */
				if (i != smallpos)
					SwitchPQ(ref p, ref q);
				/* smallval entry is now in position i */
				if (smallval != previouscol) {
					netindex[previouscol] = (startpos + i) >> 1;
					for (j = previouscol + 1; j < smallval; ++j)
						netindex[j] = i;
					previouscol = smallval;
					startpos = i;
				}
			}
			netindex[previouscol] = (startpos + maxnetpos) >> 1;
			for (j = previouscol + 1; j < 256; ++j)
				netindex[j] = maxnetpos; /* really 256 */
			return false;
		}

		protected struct PixelsFrame {
			internal byte[] arr;
			unsafe internal byte* ptr;
			internal int stride;
			internal unsafe PixelsFrame(byte* ptr, byte[] arr, int stride) {
				this.ptr = ptr;
				this.arr = arr;
				this.stride = stride;
			}
		}

		/* Main Learning Loop
		   ------------------ */
		unsafe public bool Learn() {

			int i, j, b, g, r, radius, rad, rad2, alpha, step, delta, samplepixels, pix, lim, modpix, scanPtr, scanMod;
			int len = 0;
			PixelsFrame f;
			List<PixelsFrame> ani = [];

			if (encode != null) {
				var e = encode;
				while (e.nextTask != null) {
					len += lengthcount;
					ani.Add(new PixelsFrame(e.pixelsPtr, e.pixelsArr, e.stride));
					e = e.nextTask;
				}
			} else { 
				len = lengthcount;
				ani.Add(new PixelsFrame(pixelsPtr, pixelsArr, thestride)); 
			}

			if (len < minpicturebytes)
				samplefac = 1;
			alphadec = 30 + ((samplefac - 1) / 3);
			lim = len;
			delta = (samplepixels = len / (3 * samplefac)) / ncycles;
			if (delta <= 0)
				delta = 1;
			alpha = initalpha;
			if ((rad = (radius = initradius) >> radiusbiasshift) <= 1)
				rad = 0;
			rad2 = rad * rad;
			for (i = 0; i < rad; ++i)
				radpower[i] = alpha * (((rad2 - i * i) * radbias) / rad2);

			step = len < minpicturebytes ? 3 : ((len % prime1) != 0 ? 3 * prime1 : ((len % prime2) != 0 ? 3 * prime2 : ((len % prime3) != 0 ? 3 * prime3 : 3 * prime4)));
			i = pix = 0;
			for (int x = 0; x < samplepixels; ++x) {

				f = ani[pix / lengthcount];
				modpix = pix % lengthcount;

				// we don't need & 0xff for bytes
				if (f.ptr != null) {
					// Calculate the byte offset from the beginning of the row
					scanMod = modpix % w3;  // modpix is the pixel index (modpix / w3 is the row), w3 = 3 * width (bytes per row)
					scanPtr = (modpix / w3) * f.stride;  // Find the row in memory (stride accounts for padding)

					// Access the color channels in the correct order (BGR) using the scanMod for the column
					b = (f.ptr[scanPtr + scanMod]) << netbiasshift;       // Blue channel (first byte)
					g = (f.ptr[scanPtr + scanMod + 1]) << netbiasshift;   // Green channel (second byte)
					r = (f.ptr[scanPtr + scanMod + 2]) << netbiasshift;   // Red channel (third byte)

					// original variant not accounting for padding bytes in stride:
					//r = (f.ptr[modpix + 0] /*& 0xff*/) << netbiasshift;
					//g = (f.ptr[modpix + 1] /*& 0xff*/) << netbiasshift;
					//b = (f.ptr[modpix + 2] /*& 0xff*/) << netbiasshift;
				} else {
					r = (f.arr[modpix + 0] /*& 0xff*/) << netbiasshift;
					g = (f.arr[modpix + 1] /*& 0xff*/) << netbiasshift;
					b = (f.arr[modpix + 2] /*& 0xff*/) << netbiasshift;
				}
				Altersingle(alpha, j = Contest(b, g, r), b, g, r);
				if (rad != 0)
					Alterneigh(rad, j, b, g, r); /* alter neighbours */
				if ((pix += step) >= lim)
					pix -= len;
				if ((++i) % delta != 0)
					continue;
				alpha -= alpha / alphadec;
				rad = (radius -= radius / radiusdec) >> radiusbiasshift;
				if (rad <= 1)
					rad = 0;
				rad2 = rad * rad;
				for (j = 0; j < rad; ++j)
					radpower[j] = alpha * (((rad2 - j * j) * radbias) / rad2);

			}
			return false;
		}
		/* Main Learning Loop - cancellable
		   ------------------ */
		unsafe public bool Learn(CancellationToken token) {

			int i, j, b, g, r, radius, rad, rad2, alpha, step, delta, samplepixels, pix, lim, modpix, scanPtr, scanMod;
			int len = 0;
			PixelsFrame f;
			List<PixelsFrame> ani = [];

			if (encode != null) {
				var e = encode;
				while (e.nextTask != null) {
					len += lengthcount;
					ani.Add(new PixelsFrame(e.pixelsPtr, e.pixelsArr, e.stride));
					e = e.nextTask;
				}
				delta = (samplepixels = len / (3 * samplefac)) / ncycles;
				FactorizeLength(token, len);
			} else {
				len = lengthcount;
				ani.Add(new PixelsFrame(pixelsPtr, pixelsArr, thestride));
				delta = (samplepixels = len / (3 * samplefac)) / ncycles;
			}

			if (len < minpicturebytes)
				samplefac = 1;
			alphadec = 30 + ((samplefac - 1) / 3);
			lim = len;
			
			if (delta <= 0)
				delta = 1;
			alpha = initalpha;
			if ((rad = (radius = initradius) >> radiusbiasshift) <= 1)
				rad = 0;
			rad2 = rad * rad;
			for (i = 0; i < rad; ++i)
				radpower[i] = alpha * (((rad2 - i * i) * radbias) / rad2);

			step = len < minpicturebytes ? 3 : ((len % prime1) != 0 ? 3 * prime1 : ((len % prime2) != 0 ? 3 * prime2 : ((len % prime3) != 0 ? 3 * prime3 : 3 * prime4)));
			i = pix = 0;
			samplepixels /= factor;
			try {
				for (int x = 0; x < factor; ++x) {
					if (token.IsCancellationRequested)
						return true;
					for (int y = 0; y < samplepixels; ++y) {

						f = ani[pix / lengthcount];
						modpix = pix % lengthcount;

						// we don't need & 0xff for bytes
						if (f.ptr != null) {
							// Calculate the byte offset from the beginning of the row
							scanMod = modpix % w3;  // modpix is the pixel index (modpix / w3 is the row), w3 = 3 * width (bytes per row)
							scanPtr = (modpix / w3) * f.stride;  // Find the row in memory (stride accounts for padding)

							// Access the color channels in the correct order (BGR) using the scanMod for the column
							b = (f.ptr[scanPtr + scanMod]) << netbiasshift;       // Blue channel (first byte)
							g = (f.ptr[scanPtr + scanMod + 1]) << netbiasshift;   // Green channel (second byte)
							r = (f.ptr[scanPtr + scanMod + 2]) << netbiasshift;   // Red channel (third byte)

							// original variant not accounting for padding bytes in stride:
							//r = (f.ptr[modpix + 0] /*& 0xff*/) << netbiasshift;
							//g = (f.ptr[modpix + 1] /*& 0xff*/) << netbiasshift;
							//b = (f.ptr[modpix + 2] /*& 0xff*/) << netbiasshift;
						} else {
							r = (f.arr[modpix + 0] /*& 0xff*/) << netbiasshift;
							g = (f.arr[modpix + 1] /*& 0xff*/) << netbiasshift;
							b = (f.arr[modpix + 2] /*& 0xff*/) << netbiasshift;
						}
						Altersingle(alpha, j = Contest(b, g, r), b, g, r);
						if (rad != 0)
							Alterneigh(rad, j, b, g, r); /* alter neighbours */
						if ((pix += step) >= lim)
							pix -= lengthcount;
						if ((++i) % delta != 0)
							continue;
						alpha -= alpha / alphadec;
						rad = (radius -= radius / radiusdec) >> radiusbiasshift;
						if (rad <= 1)
							rad = 0;
						rad2 = rad * rad;
						for (j = 0; j < rad; ++j)
							radpower[j] = alpha * (((rad2 - j * j) * radbias) / rad2);
					}
				}
			} catch (Exception) { }
			return false;
		}

		/* Search for BGR values 0..255 (after net is unbiased) and return colour index
		   ---------------------------------------------------------------------------- */
		public int Map(int r, int g, int b) {
			int i = netindex[g], /* index on g */
				j = i - 1, /* start at netindex[g] and work outwards */
				dist, a, best = -1,
				bestd = 1000; /* biggest possible dist is 256*3 */
			int[] p;
			while (i < netsize || j >= 0) {
				if (i < netsize) {
					if ((dist = (p = network[i])[1] - g) < bestd) { /* inx key */
						++i;
						if (dist < 0)
							dist = -dist;
						if (((a = p[0] - b) < 0 ? dist -= a : dist += a) < bestd && ((a = p[2] - r) < 0 ? dist -= a : dist += a) < bestd) {
							bestd = dist;
							best = p[3];
						}
					} else i = netsize; /* stop iter */
				}
				if (j >= 0) {
					if ((dist = g - (p = network[j])[1]) < bestd) { /* inx key - reverse dif */
						--j;
						if (dist < 0)
							dist = -dist;
						if (((a = p[0] - b) < 0 ? dist -= a : dist += a) < bestd && ((a = p[2] - r) < 0 ? dist -= a : dist += a) < bestd) {
							bestd = dist;
							best = p[3];
						}
					} else j = -1; /* stop iter */
				}
			}
			return best;
		}
		public byte[] Process() {
			if (Learn())
				return null;
			Unbiasnet();
			return Inxbuild() ? null : ColorMap();
		}

		public byte[] Process(CancellationToken token) {
			if (Learn(token))
				return null;
			Unbiasnet();
			return Inxbuild(token) ? null : ColorMap();
		}

		/* Unbias network to give byte values 0..255 and record position i to prepare for sort
		   ----------------------------------------------------------------------------------- */
		public void Unbiasnet() {
			for (int i = 0; i < netsize; ++i) {
				var ni = network[i];
				ni[0] >>= netbiasshift;
				ni[1] >>= netbiasshift;
				ni[2] >>= netbiasshift;
				ni[3] = i; /* record colour no */
			}
		}

		/* Move adjacent neurons by precomputed alpha*(1-((i-j)^2/[r]^2)) in radpower[|i-j|]
		   --------------------------------------------------------------------------------- */
		protected void Alterneigh(int rad, int i, int b, int g, int r) {
			int j = i + 1, k = i - 1, lo, hi, a, m = 1;
			int[] p;
			if ((lo = i - rad) < -1)
				lo = -1;
			if ((hi = i + rad) > netsize)
				hi = netsize;
			while (j < hi || k > lo) {
				a = radpower[m++];
				if (j < hi) {
					p = network[j++];
					try {
						p[0] -= (a * (p[0] - b)) / alpharadbias;
						p[1] -= (a * (p[1] - g)) / alpharadbias;
						p[2] -= (a * (p[2] - r)) / alpharadbias;
					} catch (Exception) { } // prevents 1.3 miscompilation
				}
				if (k > lo) {
					p = network[k--];
					try {
						p[0] -= (a * (p[0] - b)) / alpharadbias;
						p[1] -= (a * (p[1] - g)) / alpharadbias;
						p[2] -= (a * (p[2] - r)) / alpharadbias;
					} catch (Exception) { }
				}
			}
		}

		/* Move neuron i towards biased (b,g,r) by factor alpha
		   ---------------------------------------------------- */
		protected void Altersingle(int alpha, int i, int b, int g, int r) {
			/* alter hit neuron */
			int[] n = network[i];
			n[0] -= (alpha * (n[0] - b)) / initalpha;
			n[1] -= (alpha * (n[1] - g)) / initalpha;
			n[2] -= (alpha * (n[2] - r)) / initalpha;
		}

		/* Search for biased BGR values
		   ---------------------------- */
		protected int Contest(int b, int g, int r) {
			/* finds closest neuron (min dist) and updates freq */
			/* finds best neuron (min dist-bias) and returns position */
			/* for frequently chosen neurons, freq[i] is high and bias[i] is negative */
			/* bias[i] = gamma*((1/netsize)-freq[i]) */
			int i, dist, a, biasdist, betafreq, bestpos = -1, bestbiaspos = bestpos, bestd = ~(1 << 31), bestbiasd = bestd;
			int[] n;
			for (i = 0; i < netsize; ++i) {
				if ((dist = (n = network[i])[0] - b) < 0)
					dist = -dist;
				dist += (a = n[1] - g) < 0 ? -a : a;
				if ((dist += (a = n[2] - r) < 0 ? -a : a) < bestd) {
					bestd = dist;
					bestpos = i;
				}
				if ((biasdist = dist - ((bias[i]) >> (intbiasshift - netbiasshift))) < bestbiasd) {
					bestbiasd = biasdist;
					bestbiaspos = i;
				}
				freq[i] -= betafreq = freq[i] >> betashift;
				bias[i] += betafreq << gammashift;
			}
			freq[bestpos] += beta;
			bias[bestpos] -= betagamma;
			return bestbiaspos;
		}
	}
}
