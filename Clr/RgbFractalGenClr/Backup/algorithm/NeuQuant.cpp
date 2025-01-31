#include "NeuQuant.h"

void NeuQuant::initnet(const unsigned char* thepic, const int len, const int sample, const int samplepix) {
    int i;
    int* p;

    thepicture = thepic;
    lengthcount = len;
    samplefac = sample;
    samplepixels = samplepix;

    for (i = 0; i < netsize; i++) {
        p = network[i];
        p[0] = p[1] = p[2] = (i << (netbiasshift + 8)) / netsize;
        freq[i] = intbias / netsize;    /* 1/netsize */
        bias[i] = 0;
    }
}


/* Unbias network to give byte values 0..255 and record position i to prepare for sort
   ----------------------------------------------------------------------------------- */

void NeuQuant::unbiasnet() {
    for (int i = 0, j, temp; i < netsize; i++) {
        for (j = 0; j < 3; j++) {
            /* OLD CODE: network[i][j] >>= netbiasshift; */
            /* Fix based on bug report by Juergen Weigert jw@suse.de */
            temp = (network[i][j] + (1 << (netbiasshift - 1))) >> netbiasshift;
            if (temp > 255) temp = 255;
            network[i][j] = temp;
        }
        network[i][3] = i;            /* record colour no */
    }
}


/* Output colour map
   ----------------- */

/*void writecolourmap(FILE* f) {
    int i, j;

    for (i = 2; i >= 0; i--)
        for (j = 0; j < netsize; j++)
            putc(network[j][i], f);
}*/

void NeuQuant::getcolourmap(uint8_t *colorMap) {
    int *index = new int[netsize];
    for (int i = 0; i < netsize; i++)
        index[network[i][3]] = i;
    int k = 0;
    for (int i = 0; i < netsize; i++) {
        int j = index[i];
        colorMap[k++] = network[j][2];
        colorMap[k++] = network[j][1];
        colorMap[k++] = network[j][0];
    }
    delete[] index;
}


/* Insertion sort of network and building of netindex[0..255] (to do after unbias)
   ------------------------------------------------------------------------------- */

void NeuQuant::inxbuild() {
    int i, j, smallpos, smallval;
    int *p, *q;
    int previouscol, startpos;

    previouscol = 0;
    startpos = 0;
    for (i = 0; i < netsize; i++) {
        p = network[i];
        smallpos = i;
        smallval = p[1];            /* index on g */
        /* find smallest in i..netsize-1 */
        for (j = i + 1; j < netsize; j++) {
            q = network[j];
            if (q[1] < smallval) {        /* index on g */
                smallpos = j;
                smallval = q[1];    /* index on g */
            }
        }
        q = network[smallpos];
        /* swap p (i) and q (smallpos) entries */
        if (i != smallpos) {
            j = q[0];
            q[0] = p[0];
            p[0] = j;
            j = q[1];
            q[1] = p[1];
            p[1] = j;
            j = q[2];
            q[2] = p[2];
            p[2] = j;
            j = q[3];
            q[3] = p[3];
            p[3] = j;
        }
        /* smallval entry is now in position i */
        if (smallval != previouscol) {
            netindex[previouscol] = (startpos + i) >> 1;
            for (j = previouscol + 1; j < smallval; j++) netindex[j] = i;
            previouscol = smallval;
            startpos = i;
        }
    }
    netindex[previouscol] = (startpos + maxnetpos) >> 1;
    for (j = previouscol + 1; j < 256; j++) netindex[j] = maxnetpos; /* really 256 */
}


/* Search for BGR values 0..255 (after net is unbiased) and return colour index
   ---------------------------------------------------------------------------- */

int NeuQuant::inxsearch(const int b, const int g, const int r) {
    int i = netindex[g],/* index on g */
        j = i - 1,      /* start at netindex[g] and work outwards */
        dist, a,
        bestd = 1000,   /* biggest possible dist is 256*3 */
        best = -1;
    int *p;
    while ((i < netsize) || (j >= 0)) {
        if (i < netsize) {
            if ((dist = (p = network[i])[1] - g) < bestd) { /* inx key */
                i++;
                if (dist < 0)
                    dist = -dist;
                if ((a = p[0] - b) < 0) 
                    a = -a;
                if ((dist += a) < bestd) {
                    if ((a = p[2] - r) < 0) 
                        a = -a;
                    if ((dist += a) < bestd) {
                        bestd = dist;
                        best = p[3];
                    }
                }
            } else i = netsize;    /* stop iter */
        }
        if (j >= 0) {
            if ((dist = g - (p = network[j])[1]) < bestd) { /* inx key - reverse dif */
                j--;
                if (dist < 0) 
                    dist = -dist;
                if ((a = p[0] - b) < 0) 
                    a = -a;
                if ((dist += a) < bestd) {
                    if ((a = p[2] - r) < 0) 
                        a = -a;
                    if ((dist += a) < bestd) {
                        bestd = dist;
                        best = p[3];
                    }
                }
            } else j = -1; /* stop iter */
        }
    }
    return (best);
}


/* Search for biased BGR values
   ---------------------------- */

int NeuQuant::contest(const int b, const int g, const int r) {
    /* finds closest neuron (min dist) and updates freq */
    /* finds best neuron (min dist-bias) and returns position */
    /* for frequently chosen neurons, freq[i] is high and bias[i] is negative */
    /* bias[i] = gamma*((1/netsize)-freq[i]) */

    int i, dist, a, biasdist, betafreq;
    int bestpos, bestbiaspos, bestd, bestbiasd;
    int *p, *f, *n;

    bestd = ~(((int) 1) << 31);
    bestbiasd = bestd;
    bestpos = -1;
    bestbiaspos = bestpos;
    p = bias;
    f = freq;

    for (i = 0; i < netsize; i++) {
        n = network[i];
        dist = n[0] - b;
        if (dist < 0) dist = -dist;
        a = n[1] - g;
        if (a < 0) a = -a;
        dist += a;
        a = n[2] - r;
        if (a < 0) a = -a;
        dist += a;
        if (dist < bestd) {
            bestd = dist;
            bestpos = i;
        }
        biasdist = dist - ((*p) >> (intbiasshift - netbiasshift));
        if (biasdist < bestbiasd) {
            bestbiasd = biasdist;
            bestbiaspos = i;
        }
        betafreq = (*f >> betashift);
        *f++ -= betafreq;
        *p++ += (betafreq << gammashift);
    }
    freq[bestpos] += beta;
    bias[bestpos] -= betagamma;
    return (bestbiaspos);
}


/* Move neuron i towards biased (b,g,r) by factor alpha
   ---------------------------------------------------- */

void NeuQuant::altersingle(const int alpha, const int i, const int b, const int g, const int r) {
    int *n;
//	printf("New point %d: ", i);        
    *(n = network[i]) -= (alpha * (*n - b)) / initalpha; /* alter hit neuron */
//	printf("%f, ", *n / 16.0);
    *(n++) -= (alpha * (*n - g)) / initalpha;
//  printf("%f, ", *n / 16.0);
    *(n++) -= (alpha * (*n - r)) / initalpha;
//  printf("%f\n", *n / 16.0);
}

/* Move adjacent neurons by precomputed alpha*(1-((i-j)^2/[r]^2)) in radpower[|i-j|]
   --------------------------------------------------------------------------------- */

void NeuQuant::alterneigh(const int rad, const int i, const int b, const int g, const int r) {
    int j = i + 1, k = i - 1, lo = i - rad, hi = i + rad, a;
    int *p, *q = radpower;
    if (lo < -1)
        lo = -1;
    if (hi > netsize) 
        hi = netsize;
    while (j < hi || k > lo) {
        a = (*(++q));
        if (j < hi) {
//		  printf("New point %d: ", j);
            *(p = network[j]) -= (a * (*p - b)) / alpharadbias;
//		  printf("%f, ", *p / 16.0);
            *(p++) -= (a * (*p - g)) / alpharadbias;
//		  printf("%f, ", *p / 16.0);
            *(p++) -= (a * (*p - r)) / alpharadbias;
//		  printf("%f\n", *p / 16.0);
            j++;
        }
        if (k > lo) {
//      printf("New point %d: ", k);
            *(p = network[k]) -= (a * (*p - b)) / alpharadbias;
//      printf("%f, ", *p / 16.0);
            *(p++) -= (a * (*p - g)) / alpharadbias;
//      printf("%f, ", *p / 16.0);
            *(p++) -= (a * (*p - r)) / alpharadbias;
//      printf("%f\n", *p / 16.0);
            k--;
        }
    }
}

/* Main Learning Loop
   ------------------ */

void NeuQuant::learn(System::Threading::CancellationToken* token) {
    int i, j, b, g, r;
    int radius, rad, alpha, step, delta, samplepixels;
    const unsigned char *p;
    const unsigned char *lim;

    alphadec = 30 + ((samplefac - 1) / 3);
    p = thepicture;
    lim = thepicture + lengthcount;
    //samplepixels = lengthcount / (3 * samplefac); // moved to constructor
    delta = samplepixels / ncycles;
    alpha = initalpha;
    radius = initradius;

    rad = radius >> radiusbiasshift;
    if (rad <= 1) rad = 0;
    for (i = 0; i < rad; i++)
        radpower[i] = alpha * (((rad * rad - i * i) * radbias) / (rad * rad));

//	fprintf(stderr,"beginning 1D learning: initial radius=%d\n", rad);

    if ((lengthcount % prime1) != 0) step = 3 * prime1;
    else {
        if ((lengthcount % prime2) != 0) step = 3 * prime2;
        else {
            if ((lengthcount % prime3) != 0) step = 3 * prime3;
            else step = 3 * prime4;
        }
    }

#define LEARN_BODY j = contest(b = p[0] << netbiasshift, g = p[1] << netbiasshift, r = p[2] << netbiasshift);\
    altersingle(alpha, j, b, g, r);\
    if (rad) alterneigh(rad, j, b, g, r);\
    if ((p += step) >= lim) p -= lengthcount;\
    if ((i++) % delta == 0) {\
        alpha -= alpha / alphadec;\
        if ((rad = (radius -= radius / radiusdec) >> radiusbiasshift) <= 1) rad = 0;\
        for (j = 0; j < rad; j++) radpower[j] = alpha * (((rad * rad - j * j) * radbias) / (rad * rad));\
    }

    if (token == nullptr) {
        for (int i = 0, x = samplepixels; x > 0; --x) {
            LEARN_BODY
        }
        return;
    }
    auto& tokenR = *token;
    samplepixels /= factor;
    for (int i = 0, x = factor; x > 0; --x) {
        if (tokenR.IsCancellationRequested)
            return;
        for (int y = samplepixels; y > 0; --y) {
            LEARN_BODY
        }
    }
//	fprintf(stderr,"finished 1D learning: final alpha=%f !\n",((float)alpha)/initalpha);
}
