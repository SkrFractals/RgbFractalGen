#region .NET Disclaimer/Info
//===============================================================================
//
// gOODiDEA, uLand.com
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gif.Components;

/** A struct containing data for each analysis task, and also acting like a pointer list of the tasks. */
public class EncoderTaskData {
	public int TransIndex;          // transparent index in color table
	public byte[] IndexedPixels;    // converted frame indexed to palette
	public byte[] ColorTab;         // RGB palette
	public unsafe byte*
		PixelsPtr = null;           // BGR bytes (pointer array) from frame
	public int Stride;              // Number of bytes per scanline if using pixelsPtr
									// (usually 3 * width, but some widths can have few empty bytes at the end of each line, so you should request BitmapData.stride instead)
	public byte[] PixelsArr;        // BGR bytes (classic array) from frame
	public bool Finished = false;   // Is this task finished? Will let TryWriteFrameIntoFile to write this frame, and release the data
	public bool Failed = false;     // Has something failed in this task?
	public NeuQuant ThisNeuQuant = null;        // This task's instance of NeuQuant
	public Task ThisTask = null;    // If you add task reference in AddFrame, it will crate its own task, and save it for both that reference and this
	public int FrameIndex;          // Frame index within the animation
	public BitmapData
		BitmapData = null;          // LockBits when added direct image
	public Bitmap Bitmap = null;    // Direct image
	public EncoderTaskData
		NextTask = null;            // The task for next frame
									// (if its null, the previous task was the last frame and the animation should complete upon its finish)
	public MemoryStream ThisMemoryStream;
}

public class NeuQuant {
	private const int NetSize = 256; /* number of colours used */

	/* four primes near 500 - assume no image has a length so large */
	/* that it is divisible by all four primes */
	private const int Prime1 = 499;
	private const int Prime2 = 491;
	private const int Prime3 = 487;
	private const int Prime4 = 503;

	private const int MinPictureBytes = 3 * Prime4;
	/* minimum size for input image */
	/* Program Skeleton
	   ----------------
	   [select SampleFac in range 1..30]
	   [read image from input file]
	   pic = (unsigned char*) malloc(3*width*height);
	   InitNet(pic,3*width*height, SampleFac);
	   learn();
	   UnBiasNet();
	   [write output image header, using WriteColourMap(f)]
	   InxBuild();
	   write output image using InxSearch(b,g,r)      */

	/* Network Definitions
	   ------------------- */
	private const int MaxNetPos = NetSize - 1;
	private const int NetBiasShift = 4; /* bias for colour values */
	private const int NumCycles = 100; /* no. of learning cycles */

	/* defs for freq and bias */
	private const int IntBiasShift = 16; /* bias for fractions */
	private const int IntBias = 1 << IntBiasShift;

	private const int GammaShift = 10; /* gamma = 1024 */

	//protected static readonly int gamma = 1 << GammaShift;
	private const int BetaShift = 10;
	private const int Beta = IntBias >> BetaShift; /* beta = 1/1024 */
	private const int BetaGamma = IntBias << (GammaShift - BetaShift);

	/* defs for decreasing radius factor */
	private const int InitRad = NetSize >> 3; /* for 256 cols, radius starts */
	private const int RadiusBiasShift = 6; /* at 32.0 biased by 6 bits */
	private const int RadiusBias = 1 << RadiusBiasShift;
	private const int InitRadius = InitRad * RadiusBias; /* and decreases by a */
	private const int RadiusDec = 30; /* factor of 1/30 each cycle */

	/* defs for decreasing alpha factor */
	private const int AlphaBiasShift = 10; /* alpha starts at 1.0 */
	private const int InitAlpha = 1 << AlphaBiasShift;

	private int alphaDec; /* biased by 10 bits */

	/* RadBias and AlphaRadBias used for RadPower calculation */
	private const int RadBiasShift = 8;
	private const int RadBias = 1 << RadBiasShift;
	private const int AlphaRadBiasShift = AlphaBiasShift + RadBiasShift;
	private const int AlphaRadBias = 1 << AlphaRadBiasShift;

	/* Types and Global Variables
	-------------------------- */

	private readonly EncoderTaskData encode;
	private readonly unsafe byte* thePicturePtr = null; /* the input image itself as a byte pointer array */
	private readonly int theStride;
	private readonly byte[] thePictureArr; /* the input image itself as a byte classic array */

	private readonly int lengthCount; /* lengthCount = H*W*3 */

	private int sampleFac; /* sampling factor 1..30 */

	private int factor;
	private readonly int w3;

	//   typedef int pixel[4];                /* BGRc */
	private readonly int[][] network; /* the network itself - [netSize][4] */

	private readonly int[] netIndex = new int[256];
	/* for network lookup - really 256 */

	private readonly int[] bias = new int[NetSize];

	/* bias and freq arrays for learning */
	private readonly int[] freq = new int[NetSize];
	private readonly int[] radPower = new int[InitRad];
	/* radPower for precomputation */

	/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
	   ----------------------------------------------------------------------- */
	public NeuQuant(EncoderTaskData encodeTask, int frameBytes, int sample, int width3) {
		encode = encodeTask;
		w3 = width3;
		lengthCount = frameBytes;
		sampleFac = sample;
		network = new int[NetSize][];
		for (var i = 0; i < NetSize; ++i)
			InitNetwork(i);
	}

	/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
	   ----------------------------------------------------------------------- */
	public NeuQuant(byte[] thePicture, int frameBytes, int sample, int width3) {
		thePictureArr = thePicture;
		lengthCount = frameBytes;
		sampleFac = sample;
		w3 = width3;
		network = new int[NetSize][];
		for (var i = 0; i < NetSize; ++i)
			InitNetwork(i);
	}

	/* Initialise network in range (0,0,0) to (255,255,255) and set parameters - cancellable
	   ----------------------------------------------------------------------- */
	public NeuQuant(byte[] thePicture, int frameBytes, int sample, int width3, CancellationToken token) {
		thePictureArr = thePicture;
		lengthCount = frameBytes;
		sampleFac = sample;
		w3 = width3;
		network = new int[NetSize][];
		for (var i = 0; i < NetSize; ++i) {
			if (token.IsCancellationRequested)
				return;
			InitNetwork(i);
		}

		FactorizeLength(lengthCount, token);
	}

	/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
	   ----------------------------------------------------------------------- */
	public unsafe NeuQuant(byte* picture, int stride, int frameBytes, int sample, int width3) {
		thePicturePtr = picture;
		lengthCount = frameBytes;
		sampleFac = sample;
		w3 = width3;
		theStride = stride;
		network = new int[NetSize][];
		for (var i = 0; i < NetSize; ++i)
			InitNetwork(i);
	}

	/* Initialise network in range (0,0,0) to (255,255,255) and set parameters - cancellable
	   ----------------------------------------------------------------------- */
	public unsafe NeuQuant(byte* picture, int stride, int len, int sample, int width3, CancellationToken token) {
		thePicturePtr = picture;
		lengthCount = len;
		sampleFac = sample;
		theStride = stride;
		w3 = width3;
		network = new int[NetSize][];
		for (var i = 0; i < NetSize; ++i) {
			if (token.IsCancellationRequested)
				return;
			InitNetwork(i);
		}

		FactorizeLength(lengthCount, token);
	}

	private void InitNetwork(int i) {
		var p = network[i] = new int[4];
		p[0] = p[1] = p[2] = (i << (NetBiasShift + 8)) / NetSize;
		freq[i] = IntBias / NetSize; /* 1/netSize */
		bias[i] = 0;
	}

	private void FactorizeLength(int len, CancellationToken token) {
		var samplePixels = len / (3 * sampleFac);
		factor = 1;
		for (int t = 2, xt = factor * t, st = samplePixels / t;
			 xt - st < samplePixels - factor;
			 xt = factor * t, st = samplePixels / t) {
			if (token.IsCancellationRequested)
				return;
			if (samplePixels % t == 0) {
				factor = xt;
				samplePixels = st;
				continue;
			}

			++t;
		}
	}

	private byte[] ColorMap() {
		var map = new byte[3 * NetSize];
		var index = new int[NetSize];
		int k;
		for (k = 0; k < NetSize; ++k)
			index[network[k][3]] = k;
		var i = k = 0;
		while (i < NetSize) {
			var j = index[i++];
			map[k++] = (byte)network[j][0];
			map[k++] = (byte)network[j][1];
			map[k++] = (byte)network[j][2];
		}

		return map;
	}

	private static void SwitchPq(ref int[] p, ref int[] q) {
		(p[0], q[0]) = (q[0], p[0]);
		(p[1], q[1]) = (q[1], p[1]);
		(p[2], q[2]) = (q[2], p[2]);
		(p[3], q[3]) = (q[3], p[3]);
	}

	/* Insertion sort of network and building of netIndex[0..255] (to do after unBias)
   ------------------------------------------------------------------------------- */
	private bool InxBuild() {

		int i, j, previousCol = 0, startPos = 0;
		for (i = 0; i < NetSize; ++i) {
			int[] p, q;
			int smallPos = i, smallVal = (p = network[i])[1]; /* index on g */

			/* find smallest in i..netSize-1 */
			for (j = i + 1; j < NetSize; ++j) {
				if ((q = network[j])[1] >= smallVal)
					continue;
				/* index on g */
				smallPos = j;
				smallVal = q[1]; /* index on g */
			}
			q = network[smallPos];
			/* swap p (i) and q (smallPos) entries */
			if (i != smallPos)
				SwitchPq(ref p, ref q);
			/* smallVal's entry is now in position i */
			if (smallVal == previousCol)
				continue;
			netIndex[previousCol] = (startPos + i) >> 1;
			for (j = previousCol + 1; j < smallVal; ++j)
				netIndex[j] = i;
			previousCol = smallVal;
			startPos = i;
		}

		netIndex[previousCol] = (startPos + MaxNetPos) >> 1;
		for (j = previousCol + 1; j < 256; ++j)
			netIndex[j] = MaxNetPos; /* really 256 */
		return false;
	}

	/* Insertion sort of network and building of netIndex[0..255] (to do after unBias)
	   ------------------------------------------------------------------------------- */
	private bool InxBuild(CancellationToken token) {
		int i, j, previousCol = 0, startPos = 0;
		for (i = 0; i < NetSize; ++i) {
			if (token.IsCancellationRequested)
				return true;
			int[] p, q;
			int smallPosition = i, smallVal = (p = network[i])[1]; /* index on g */
			/* find smallest in i..netSize-1 */
			for (j = i + 1; j < NetSize; ++j) {
				if ((q = network[j])[1] >= smallVal)
					continue;
				/* index on g */
				smallPosition = j;
				smallVal = q[1]; /* index on g */
			}
			q = network[smallPosition];
			/* swap p (i) and q (smallPosition) entries */
			if (i != smallPosition)
				SwitchPq(ref p, ref q);
			/* smallVal entry is now in position i */
			if (smallVal == previousCol)
				continue;
			netIndex[previousCol] = (startPos + i) >> 1;
			for (j = previousCol + 1; j < smallVal; ++j)
				netIndex[j] = i;
			previousCol = smallVal;
			startPos = i;
		}
		netIndex[previousCol] = (startPos + MaxNetPos) >> 1;
		for (j = previousCol + 1; j < 256; ++j)
			netIndex[j] = MaxNetPos; /* really 256 */
		return false;
	}

	private readonly struct PixelsFrame {
		internal byte[] Arr { get; }
		internal unsafe byte* Ptr { get; }
		internal int Stride { get; }

		internal unsafe PixelsFrame(byte* ptr, byte[] arr, int stride) {
			Ptr = ptr;
			Arr = arr;
			Stride = stride;
		}
	}
	/* Main Learning Loop
   ------------------ */
	private unsafe bool Learn() {
		var len = 0;
		List<PixelsFrame> ani = [];
		if (encode != null) {
			var e = encode;
			while (e.NextTask != null) {
				len += lengthCount;
				ani.Add(new PixelsFrame(e.PixelsPtr, e.PixelsArr, e.Stride));
				e = e.NextTask;
			}
		} else {
			len = lengthCount;
			ani.Add(new PixelsFrame(thePicturePtr, thePictureArr, theStride));
		}
		if (len < MinPictureBytes)
			sampleFac = 1;
		alphaDec = 30 + (sampleFac - 1) / 3;
		var lim = len;
		var samplePixels = len / (3 * sampleFac);
		var delta = samplePixels / NumCycles;
		if (delta <= 0)
			delta = 1;
		var alpha = InitAlpha;
		var radius = InitRadius;
		var rad = radius >> RadiusBiasShift;
		//if (rad <= 1) // always false
		//	rad = 0;
		var rad2 = rad * rad;
		int i;
		for (i = 0; i < rad; ++i)
			radPower[i] = alpha * ((rad2 - i * i) * RadBias / rad2);

		var step = len < MinPictureBytes
			? 3 : len % Prime1 != 0
				? 3 * Prime1 : len % Prime2 != 0
					? 3 * Prime2 : len % Prime3 != 0
						? 3 * Prime3 : 3 * Prime4;
		var pix = i = 0;
		for (var x = 0; x < samplePixels; ++x) {
			var f = ani[pix / lengthCount];
			var modPix = pix % lengthCount;
			int b, g, r;
			// we don't need & 0xff for bytes
			if (f.Ptr != null) {
				// Calculate the byte offset from the beginning of the row
				// modPix is the pixel index (modPix / w3 is the row), w3 = 3 * width (bytes per row)
				var scanMod = modPix % w3;
				var scanPtr = modPix / w3 * f.Stride; // Find the row in memory (stride accounts for padding)
													  // Access the color channels in the correct order (BGR) using the scanMod for the column
				b = f.Ptr[scanPtr + scanMod] << NetBiasShift; // Blue channel (first byte)
				g = f.Ptr[scanPtr + scanMod + 1] << NetBiasShift; // Green channel (second byte)
				r = f.Ptr[scanPtr + scanMod + 2] << NetBiasShift; // Red channel (third byte)
			} else {
				r = f.Arr[modPix + 0] << NetBiasShift;
				g = f.Arr[modPix + 1] << NetBiasShift;
				b = f.Arr[modPix + 2] << NetBiasShift;
			}
			int j;
			AlterSingle(alpha, j = Contest(b, g, r), b, g, r);
			if (rad != 0 && AlterNeigh(rad, j, b, g, r))
				return true; /* alter neighbours */
			if ((pix += step) >= lim)
				pix -= len;
			if (++i % delta != 0)
				continue;
			alpha -= alpha / alphaDec;
			rad = (radius -= radius / RadiusDec) >> RadiusBiasShift;
			if (rad <= 1)
				rad = 0;
			rad2 = rad * rad;
			for (j = 0; j < rad; ++j)
				radPower[j] = alpha * ((rad2 - j * j) * RadBias / rad2);
		}
		return false;
	}

	/* Main Learning Loop - cancellable
	   ------------------ */
	private unsafe bool Learn(CancellationToken token) {
		if (token.IsCancellationRequested)
			return true;
		int delta, samplePixels, len = 0;
		//PixelsFrame f;
		List<PixelsFrame> ani = [];
		if (encode != null) {
			var e = encode;
			while (e.NextTask != null) {
				len += lengthCount;
				ani.Add(new PixelsFrame(e.PixelsPtr, e.PixelsArr, e.Stride));
				e = e.NextTask;
			}

			delta = (samplePixels = len / (3 * sampleFac)) / NumCycles;
			FactorizeLength(len, token);
		} else {
			len = lengthCount;
			ani.Add(new PixelsFrame(thePicturePtr, thePictureArr, theStride));
			delta = (samplePixels = len / (3 * sampleFac)) / NumCycles;
		}
		if (len < MinPictureBytes)
			sampleFac = 1;
		alphaDec = 30 + (sampleFac - 1) / 3;
		var lim = len;

		if (delta <= 0)
			delta = 1;
		var alpha = InitAlpha;
		var radius = InitRadius;
		var rad = radius >> RadiusBiasShift;
		//if (rad <= 1) // always false
		//	rad = 0;
		var rad2 = rad * rad;
		int i;
		for (i = 0; i < rad; ++i)
			radPower[i] = alpha * ((rad2 - i * i) * RadBias / rad2);
		var step = len < MinPictureBytes
			? 3 : len % Prime1 != 0
				? 3 * Prime1 : len % Prime2 != 0
					? 3 * Prime2 : len % Prime3 != 0
						? 3 * Prime3 : 3 * Prime4;
		var pix = i = 0;
		samplePixels /= factor;

		int scanMod, scanPtr;

		try {
			for (var x = 0; x < factor; ++x) {
				if (token.IsCancellationRequested)
					return true;
				for (var y = 0; y < samplePixels; ++y) {
					var f = ani[pix / lengthCount];
					var modPix = pix % lengthCount;
					int b, g, r, j;
					// we don't need & 0xff for bytes
					if (f.Ptr != null) {
						// Calculate the byte offset from the beginning of the row
						// modPix is the pixel index (modPix / w3 is the row), w3 = 3 * width (bytes per row)
						scanMod = modPix % w3;
						scanPtr = modPix / w3 * f.Stride; // Find the row in memory (stride accounts for padding)
														  // Access the color channels in the correct order (BGR) using the scanMod for the column
						b = f.Ptr[scanPtr + scanMod] << NetBiasShift; // Blue channel (first byte)
						g = f.Ptr[scanPtr + scanMod + 1] << NetBiasShift; // Green channel (second byte)
						r = f.Ptr[scanPtr + scanMod + 2] << NetBiasShift; // Red channel (third byte)
					} else {
						r = f.Arr[modPix + 0] << NetBiasShift;
						g = f.Arr[modPix + 1] << NetBiasShift;
						b = f.Arr[modPix + 2] << NetBiasShift;
					}
					AlterSingle(alpha, j = Contest(b, g, r), b, g, r);
					if (rad != 0 && AlterNeigh(rad, j, b, g, r))
						return true; /* alter neighbours */
					if ((pix += step) >= lim)
						pix -= lengthCount;
					if (++i % delta != 0)
						continue;
					alpha -= alpha / alphaDec;
					rad = (radius -= radius / RadiusDec) >> RadiusBiasShift;
					if (rad <= 1)
						rad = 0;
					rad2 = rad * rad;
					for (j = 0; j < rad; ++j)
						radPower[j] = alpha * ((rad2 - j * j) * RadBias / rad2);
				}
			}
		} catch (Exception) {
			return true;
		}
		return false;
	}

	/* Search for BGR values 0..255 (after net is unbiased) and return colour index
		   ---------------------------------------------------------------------------- */
	public int Map(int r, int g, int b) {
		int i = netIndex[g], /* index on g */
			j = i - 1, /* start at netIndex[g] and work outwards */
			best = -1,
			bestDist = 1000; /* biggest possible dist is 256*3 */
		while (i < NetSize || j >= 0) {
			int dist, a;
			int[] p;
			if (i < NetSize) {
				if ((dist = (p = network[i])[1] - g) < bestDist) {
					/* inx key */
					++i;
					if (dist < 0)
						dist = -dist;
					if (((a = p[0] - b) < 0 ? dist -= a : dist += a) < bestDist &&
						((a = p[2] - r) < 0 ? dist -= a : dist += a) < bestDist) {
						bestDist = dist;
						best = p[3];
					}
				} else i = NetSize; /* stop iter */
			}

			if (j < 0)
				continue;
			if ((dist = g - (p = network[j])[1]) < bestDist) {
				/* inx key - reverse dif */
				--j;
				if (dist < 0)
					dist = -dist;
				if (((a = p[0] - b) < 0 ? dist -= a : dist += a) >= bestDist ||
					((a = p[2] - r) < 0 ? dist -= a : dist += a) >= bestDist)
					continue;
				bestDist = dist;
				best = p[3];
			} else j = -1; /* stop iter */
		}

		return best;
	}

	public byte[] Process() {
		if (Learn())
			return null;
		UnBiasNet();
		return InxBuild() ? null : ColorMap();
	}

	public byte[] Process(CancellationToken token) {
		if (Learn(token))
			return null;
		UnBiasNet();
		return InxBuild(token) ? null : ColorMap();
	}

	/* UnBias network to give byte values 0..255 and record position i to prepare for sort
	   ----------------------------------------------------------------------------------- */
	private void UnBiasNet() {
		for (var i = 0; i < NetSize; ++i) {
			var ni = network[i];
			ni[0] >>= NetBiasShift;
			ni[1] >>= NetBiasShift;
			ni[2] >>= NetBiasShift;
			ni[3] = i; /* record colour no */
		}
	}

	/* Move adjacent neurons by precomputed alpha*(1-((i-j)^2/[r]^2)) in radPower[|i-j|]
	   --------------------------------------------------------------------------------- */
	private bool AlterNeigh(int rad, int i, int b, int g, int r) {
		int j = i + 1, k = i - 1, lo, hi, m = 1;
		if ((lo = i - rad) < -1)
			lo = -1;
		if ((hi = i + rad) > NetSize)
			hi = NetSize;
		while (j < hi || k > lo) {
			var a = radPower[m++];
			int[] p;

			if (j < hi) {
				p = network[j++];
				try {
					p[0] -= (a * (p[0] - b)) / AlphaRadBias;
					p[1] -= (a * (p[1] - g)) / AlphaRadBias;
					p[2] -= (a * (p[2] - r)) / AlphaRadBias;
				} catch (Exception) { } // prevents 1.3 miscompilation
			}
			if (k <= lo)
				continue;
			
			p = network[k--];
			try {
				p[0] -= (a * (p[0] - b)) / AlphaRadBias;
				p[1] -= (a * (p[1] - g)) / AlphaRadBias;
				p[2] -= (a * (p[2] - r)) / AlphaRadBias;
			} catch (Exception) { }
		}
		return false;
	}

	/* Move neuron i towards biased (b,g,r) by factor alpha
	   ---------------------------------------------------- */
	private void AlterSingle(int alpha, int i, int b, int g, int r) {
		/* alter hit neuron */
		var n = network[i];
		n[0] -= alpha * (n[0] - b) / InitAlpha;
		n[1] -= alpha * (n[1] - g) / InitAlpha;
		n[2] -= alpha * (n[2] - r) / InitAlpha;
	}

	/* Search for biased BGR values
	   ---------------------------- */
	private int Contest(int b, int g, int r) {
		/* finds closest neuron (min dist) and updates freq */
		/* finds the best neuron (min dist-bias) and returns position */
		/* for frequently chosen neurons, freq[i] is high and bias[i] is negative */
		/* bias[i] = gamma*((1/netSize)-freq[i]) */
		int bestPosition = -1, bestBiasPosition = bestPosition, bestDist = ~(1 << 31), bestBiasDist = bestDist;
		for (var i = 0; i < NetSize; ++i) {
			int[] n;
			int biasDist, betaFreq, a;
			var dist = (n = network[i])[0] - b;
			if (dist < 0)
				dist = -dist;
			dist += (a = n[1] - g) < 0 ? -a : a;
			if ((dist += (a = n[2] - r) < 0 ? -a : a) < bestDist) {
				bestDist = dist;
				bestPosition = i;
			}

			if ((biasDist = dist - (bias[i] >> (IntBiasShift - NetBiasShift))) < bestBiasDist) {
				bestBiasDist = biasDist;
				bestBiasPosition = i;
			}

			freq[i] -= betaFreq = freq[i] >> BetaShift;
			bias[i] += betaFreq << GammaShift;
		}

		freq[bestPosition] += Beta;
		bias[bestPosition] -= BetaGamma;
		return bestBiasPosition;
	}
}