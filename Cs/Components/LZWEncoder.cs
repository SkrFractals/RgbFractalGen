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
//==============================================================================
//  Adapted from Jef Poskanzer's Java port by way of J. M. G. Elliott.
//  K Weiner 12/00
#endregion
using System;
using System.IO;

namespace Gif.Components;

public class LzwEncoder(byte[] pixAry, int colorDepth) {

	//private readonly byte[] pixAry = pixels;
	private readonly int initCodeSize = Math.Max(2, colorDepth);

	private const int EndOfFile = -1;

	//private readonly int imgW, imgH;
	//private int remaining;
	private int curPixel;

	// gifcompr.c       - GIF Image compression routines
	//
	// Lempel-Ziv compression based on 'compress'.  GIF modifications by
	// David Rowley (mgardi@watdcsu.waterloo.edu)

	// General defines

	private const int Bits = 12;

	private const int HSize = 5003; // 80% occupancy

	// GIF Image compression - modified 'compress'
	//
	// Based on: compress.c - File compression ala IEEE Computer, June 1984.
	//
	// By Authors:  Spencer W. Thomas      (decvax!harpo!utah-cs!utah-gr!thomas)
	//              Jim McKie              (decvax!mcvax!jim)
	//              Steve Davies           (decvax!vax135!petsd!peora!srd)
	//              Ken Turkowski          (decvax!decwrl!turtlevax!ken)
	//              James A. Woods         (decvax!ihnp4!ames!jaw)
	//              Joe Orost              (decvax!vax135!petsd!joe)

	private int numBits; // number of bits/code
	private const int MaxBits = Bits; // user settable max # bits/code
	private int maxCode; // maximum code, given n_bits
	private const int MaxMaxCode = 1 << Bits; // should NEVER generate this code

	private readonly int[] hTab = new int[HSize];
	private readonly int[] codeTab = new int[HSize];

	//private const int hSize = HSize; // for dynamic table sizing

	private int freeEnt; // first unused entry

	// block compression parameters -- after all codes are used up,
	// and compression rate changes, start over.
	private bool clearFlag;

	// Algorithm:  use open addressing double hashing (no chaining) on the
	// prefix code / next character combination.  We do a variant of Knuth's
	// algorithm D (vol. 3, sec. 6.4) along with G. Knott's relatively-prime
	// secondary probe.  Here, the modular division first probe is gives way
	// to a faster exclusive-or manipulation.  Also do block compression with
	// an adaptive reset, whereby the code table is cleared when the compression
	// ratio decreases, but after the table fills.  The variable-length output
	// codes are re-sized at this point, and a special CLEAR code is generated
	// for the decompressor.  Late addition:  construct the table according to
	// file size for noticeable speed improvement on small files.  Please direct
	// questions about this implementation to ames!jaw.

	private int gInitBits;

	private int clearCode;
	private int endOfFileCode;

	// Output the given code.
	// Inputs:
	//      code:   A n_bits-bit integer.  If == -1, then EOF.  This assumes
	//              that n_bits =< wordSize - 1.
	// Outputs:
	//      Outputs code to the file.
	// Assumptions:
	//      Chars are 8 bits long.
	// Algorithm:
	//      Maintain a BITS character long buffer (so that 8 codes will
	// fit in it exactly).  Use the VAX insv instruction to insert each
	// code in turn.  When the buffer fills up empty it and start over.

	private int curAccum;
	private int curBits;

	private readonly int[] masks =
	[
		0x0000,
		0x0001,
		0x0003,
		0x0007,
		0x000F,
		0x001F,
		0x003F,
		0x007F,
		0x00FF,
		0x01FF,
		0x03FF,
		0x07FF,
		0x0FFF,
		0x1FFF,
		0x3FFF,
		0x7FFF,
		0xFFFF
	];

	// Number of characters so far in this 'packet'
	private int aCount;

	// Define the storage for the packet accumulator
	private readonly byte[] accum = new byte[256];

	//----------------------------------------------------------------------------
	/*public LzwEncoder(int width, int height, byte[] pixels, int colorDepth) {
		//imgW = width; // unused
		//imgH = height; // unused
		pixAry = pixels;
		initCodeSize = Math.Max(2, colorDepth);
	}*/

	// Add a character to the end of the current packet, and if it is 254
	// characters, flush the packet to disk.
	private void Add(byte c, Stream outs) {
		accum[aCount++] = c;
		if (aCount >= 254)
			Flush(outs);
	}

	// Clear out the hash table

	// table clear for block compress
	private void ClearTable(Stream outs) {
		ResetCodeTable(HSize);
		freeEnt = clearCode + 2;
		clearFlag = true;
		Output(clearCode, outs);
	}

	// reset code table
	private void ResetCodeTable(int resetHSize) {
		for (var i = 0; i < resetHSize; ++i)
			hTab[i] = -1;
	}

	private void Compress(int initBits, Stream outs) {
		int fCode;
		int c;

		// Set up the globals:  g_init_bits - initial number of bits
		gInitBits = initBits;

		// Set up the necessary values
		clearFlag = false;
		numBits = gInitBits;
		maxCode = MaxCode(numBits);

		clearCode = 1 << (initBits - 1);
		endOfFileCode = clearCode + 1;
		freeEnt = clearCode + 2;

		aCount = 0; // clear packet

		var ent = NextPixel();

		var hShift = 0;
		for (fCode = HSize; fCode < 65536; fCode *= 2)
			++hShift;
		hShift = 8 - hShift; // set hash code range bound

		//var hSizeReg = hSize;
		ResetCodeTable(HSize); // clear hash table

		Output(clearCode, outs);

	outer_loop:
		while ((c = NextPixel()) != EndOfFile) {

			var i = (c << hShift) ^ ent;
			if (hTab[i] == (fCode = (c << MaxBits) + ent)) {
				// xor hashing
				ent = codeTab[i];
				continue;
			}
			if (hTab[i] >= 0) {
				// non-empty slot

				var d = HSize - i; // secondary hash (after G. Knott)
				if (i == 0)
					d = 1;
				do {
					if ((i -= d) < 0)
						i += HSize;
					if (hTab[i] != fCode)
						continue;
					ent = codeTab[i];
					goto outer_loop;

				} while (hTab[i] >= 0);
			}

			Output(ent, outs);
			ent = c;
			if (freeEnt < MaxMaxCode) {
				codeTab[i] = freeEnt++; // code -> hashtable
				hTab[i] = fCode;
			} else
				ClearTable(outs);
		}

		// Put out the final code.
		Output(ent, outs);
		Output(endOfFileCode, outs);
	}

	//----------------------------------------------------------------------------
	public void Encode(Stream os) {
		os.WriteByte(Convert.ToByte(initCodeSize)); // write "initial code size" byte

		//remaining = imgW * imgH; // reset navigation variables (unused)
		curPixel = 0;

		Compress(initCodeSize + 1, os); // compress and write the pixel data

		os.WriteByte(0); // write block terminator
	}

	// Flush the packet to disk, and reset the accumulator
	private void Flush(Stream outs) {
		if (aCount <= 0)
			return;
		outs.WriteByte(Convert.ToByte(aCount));
		outs.Write(accum, 0, aCount);
		aCount = 0;
	}

	private static int MaxCode(int nBits) {
		return (1 << nBits) - 1;
	}

	//----------------------------------------------------------------------------
	// Return the next pixel from the image
	//----------------------------------------------------------------------------
	private int NextPixel() {
		var upperBound = pixAry.GetUpperBound(0);
		return curPixel <= upperBound ? pixAry[curPixel++] & 0xff : EndOfFile;
	}

	private void Output(int code, Stream outs) {
		curAccum &= masks[curBits];

		if (curBits > 0)
			curAccum |= code << curBits;
		else
			curAccum = code;

		curBits += numBits;

		while (curBits >= 8) {
			Add((byte)(curAccum & 0xff), outs);
			curAccum >>= 8;
			curBits -= 8;
		}

		// If the next entry is going to be too big for the code size,
		// then increase it, if possible.
		if (freeEnt > maxCode || clearFlag) {
			if (clearFlag) {
				maxCode = MaxCode(numBits = gInitBits);
				clearFlag = false;
			} else maxCode = ++numBits == MaxBits ? MaxMaxCode : MaxCode(numBits);
		}

		if (code != endOfFileCode)
			return;
		// At EOF, write the rest of the buffer.
		while (curBits > 0) {
			Add((byte)(curAccum & 0xff), outs);
			curAccum >>= 8;
			curBits -= 8;
		}
		Flush(outs);
	}
}