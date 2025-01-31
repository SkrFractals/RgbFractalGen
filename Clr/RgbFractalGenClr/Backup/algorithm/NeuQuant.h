/* NeuQuant Neural-Net Quantization Algorithm Interface
 * ----------------------------------------------------
 *
 * Copyright (c) 1994 Anthony Dekker
 *
 * NEUQUANT Neural-Net quantization algorithm by Anthony Dekker, 1994.
 * See "Kohonen neural networks for optimal colour quantization"
 * in "Network: Computation in Neural Systems" Vol. 5 (1994) pp 351-367.
 * for a discussion of the algorithm.
 * See also  http://members.ozemail.com.au/~dekker/NEUQUANT.HTML
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

#pragma once

#include <cstdio>
#include <cstdint>
#include <thread>

#define netsize		256			/* number of colours used */

 /* For 256 colours, fixed arrays need 8kb, plus space for the image
	---------------------------------------------------------------- */

	/* four primes near 500 - assume no image has a length so large */
	/* that it is divisible by all four primes */
#define prime1		499
#define prime2		491
#define prime3		487
#define prime4		503

#define minpicturebytes	(3*prime4)		/* minimum size for input image */

#define maxnetpos    255
#define netbiasshift    4            /* bias for colour values */
#define ncycles        100            /* no. of learning cycles */

       /* defs for freq and bias */
#define intbiasshift    16            /* bias for fractions */
#define intbias        65536
#define gammashift    10            /* gamma = 1024 */
#define gamma    1024
#define betashift    10
#define beta        64    /* beta = 1/1024 */
#define betagamma    65536

/* defs for decreasing radius factor */
#define initrad        32        /* for 256 cols, radius starts */
#define radiusbiasshift    6            /* at 32.0 biased by 6 bits */
#define radiusbias    64
#define initradius    2048    /* and decreases by a */
#define radiusdec    30            /* factor of 1/30 each cycle */

/* defs for decreasing alpha factor */
#define alphabiasshift    10            /* alpha starts at 1.0 */
#define initalpha    1024


/* radbias and alpharadbias used for radpower calculation */
#define radbiasshift    8
#define radbias        256
#define alpharadbshift  18
#define alpharadbias    262144

public struct NeuQuant {
    /* Types and Original Variables
       ---------------------------- */
    const unsigned char* thepicture;
    int lengthcount;        /* lengthcount = H*W*3 */
    int samplefac;          /* sampling factor 1..30 */
    int samplepixels;
    typedef int pixel[4];   /* BGRc */
    pixel network[netsize]; /* the network itself */
    int netindex[256];      /* for network lookup - really 256 */
    int bias[netsize];      /* bias and freq arrays for learning */
    int freq[netsize];
    int radpower[initrad];  /* radpower for precomputation */

    int alphadec;                    /* biased by 10 bits */
    int factor;

    /* Initialise network in range (0,0,0) to (255,255,255) and set parameters
       ----------------------------------------------------------------------- */
    void initnet(const unsigned char* thepic, const int len, const int sample, const int samplepix);

    //int getNetwork(int i, int j);

    /* Unbias network to give byte values 0..255 and record position i to prepare for sort
       ----------------------------------------------------------------------------------- */
    void unbiasnet();	/* can edit this function to do output of colour map */

    /* Output colour map
       ----------------- */
       //void writecolourmap(FILE* f);

    void getcolourmap(uint8_t* colorMap);

    /* Insertion sort of network and building of netindex[0..255] (to do after unbias)
       ------------------------------------------------------------------------------- */
    void inxbuild();

    /* Search for BGR values 0..255 (after net is unbiased) and return colour index
       ---------------------------------------------------------------------------- */
    int inxsearch(int b, int g, int r);

private:
    int contest(int b, int g, int r);
    void altersingle(int alpha, int i, int b, int g, int r);
    void alterneigh(int rad, int i, int b, int g, int r);

public:
    /* Main Learning Loop
       ------------------ */
    void learn(System::Threading::CancellationToken* token);

};

/* Program Skeleton (old)
   ----------------------
  [select samplefac in range 1..30]
  pic = (unsigned char*) malloc(3*width*height);
  [read image from input file into pic]
	initnet(pic,3*width*height,samplefac);
	learn();
	unbiasnet();
	[write output image header, using writecolourmap(f),
	possibly editing the loops in that function]
	inxbuild();
	[write output image using inxsearch(b,g,r)]		*/