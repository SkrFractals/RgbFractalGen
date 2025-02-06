using System.Collections.Generic;

namespace RgbFractalGenCs;
/// <summary>
/// Fractal Definitions
/// </summary>
internal class Fractal {

	internal static List<(string, CutFunction)> cutFunctions = [
		("NoChildSimple", NoChildSimple),													// 0
		("NoChildComplexAngled", NoChildComplexAngled),											// 1
		("NoBackDiag_Seeds", NoBackDiag),													// 2
		("TriComb_Seeds", TriComb),															// 3
		("Tetraflake_Seeds", Tetraflake_Symmetric),											// 4
		("Tetraflake_Symmetric_RadHoles_Seeds", Tetraflake_Symmetric_RadHoles),				// 5
		("Tetraflake_Symmetric_CornerHoles_Seeds", Tetraflake_Symmetric_CornerHoles),		// 6
		("Tetraflake_Symmetric_TriangleHoles_Seeds", Tetraflake_Symmetric_TriangleHoles),	// 7
		("Beamtree_NoBeam", Beamtree_NoBeam),												// 8
		("Beamtree_OuterJoint", Beamtree_OuterJoint),										// 9
		("Beamtree_InnerJoint", Beamtree_InnerJoint),										// 10
		("Carpet_Symmetric",Carpet_Symmetric),												// 11
		("NoChildComplexNoAngled", NoChildComplexNoAngled),									// 12
	];

	internal delegate long CutFunction(int index, long flags, Fractal f);
	// Properties
	internal string name;       // Fractal name (only for selection list)
	internal int childCount;    // ChildCount of Self Similars Inside (must equal the length of all the following arrays)
	internal float childSize;   // Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)
	internal float maxSize;     // The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)
	internal float minSize;     // How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)
	internal float cutSize;     // A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)
	internal float[]
		childX;                 // X coord shifts of Self Similars Inside
	internal float[] 
		childY;                 // Y coord shifts of Self Similars Inside
	internal List<(string, float[])> 
		childAngle;				// Angle shifts of Self Similars Inside
	internal List<(string, byte[])> 
		childColor;				// Color shifts of Self Similars Inside
	internal List<(int, int[])> 
		cutFunction;            // Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars
	internal string path = "";	// where was it loaded from?
	internal bool edit = false; // is it edited?

	// Autoproperties
	private readonly int nbdbits;
	/// <summary>
	/// Definition contructor
	/// </summary>
	/// <param name="name">Fractal name (only for selection list)</param>
	/// <param name="childCount">ChildCount of Self Similars Inside (must equal the length of all the following arrays)</param>
	/// <param name="childSize">Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)</param>
	/// <param name="maxSize">The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)</param>
	/// <param name="minSize">How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)</param>
	/// <param name="cutSize">A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)</param>
	/// <param name="childX">X coord shifts of Self Similars Inside</param>
	/// <param name="childY">Y coord shifts of Self Similars Inside</param>
	/// <param name="childAngle">Angle shifts of Self Similars Inside</param>
	/// <param name="childColor">Color shifts of Self Similars Inside</param>
	/// <param name="cutFunction">Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars</param>
	public Fractal(
		string name,
		int childCount,
		float childSize,
		float maxSize,
		float minSize,
		float cutSize,
		float[] childX,
		float[] childY,
		List<(string, float[])> childAngle,
		List<(string, byte[])> childColor,
		List<(int, int[])> cutFunction
	) {
		this.name = name;
		this.childCount = childCount;
		this.childSize = childSize;
		this.maxSize = maxSize;
		this.minSize = minSize;
		this.cutSize = cutSize;
		this.childX = childX;
		this.childY = childY;
		this.childAngle = childAngle;
		this.childColor = childColor;
		this.cutFunction = cutFunction;
		nbdbits = (childCount / 2) * ((childCount - 1) / 2);//((childCount - 3) / 2) * ((childCount - 1) / 2) + (childCount / 2 - 1) * ((childCount + 1) % 2);
	}
	/// <summary>
	/// Copy constructor
	/// </summary>
	/// <param name="copy">Parent practal to copy properties from</param>
	/// <param name="name">New copy name</param>
	/// <param name="childCount">If not -1, replace childCount with this one</param>
	/// <param name="childAngle">If not null, replace childAngle with this one</param>
	/// <param name="childColor">If not null, replace childColor with this one</param>
	/// <param name="cutFunction">If not null, replace cutFunction with this one</param>
	public Fractal(
		Fractal copy,
		string name,
		int childCount,
		List<(string, float[])> childAngle,
		List<(string, byte[])> childColor,
		List<(int, int[])> cutFunction
	) {
		childSize = copy.childSize;
		maxSize = copy.maxSize;
		minSize = copy.minSize;
		cutSize = copy.cutSize;
		childX = copy.childX;
		childY = copy.childY;
		this.name = name;
		this.childCount = childCount < 0 ? copy.childCount : childCount;
		this.childAngle = childAngle ?? copy.childAngle;
		this.childColor = childColor ?? copy.childColor;
		this.cutFunction = cutFunction ?? copy.cutFunction;
		nbdbits = (childCount / 2) * ((childCount - 1) / 2);//((childCount-3) / 2) * ((childCount - 1) / 2) + (childCount / 2 - 1) * ((childCount + 1) % 2);
	}
	/// <summary>
	/// Copy function
	/// </summary>
	/// <param name="name">New copy name</param>
	/// <param name="childCount">If not -1, replace childCount with this one</param>
	/// <param name="childAngle">If not nullptr, replace childAngle with this one</param>
	/// <param name="childColor">If not nullptr, replace childColor with this one</param>
	/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
	/// <returns>Copy fractal</returns>
	public Fractal Copy(
		string name,
		int childCount,
		List<(string, float[])> childAngle,
		List<(string, byte[])> childColor,
		List<(int, int[])> cutFunction
	) => new(this, name, childCount, childAngle, childColor, cutFunction);
	/// <summary>
	/// Copy function
	/// </summary>
	/// <param name="name">New copy name</param>
	/// <param name="childAngle">If not nullptr, replace childAngle with this one</param>
	/// <param name="childColor">If not nullptr, replace childColor with this one</param>
	/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
	/// <returns>Copy fractal</returns>
	public Fractal Copy(
		string name,
		List<(string, float[])> childAngle,
		List<(string, byte[])> childColor,
		List<(int, int[])> cutFunction
	) => new(this, name, childCount, childAngle, childColor, cutFunction);
	/// <summary>
	/// Copy function
	/// </summary>
	/// <param name="name">New copy name</param>
	/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
	/// <returns>Copy fractal</returns>
	public Fractal Copy(
		string name,
		List<(int, int[])> cutFunction
	) => new(this, name, childCount, null, null, cutFunction);


	#region CutFunctions_UniversalSeeds
	public static (long, long, long, long, long) Param(long inFlags, int flagbits, int rulebits) {
		if (inFlags < 0)
			inFlags = ((-inFlags) & ((1 << rulebits) - 1)) << flagbits;
		long mask = (1 << flagbits) - 1;
		return (
			inFlags,										// inFlags
			inFlags & ~mask,								// newFlags
			(inFlags >> flagbits) & ((1 << rulebits) - 1),	// rules
			inFlags & mask,									// flags
			mask											// mask
		);
	}
	public static long NoChildSimple(int index, long inFlags, Fractal f) {
		// Not a very good one...
		(_, var newFlags, var rules, var flags, _) = Param(inFlags, f.childCount - 1, f.childCount - 1);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, rules, flags);
	}
	public static long NoChildComplexAngled(int index, long inFlags, Fractal f) {
		int c = f.childCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.childCount, f.childCount + 1);
		return index > 0 ? ApplyNoChildComplexAngle(index, rules, flags, c, c) | (rules << f.childCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
	}
	public static long NoChildComplexNoAngled(int index, long inFlags, Fractal f) {
		int c = f.childCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.childCount, f.childCount + 1);
		return index > 0 ? ApplyNoChildComplexNoAngle(index, rules, flags, c, c) | (rules << f.childCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
	}
	public static long NoBackDiag(int index, long inFlags, Fractal f) {
		int c = f.childCount - 1;
		(inFlags, var newFlags, var rules, var flags, var _) = Param(inFlags, c, f.nbdbits);
		if (index == 0)
			return newFlags;
		int p, m, n = 1, ic = index + c - 1;
		/*for (int i = 1; i <= c - 2; ++i) 
			for (int b = i / 2; ++b <= i; n *= 2) 
				if ((rules & n) > 0) 
					if (( (flags >> (ic - (p = b)) % c & flags >> (ic + (m = i - b + 1)) % c)
						| (flags >> (ic + p) % c & flags >> (ic - m) % c)
						& 1) == 1) return -1;*/

		for (int i = 1; i <= c - 2; ++i)
			for (int b = i / 2; ++b <= i; n *= 2)
				if ((rules & n) > 0) {
					p = b; m = i - b + 1;
					if (( (flags >> (ic - p) % c & flags >> (ic + m) % c) & 1) == 1)
						return -1;
					if (( (flags >> (ic + p) % c & flags >> (ic - m) % c) & 1) == 1)
						return -1;
				}
		--ic;
		//rules += n; // skipped a lot of empty params at the beginning, but not needed anymore with hashes
		for (int i = c / 2; 0 <= --i; n *= 2)
			if ((rules & n) > 0) {
				if ((inFlags >> (index + i) % c & 1) == 1)
					newFlags |= (long)1 << (index + i) % c;
				if ((inFlags >> (ic - i) % c & 1) == 1)
					newFlags |= (long)1 << (ic - i) % c;
			}
		/*for (int c2 = c / 2, i = c2 - 2 + (c % 2); i <= c2; ++i) 
			if ((inFlags >> (index + i) % c & 1) == 1)
				newFlags |= 1 << (index + i) % c;*/
		newFlags |= (long)1 << (index - 1);
		return newFlags;
	}
	#endregion

	#region CutFunctions_Seeds
	public static long TriComb(int index, long inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, _, _) = Param(inFlags, f.childCount, 6);
		if ((inFlags >> index & 1) == 1)
			return -1;
		if ((rules & 1) > 0 && index > 3 && index < 10) // Med
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9; // Inner
		if ((rules & 2) > 0 && index > 3 && index < 10) // Med
			newFlags |= 1 << 7 | 1 << 6 | 1 << 11; // Outer
		if ((rules & 4) > 0 && index > 9 && index < 13) // Out
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if ((rules & 8) > 0 && index > 0 && index < 4) // In
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if ((rules & 16) > 0 && index > 0 && index < 4)
			newFlags |= 1 << 0;
		if ((rules & 32) > 0 && index > 3 && index < 10)
			newFlags |= 1 << 0;
		return newFlags;
	}
	private static long ApplyNoChildSimple(int index, long rules, long flags) => (flags >> (index - 1) & 1) == 1 ? -1 : (flags | rules);
	private static long ApplyNoChildComplexAngle(int index, long rules, long flags, int lastRule, int lastIndex) {

		long newFlags = flags & (1 << (lastIndex));
		long cb = 1 << lastIndex;
		if ((rules & cb) > 0)
			newFlags |= cb;
		if ((rules & (2 << lastRule)) > 0) {
			if ((newFlags & cb) > 0)
				newFlags &= ~cb;
			else
				newFlags |= cb;
		}
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		long m = (1 << lastRule) - 1, r = rules & m;
		return newFlags | ((r >> (lastRule - index + 1)) | (r << (index - 1))) & m;
	}
	private static long ApplyNoChildComplexNoAngle(int index, long rules, long flags, int lastRule, int lastIndex) {

		long newFlags = flags & (1 << (lastIndex));
		long cb = 1 << lastIndex;
		if ((rules & cb) > 0)
			newFlags |= cb;
		if ((rules & (2 << lastRule)) > 0) {
			if ((newFlags & cb) > 0)
				newFlags &= ~cb;
			else
				newFlags |= cb;
		}
		return (flags >> (index - 1) & 1) == 1 ? -1 : newFlags | (rules & ((1 << lastRule) - 1));
		//long m = (1 << lastRule) - 1, r = rules & m;
		//return newFlags | ((r >> (lastRule - index + 1)) | (r << (index - 1))) & m;
	}
	private static long GetCarpetSymmetricRules(long rules)
			=> (rules & (1 << 0))		                    // 1->1
			+ (rules & (1 << 1)) * ((1 << 0) + (1 << 6))    // 2->2+8 (2 and 8 are symmetric)
			+ (rules & (1 << 2)) * ((1 << 0) + (1 << 4))    // 3->3+7 (3 and 7 are symmetric)
			+ (rules & (1 << 3)) * ((1 << 0) + (1 << 2))    // 4->4+6 (4 and 6 are symmetric)
			+ (rules & (1 << 4))							// 5->5
			+ (rules & (1 << 5)) * (1 << 3)					// 6->9 primary child bit
			+ (rules & (1 << 6)) * (1 << 3);                // 7->10 second primary child bit
	public static long Carpet_Symmetric(int index, long inFlags, Fractal f) {
		int c = f.childCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.childCount, f.childCount + 1 - 3);
		return index > 0 ? ApplyNoChildComplexAngle(index, GetCarpetSymmetricRules(rules), flags, c, c) | (rules << f.childCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
	}
	private static long GetTetraflakeSymmetricRules(long rules)
			=> (rules & (1 << 0))                           // 1->1
			+ (rules & (1 << 1)) * ((1 << 0) + (1 << 1))    // 2->2+3 (2 and 3 are symmetric)
			+ (rules & (1 << 2)) * ((1 << 1) + (1 << 4))    // 3->4+7 (4 and 7 are symmetric)
			+ (rules & (1 << 3)) * ((1 << 1) + (1 << 5))    // 4->5+9 (5 and 9 are symmetric)
			+ (rules & (1 << 4)) * ((1 << 1) + (1 << 3))    // 5->6+8 (6 and 8 are symmetric)
			+ (rules & (1 << 5)) * (1 << 4)                 // 6->10
			+ (rules & (1 << 6)) * ((1 << 4) + (1 << 5))    // 7->11+12 (11 and 12 are symmetric)
			+ (rules & (1 << 7)) * (1 << 5)                 // 8->13
			+ (rules & (1 << 8)) * ((1 << 5) + (1 << 6))    // 9->14+15 (14 and 15 are symmetric)
			+ (rules & (1 << 9)) * (1 << 6)					// 10->16 primary child bit
			+ (rules & (1 << 10)) * (1 << 6);				// 11->17 second primary child bit
	public static long Tetraflake_Symmetric(int index, long inFlags, Fractal f) {
		int c = f.childCount - 1;
		// 15 secondary children bits + 1 primary child bit , 15 secondary children rules + 2 primary child bits - 6 symmetric pairs
		//(_, var newFlags, var rules, var flags, _) = Param(inFlags, c, c - 6); 
		//return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraflakeSymmetricRules(rules), flags) | (rules << c);

		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.childCount, f.childCount + 1 - 6);
		return index > 0 ? ApplyNoChildComplexNoAngle(index, GetTetraflakeSymmetricRules(rules), flags, c, c) | (rules << f.childCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
		//return index == 0 && (inFlags & (1 << c)) > 0 ? -1 : index == 0 ? newFlags : ApplyNoChildComplexNoAngle(index, GetTetraflakeSymmetricRules(rules), flags, c, c ) | (rules << f.childCount);
	}
	public static long Tetraflake_Symmetric_RadHoles(int index, long inFlags, Fractal f) {
		if (index >= 13)
			return -1;
		int c = f.childCount - 1;
		(_, var newFlags, var rules, var flags, _) = Param(inFlags, c, c - 6 - 2);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraflakeSymmetricRules(rules), flags) | (rules << c); //12
	}
	public static long Tetraflake_Symmetric_CornerHoles(int index, long inFlags, Fractal f) {
		if (index is >= 10 and < 13)
			return -1;
		int c = f.childCount - 1;
		(_, var newFlags, var rules, var flags, _) = Param(inFlags, c, c - 6);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraflakeSymmetricRules(rules), flags) | (rules << c); //9
	}
	public static long Tetraflake_Symmetric_TriangleHoles(int index, long inFlags, Fractal f) {
		if (index is > 0 and <= 3)
			return -1;
		int c = f.childCount - 1;
		(_, var newFlags, var rules, var flags, _) = Param(inFlags, c, c - 6);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraflakeSymmetricRules(rules), flags) | (rules << c); //12
	}
	public static long Beamtree_NoBeam(int index, long inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, _, _) = Param(inFlags, f.childCount - 1, 2);
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		if ((rules & 1) > 0)
			newFlags |= (long)1 << (3 * index + 2) % 6;// 002244 InnerBeam
		if ((rules & 2) > 0)
			newFlags |= (long)1 << (3 * index + 5) % 6;// 002244 OuterBeam
		return newFlags;
	}
	#endregion

	#region CutFunctions_Special
	public static long Beamtree_OuterJoint(int index, long inFlags, Fractal f) {
		if (inFlags < 0) inFlags = 0; // No params
		long newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= (long)1 << (12 - index) % 6;
		return newFlags;
	}
	public static long Beamtree_InnerJoint(int index, long inFlags, Fractal f) {
		if (inFlags < 0) inFlags = 0; // No params
		long newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= (long)1 << (index + 5) % 6;
		return newFlags;
	}
	#endregion

	#region DeprecatedCutFunctions
	/*public static int FlakeTemplate_WithCenters(int index, int inFlags, int bits) {
		(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, bits, bits);
		if (index == 0)
			return newFlags;
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= ((rules >> (bits - index + 1)) | (rules << (index - 1))) & mask;
		return newFlags;
	}*/
	/*public static int Triflake_NoBackDiag(int index, int inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, var flags, var _) = Param(inFlags, f.childCount, f.nbdbits);
		int c = f.childCount - 1; // 3
		if (index == 0)
			return newFlags;
		if ((flags >> (index + 1) % c & flags >> (index + 1) % c & 1) == 1)
			return -1;
		//-012
		//-II-
		for (int c2 = c / 2, i = c2 - 2 + (c % 2); i <= c2; ++i)
			if ((inFlags >> (index + i) % c & 1) == 1)
				newFlags |= 1 << (index + i) % c;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}*/
	/*public static int Pentaflake_NoBackDiag(int index, int inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, var flags, var _) = Param(inFlags, f.childCount, f.nbdbits);
		int c = f.childCount - 1;
		//if (inFlags < 0) inFlags = 0; // No params
		//var newFlags = 0;
		if (index == 0)
			return newFlags;

		// Diagonal Inner (I)
		if ((rules & 1) > 0) {
			if ((flags >> (index + 2) % c & flags >> (index + 0) % c & 1) == 1)
				return -1;
			if ((flags >> (index + 3) % c & flags >> (index + 1) % c & 1) == 1)
				return -1;
		}
		//-01234
		//--II--
		for (int c2 = c / 2, i = c2 - 2 + (c % 2); i <= c2; ++i)
			if ((inFlags >> (index + i) % c & 1) == 1)
				newFlags |= 1 << (index + i) % c;
		
		newFlags |= 1 << (index - 1);
		return newFlags;
	}*/
	/*public static int Hexaflake_NoBackDiag(int index, int inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, var flags, var _) = Param(inFlags, f.childCount, f.nbdbits);

		--f.childCount; // 6
		//if (inFlags < 0) inFlags = 0; // No params
		//var newFlags = 0;
		if (index == 0)
			return newFlags;

		int q = f.childCount / 4; // 1

		// Diagonal Inner (I)
		if ((rules & 1) > 0)
			if ((flags >> (index + 1) % f.childCount & flags >> (index + 3) % f.childCount & 1) == 1)
			return -1; // diagonals (LL+RR) 

		//BackNeighbors +-1 (M)
		if ((rules & 2) > 0) {
			if ((flags >> (index + 0) % f.childCount & flags >> (index + 3) % f.childCount & 1) == 1)
				return -1; // backwards (L+RR)
			if ((flags >> (index + 1) % f.childCount & flags >> (index + 4) % f.childCount & 1) == 1)
				return -1; // backwards (LL+R)
		}

		for (int c = f.childCount / 2, i = c - 2 + (f.childCount % 2); i <= c; ++i)
			if ((inFlags >> (index + i) % f.childCount & 1) == 1)
				newFlags |= 1 << (index + i) % f.childCount;
		//-012345
		//--III--
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
	public static int Carpet_NoBackDiag(int index, int inFlags, Fractal f) {
		--f.childCount; // 8
		(inFlags, var newFlags, var rules, var flags, var _) = Param(inFlags, f.childCount, f.nbdbits);
		//if (inFlags < 0) inFlags = 0; // No params
		//var newFlags = 0;
		if (index == 0)
			return newFlags;

		
		// Diagonal Inner (I)
		if ((rules & 1) > 0) {
			if ((flags >> (index + 1) % f.childCount & flags >> (index + 4) % f.childCount & 1) == 1)
				return -1;
			if ((flags >> (index + 2) % f.childCount & flags >> (index + 5) % f.childCount & 1) == 1)
				return -1;
		}

		//BackNeighbors +-1 (M)
		if ((rules & 2) > 0) {
			if ((flags >> (index + 2) % f.childCount & flags >> (index + 6) % f.childCount & 1) == 1)
				return -1;
			if ((flags >> (index + 0) % f.childCount & flags >> (index + 4) % f.childCount & 1) == 1)
				return -1;
		}

		// BackNeighbor +-2 (B)
		if ((rules & 4) > 0) {
			if ((flags >> (index + 1) % f.childCount & flags >> (index + 5) % f.childCount & 1) == 1)
				return -1;
		}

		//-01234567
		//---III---
		for (int c = f.childCount / 2, i = c - 2 + (f.childCount % 2); i <= c; ++i)
			if ((inFlags >> (index + i) % f.childCount & 1) == 1)
				newFlags |= 1 << (index + i) % f.childCount;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}
	public static int Carpet_NoBack_IMB(int index, int inFlags, Fractal f) {
		--f.childCount; // 8
		if (inFlags < 0) inFlags = 0; // No params
		var newFlags = 0;
		if (index == 0)
			return newFlags;

		// Diagonal Inner (I)
		if ((inFlags >> (index + 1) % f.childCount & inFlags >> (index + 4) % f.childCount & 1) == 1)
			return -1;
		if ((inFlags >> (index + 2) % f.childCount & inFlags >> (index + 5) % f.childCount & 1) == 1)
			return -1;

		//BackNeighbors +-1 (M)
		if ((inFlags >> (index + 2) % f.childCount & inFlags >> (index + 6) % f.childCount & 1) == 1)
			return -1; 
		if ((inFlags >> (index + 0) % f.childCount & inFlags >> (index + 4) % f.childCount & 1) == 1)
			return -1;

		// BackNeighbor +-2 (B)
		if ((inFlags >> (index + 1) % f.childCount & inFlags >> (index + 5) % f.childCount & 1) == 1)
			return -1;

		for (int c = f.childCount / 2, i = c - 2 + (f.childCount % 2); i <= c; ++i)
			if ((inFlags >> (index + i) % f.childCount & 1) == 1)
				newFlags |= 1 << (index + i) % f.childCount;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}*/
	/*public static int Pentaflake_NoOuterCenter(int index, int inFlags) {
	int newFlags = 0;
		if (inFlags == 1 << index)
			return -1;
		if (index == 0)
			return newFlags;
		newFlags = 1;
		return newFlags;
	}
	public static int Pentaflake_NoZigCenter(int index, int inFlags) {
		int newFlags = 0;
		if (inFlags == 1 << index)
			return -1;
		if (index == 0)
			return newFlags;
		newFlags = inFlags == 0 ? 1 : 0;
		return newFlags;
	}*/
	/*public static int Beamtree_InnerBeam(int index, int inFlags) {
if (inFlags < 0) inFlags = 0; // No params
int newFlags = 0;
if (index < 4)
	return newFlags;
if ((inFlags >> (index - 4) & 1) == 1)
	return -1;
newFlags |= 1 << (3 * index + 2) % 6;// 002244 InnerBeam
return newFlags;
}
public static int Beamtree_OuterBeam(int index, int inFlags) {
if (inFlags < 0) inFlags = 0; // No params
int newFlags = 0;
if (index < 4)
	return newFlags;
if ((inFlags >> (index - 4) & 1) == 1)
	return -1;
newFlags |= 1 << (3 * index + 5) % 6;// 002244 OuterBeam
return newFlags;
}*/
	/*public static int Triflake_NoCenter(int index, int inFlags) {
(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, 3 + 1, 3);
if (index == 0 && (inFlags & (1 << 3)) > 0)
	return -1;
if (index == 0)
	return newFlags;
else
	newFlags |= 1 << 3;
if ((flags >> (index - 1) & 1) == 1)
	return -1;
newFlags |= ((rules >> (3 - index + 1)) | (rules << (index - 1))) & mask;
return newFlags;
}*/
	/*public static int TriComb_NoCenter3(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 0 && index < 4)
			newFlags |= 1 << 0;
		return newFlags;
	}
	public static int TriComb_NoCenter6(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 3 && index < 10)
			newFlags |= 1 << 0;
		return newFlags;
	}
	public static int TriComb_NoMedInner(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 3 && index < 10) // Med
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9; // Inner
		return newFlags;
	}
	public static int TriComb_NoMedOuter(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 3 && index < 10) // Med
			newFlags |= 1 << 7 | 1 << 6 | 1 << 11; // Outer
		return newFlags;
	}
	public static int TriComb_NoInMed(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 3 && index < 10) // Med
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9; // Inner
		if (index > 0 && index < 4) // In
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		return newFlags;
	}
	public static int TriComb_NoInOut(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> (index) & 1) == 1)
			return -1;
		if (index > 9 && index < 13) // Out
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if (index > 0 && index < 4) // In
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		return newFlags;
	}
	public static int TriComb_NoMedInOut(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> index & 1) == 1)
			return -1;
		if (index > 9 && index < 13) // Out
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if (index > 3 && index < 10) // Med
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9; // Inner
		return newFlags;
	}
	public static int TriComb_NoMedOutOut(int index, int inFlags) {
		if (inFlags < 0) inFlags = 0; // No params
		int newFlags = 0;
		if ((inFlags >> (index) & 1) == 1)
			return -1;
		if (index > 9 && index < 13) // Out
			newFlags |= 1 << 1 | 1 << 4 | 1 << 9;
		if (index > 3 && index < 10) // Med
			newFlags |= 1 << 7 | 1 << 6 | 1 << 11; // Outer
		return newFlags;
	}*/
	/*public static int Trees_NoChildParam(int index, int inFlags) {

		(inFlags, int newFlags, int big, int smol, int mask) = Param(inFlags, 11);
		if (index == 0)
			return newFlags;
		if ((smol >> (index - 1) & 1) == 1)
			return -1;

		newFlags |= ((big >> (9 - index)) | (big << (index))) & mask;//1 << 0;
		return newFlags;
	}*/
	/*public static int Pentaflake_NoOuter(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << (index + 4) % 5;
		return newFlags;
	}
	public static int Pentaflake_NoSide(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << index % 5;
		return newFlags;
	}
	public static int Pentaflake_NoInner(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << (index + 1) % 5;
		return newFlags;
	}
	public static int Pentaflake_NoFullSide(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << index % 5;
		newFlags |= 1 << (index + 1) % 5;
		return newFlags;
	}
	public static int Pentaflake_NoZigSide(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << index % 5;
		newFlags |= 1 << (index + 2) % 5;
		return newFlags;
	}
	public static int Pentaflake_NoOuterInner(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index - 1) & 1) == 1)
			return -1;
		newFlags |= 1 << (index + 4) % 5; // Outer
		newFlags |= 1 << (index + 1) % 5; // Inner
		return newFlags;
	}
	public static int Pentaflake_NoOuterCenter(int index, int inFlags) {
		int newFlags = 0;
		if (inFlags == 1 << index)
			return -1;
		if (index == 0)
			return newFlags;
		newFlags = 1;
		return newFlags;
	}
	public static int Pentaflake_NoZigCenter(int index, int inFlags) {
		int newFlags = 0;
		if (inFlags == 1 << index)
			return -1;
		if (index == 0)
			return newFlags;
		newFlags = inFlags == 0 ? 1 : 0;
		return newFlags;
	}*/
	/*public static int Hexaflake_NoCornerOuter(int index, int inFlags) {
		int newFlags = 0;
		if (index == 0)
			return newFlags;
		if ((inFlags >> (index + 5) % 6 & 1) == 1) // ...(index + N)... N: 5=Outer 4=CornOuter 3=CornInnerSide 2=CornInner
			return -1;
		newFlags |= 1 << (index - 1);
		return newFlags;
	}*/
	/*public static int Beamtree_Param1(int index, int inFlags) {
		return 0;

		(inFlags, int newFlags, int big, int smol) = Param(inFlags, 11);

		if (index == 0)
			return newFlags;

		if ((((smol >> (index)) | (smol << (9 - index ))) & big) > 0)
			return -1;

		newFlags |= 1 << (index - 1);

		return newFlags;
	}*/
	#endregion
}

/*internal class VecRefWrapper {
	internal Vector3[][] buffT;
	internal Vector3 H, I;
	internal (float, float, (float, float)[])[] preIterate;
	internal VecRefWrapper(Vector3[][] buffT, Vector3 I, Vector3 H) {
		this.buffT = buffT;
		this.H = H; this.I = I;
	}

}*/
