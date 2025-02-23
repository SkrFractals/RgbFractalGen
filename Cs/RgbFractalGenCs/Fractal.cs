using System.Collections.Generic;

namespace RgbFractalGenCs;
/// <summary>
/// Fractal Definitions
/// </summary>
internal class Fractal {

	internal static readonly List<(string, CutFunction)> cutFunctions = [
		("NoCornersSimple", NoChildSimple),						// 0
		("NoChildAngled", NoChildAngled),						// 1
		("NoBackDiag", NoBackDiag),								// 2
		("TriComb", TriComb),									// 3
		("TetraFlake_Symmetric", TetraFlakeSymmetric),			// 4
		("RadHoles", TetraFlakeSymmetricRadHoles),				// 5
		("CornerHoles", TetraFlakeSymmetricCornerHoles),		// 6
		("TriangleHoles", TetraFlakeSymmetricTriangleHoles),	// 7
		("BeamTree_NoBeam", BeamTreeNoBeam),					// 8
		("BeamTree_OuterJoint", BeamTreeOuterJoint),			// 9
		("BeamTree_InnerJoint", BeamTreeInnerJoint),			// 10
		("Carpet_Symmetric",CarpetSymmetric),					// 11
		("NoChildNoAngled", NoChildNoAngled)					// 12
	];

	internal delegate long CutFunction(int index, long flags, Fractal f);
	// Properties
	internal string Name;       // Fractal name (only for selection list)
	internal int ChildCount;    // ChildCount of children Inside (must equal the length of all the following arrays)
	internal double ChildSize;   // Scale Of children Inside (how much to scale the image when switching parent-child)
	internal double MaxSize;     // The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)
	internal double MinSize;     // How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)
	internal double CutSize;     // A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not you can decrease to boost performance)
	internal double[]
		ChildX;                 // X coordinate shifts of children inside
	internal double[]
		ChildY;                 // Y coordinate shifts of children inside
	internal readonly List<(string, double[])>
		ChildAngle;             // Angle shifts of children inside
	internal readonly List<(string, short[])>
		ChildColor;             // Color shifts of children inside
	internal readonly List<(int, int[])>
		ChildCutFunction;            // Function that takes a bitarray transforms it and decides to cut some specific patterns of children
	internal string Path = "";  // where was it loaded from?
	internal bool Edit = false; // is it edited?

	// AutoProperties
	private readonly int noBackDiagBits;

	/// <summary>
	/// Definition constructor
	/// </summary>
	/// <param name="name">Fractal name (only for selection list)</param>
	/// <param name="childCount">ChildCount of children Inside (must equal the length of all the following arrays)</param>
	/// <param name="childSize">Scale Of children Inside (how much to scale the image when switching parent-child)</param>
	/// <param name="maxSize">The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)</param>
	/// <param name="minSize">How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)</param>
	/// <param name="cutSize">A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not you can decrease to boost performance)</param>
	/// <param name="childX">X coordinate shifts of children Inside</param>
	/// <param name="childY">Y coordinate shifts of children Inside</param>
	/// <param name="childAngle">Angle shifts of children Inside</param>
	/// <param name="childColor">Color shifts of children Inside</param>
	/// <param name="childCutFunction">Function that takes a bitarray transforms it and decides to cut some specific patterns of children</param>
	public Fractal(
		string name,
		int childCount,
		double childSize,
		double maxSize,
		double minSize,
		double cutSize,
		double[] childX,
		double[] childY,
		List<(string, double[])> childAngle,
		List<(string, short[])> childColor,
		List<(int, int[])> childCutFunction
	) {
		Name = name;
		ChildCount = childCount;
		ChildSize = childSize;
		MaxSize = maxSize;
		MinSize = minSize;
		CutSize = cutSize;
		ChildX = childX;
		ChildY = childY;
		ChildAngle = childAngle;
		ChildColor = childColor;
		ChildCutFunction = childCutFunction;
		noBackDiagBits = childCount / 2 * ((childCount - 1) / 2);
	}
	/// <summary>
	/// Copy constructor
	/// </summary>
	/// <param name="copy">Parent fractal to copy properties from</param>
	/// <param name="name">New copy name</param>
	/// <param name="childCount">If not -1, replace childCount with this one</param>
	/// <param name="childAngle">If not null, replace childAngle with this one</param>
	/// <param name="childColor">If not null, replace childColor with this one</param>
	/// <param name="cutFunction">If not null, replace cutFunction with this one</param>
	private Fractal(
		Fractal copy,
		string name,
		int childCount,
		List<(string, double[])> childAngle,
		List<(string, short[])> childColor,
		List<(int, int[])> cutFunction
	) {
		ChildSize = copy.ChildSize;
		MaxSize = copy.MaxSize;
		MinSize = copy.MinSize;
		CutSize = copy.CutSize;
		ChildX = copy.ChildX;
		ChildY = copy.ChildY;
		Name = name;
		ChildCount = childCount < 0 ? copy.ChildCount : childCount;
		ChildAngle = childAngle ?? copy.ChildAngle;
		ChildColor = childColor ?? copy.ChildColor;
		ChildCutFunction = cutFunction ?? copy.ChildCutFunction;
		noBackDiagBits = childCount / 2 * ((childCount - 1) / 2);
	}
	public Fractal(Fractal copy) {
		ChildSize = copy.ChildSize;
		MaxSize = copy.MaxSize;
		MinSize = copy.MinSize;
		CutSize = copy.CutSize;
		ChildX = copy.ChildX;
		ChildY = copy.ChildY;
		Name = copy.Name;
		ChildCount = copy.ChildCount;
		ChildAngle = copy.ChildAngle;
		ChildColor = copy.ChildColor;
		ChildCutFunction = copy.ChildCutFunction;
		noBackDiagBits = ChildCount / 2 * ((ChildCount - 1) / 2);
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
		List<(string, double[])> childAngle,
		List<(string, short[])> childColor,
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
		List<(string, double[])> childAngle,
		List<(string, short[])> childColor,
		List<(int, int[])> cutFunction
	) => new(this, name, ChildCount, childAngle, childColor, cutFunction);
	/// <summary>
	/// Copy function
	/// </summary>
	/// <param name="name">New copy name</param>
	/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
	/// <returns>Copy fractal</returns>
	public Fractal Copy(
		string name,
		List<(int, int[])> cutFunction
	) => new(this, name, ChildCount, null, null, cutFunction);

	#region CutFunctions_UniversalSeeds
	private static long ApplyNoChildSimple(int index, long rules, long flags) => (flags >> (index - 1) & 1) == 1 ? -1 : flags | rules;
	private static long ApplyNoChildComplexAngle(int index, long rules, long flags, int lastRule, int lastIndex) {
		var newFlags = flags & ((long)1 << lastIndex);
		var cb = (long)1 << lastIndex;
		if ((rules & cb) > 0)
			newFlags |= cb;
		if ((rules & ((long)2 << lastRule)) > 0)
			newFlags = (newFlags & cb) > 0 ? newFlags & ~cb : newFlags | cb;
		if ((flags >> (index - 1) & 1) == 1)
			return -1;
		long m = ((long)1 << lastRule) - 1, r = rules & m;
		return newFlags | ((r >> (lastRule - index + 1)) | (r << (index - 1))) & m;
	}
	private static long ApplyNoChildComplexNoAngle(int index, long rules, long flags, int lastRule, int lastIndex) {
		var newFlags = flags & ((long)1 << lastIndex);
		var cb = (long)1 << lastIndex;
		if ((rules & cb) > 0)
			newFlags |= cb;
		if ((rules & ((long)2 << lastRule)) > 0)
			newFlags = (newFlags & cb) > 0 ? newFlags & ~cb : newFlags | cb;
		return (flags >> (index - 1) & 1) == 1 ? -1 : newFlags | (rules & (((long)1 << lastRule) - 1));
	}
	private static (long, long, long, long, long) Param(long inFlags, int flagBits, int ruleBits) {
		if (inFlags < 0)
			inFlags = (-inFlags & (((long)1 << ruleBits) - 1)) << flagBits;
		var mask = ((long)1 << flagBits) - 1;
		return (
			inFlags,                                        // inFlags
			inFlags & ~mask,                                // newFlags
			(inFlags >> flagBits) & ((1 << ruleBits) - 1),  // rules
			inFlags & mask,                                 // flags
			mask                                            // mask
		);
	}
	private static long NoChildSimple(int index, long inFlags, Fractal f) {
		// Not a very good one...
		var (_, newFlags, rules, flags, _) = Param(inFlags, f.ChildCount - 1, f.ChildCount - 1);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, rules, flags);
	}
	private static long NoChildAngled(int index, long inFlags, Fractal f) {
		var c = f.ChildCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.ChildCount, f.ChildCount + 1);
		return index > 0
			? ApplyNoChildComplexAngle(index, rules, flags, c, c) | (rules << f.ChildCount)
			: (inFlags & ((long)1 << c)) > 0 ? -1 : newFlags;
	}
	private static long NoChildNoAngled(int index, long inFlags, Fractal f) {
		var c = f.ChildCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.ChildCount, f.ChildCount + 1);
		return index > 0
			? ApplyNoChildComplexNoAngle(index, rules, flags, c, c) | (rules << f.ChildCount)
			: (inFlags & ((long)1 << c)) > 0 ? -1 : newFlags;
	}
	private static long NoBackDiag(int index, long inFlags, Fractal f) {
		var c = f.ChildCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, c, f.noBackDiagBits);
		if (index == 0)
			return newFlags;
		int n = 1, ic = index + c - 1;
		for (var i = 1; i <= c - 2; ++i)
			for (var b = i / 2; ++b <= i; n *= 2)
				if ((rules & n) > 0) {
					var m = i - b + 1;
					if ((flags >> (ic - b) % c & flags >> (ic + m) % c & 1) == 1)
						return -1;
					if ((flags >> (ic + b) % c & flags >> (ic - m) % c & 1) == 1)
						return -1;
				}
		--ic;
		for (var i = c / 2; 0 <= --i; n *= 2)
			if ((rules & n) > 0) {
				if ((inFlags >> (index + i) % c & 1) == 1)
					newFlags |= (long)1 << (index + i) % c;
				if ((inFlags >> (ic - i) % c & 1) == 1)
					newFlags |= (long)1 << (ic - i) % c;
			}
		newFlags |= (long)1 << (index - 1);
		return newFlags;
	}
	#endregion

	#region CutFunctions_Seeds
	private static long GetCarpetSymmetricRules(long rules)
			=> (rules & ((long)1 << 0))                                     // 1->1
			+ (rules & ((long)1 << 1)) * (((long)1 << 0) + ((long)1 << 6))  // 2->2+8 (2 and 8 are symmetric)
			+ (rules & ((long)1 << 2)) * (((long)1 << 0) + ((long)1 << 4))  // 3->3+7 (3 and 7 are symmetric)
			+ (rules & ((long)1 << 3)) * (((long)1 << 0) + ((long)1 << 2))  // 4->4+6 (4 and 6 are symmetric)
			+ (rules & ((long)1 << 4))                                      // 5->5
			+ (rules & ((long)1 << 5)) * ((long)1 << 3)                     // 6->9 primary child bit
			+ (rules & ((long)1 << 6)) * ((long)1 << 3);                    // 7->10 second primary child bit
	private static long GetTetraFlakeSymmetricRules(long rules)
		=> (rules & ((long)1 << 0))                                     // 1->1
		+ (rules & ((long)1 << 1)) * (((long)1 << 0) + ((long)1 << 1))  // 2->2+3 (2 and 3 are symmetric)
		+ (rules & ((long)1 << 2)) * (((long)1 << 1) + ((long)1 << 4))  // 3->4+7 (4 and 7 are symmetric)
		+ (rules & ((long)1 << 3)) * (((long)1 << 1) + ((long)1 << 5))  // 4->5+9 (5 and 9 are symmetric)
		+ (rules & ((long)1 << 4)) * (((long)1 << 1) + ((long)1 << 3))  // 5->6+8 (6 and 8 are symmetric)
		+ (rules & ((long)1 << 5)) * ((long)1 << 4)                     // 6->10
		+ (rules & ((long)1 << 6)) * (((long)1 << 4) + ((long)1 << 5))  // 7->11+12 (11 and 12 are symmetric)
		+ (rules & ((long)1 << 7)) * ((long)1 << 5)                     // 8->13
		+ (rules & ((long)1 << 8)) * (((long)1 << 5) + ((long)1 << 6))  // 9->14+15 (14 and 15 are symmetric)
		+ (rules & ((long)1 << 9)) * ((long)1 << 6)                     // 10->16 primary child bit
		+ (rules & ((long)1 << 10)) * ((long)1 << 6);                   // 11->17 second primary child bit

	private static long TriComb(int index, long inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, _, _) = Param(inFlags, f.ChildCount, 6);
		if ((inFlags >> index & 1) == 1)
			return -1;
		if ((rules & 1) > 0 && index is > 3 and < 10) // Med
			newFlags |= (long)1 << 1 | (long)1 << 4 | (long)1 << 9; // Inner
		if ((rules & 2) > 0 && index is > 3 and < 10) // Med
			newFlags |= (long)1 << 7 | (long)1 << 6 | (long)1 << 11; // Outer
		if ((rules & 4) > 0 && index is > 9 and < 13) // Out
			newFlags |= (long)1 << 1 | (long)1 << 4 | (long)1 << 9;
		if ((rules & 8) > 0 && index is > 0 and < 4) // In
			newFlags |= (long)1 << 1 | (long)1 << 4 | (long)1 << 9;
		if ((rules & 16) > 0 && index is > 0 and < 4)
			newFlags |= (long)1 << 0;
		if ((rules & 32) > 0 && index is > 3 and < 10)
			newFlags |= (long)1 << 0;
		return newFlags;
	}
	private static long CarpetSymmetric(int index, long inFlags, Fractal f) {
		var c = f.ChildCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.ChildCount, f.ChildCount + 1 - 3);
		return index > 0 ? ApplyNoChildComplexAngle(index, GetCarpetSymmetricRules(rules), flags, c, c) | (rules << f.ChildCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
	}
	private static long TetraFlakeSymmetric(int index, long inFlags, Fractal f) {
		var c = f.ChildCount - 1;
		(inFlags, var newFlags, var rules, var flags, _) = Param(inFlags, f.ChildCount, f.ChildCount + 1 - 6);
		return index > 0 ? ApplyNoChildComplexNoAngle(index, GetTetraFlakeSymmetricRules(rules), flags, c, c) | (rules << f.ChildCount) : (inFlags & (1 << c)) > 0 ? -1 : newFlags;
	}
	private static long TetraFlakeSymmetricRadHoles(int index, long inFlags, Fractal f) {
		if (index >= 13)
			return -1;
		var c = f.ChildCount - 1;
		var (_, newFlags, rules, flags, _) = Param(inFlags, c, c - 6 - 2);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraFlakeSymmetricRules(rules), flags) | (rules << c); //12
	}
	private static long TetraFlakeSymmetricCornerHoles(int index, long inFlags, Fractal f) {
		if (index is >= 10 and < 13)
			return -1;
		var c = f.ChildCount - 1;
		var (_, newFlags, rules, flags, _) = Param(inFlags, c, c - 6);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraFlakeSymmetricRules(rules), flags) | (rules << c); //9
	}
	private static long TetraFlakeSymmetricTriangleHoles(int index, long inFlags, Fractal f) {
		if (index is > 0 and <= 3)
			return -1;
		var c = f.ChildCount - 1;
		var (_, newFlags, rules, flags, _) = Param(inFlags, c, c - 6);
		return index == 0 ? newFlags : ApplyNoChildSimple(index, GetTetraFlakeSymmetricRules(rules), flags) | (rules << c); //12
	}
	private static long BeamTreeNoBeam(int index, long inFlags, Fractal f) {
		(inFlags, var newFlags, var rules, _, _) = Param(inFlags, f.ChildCount - 1, 2);
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
	private static long BeamTreeOuterJoint(int index, long inFlags, Fractal f) {
		if (inFlags < 0) inFlags = 0; // No params
		long newFlags = 0;
		if (index < 4)
			return newFlags;
		if ((inFlags >> (index - 4) & 1) == 1)
			return -1;
		newFlags |= (long)1 << (12 - index) % 6;
		return newFlags;
	}
	private static long BeamTreeInnerJoint(int index, long inFlags, Fractal f) {
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

}

