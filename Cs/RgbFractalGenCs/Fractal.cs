using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace RgbFractalGenCs {
	/// <summary>
	/// Fractal Definitions
	/// </summary>
	internal class Fractal {
		internal delegate int CutFunction(int index, int flags);
		// Properties
		internal string name;							// Fractal name (only for selection list)
		internal int childCount;						// ChildCount of Self Similars Inside (must equal the length of all the following arrays)
		internal float childSize;						// Scale Of Self Similars Inside (how much to scale the image when switching parent<->child)
		internal float maxSize;							// The root scale (if too small, the fractal might not fill the whole screen, if too large, it might hurt the performance)
		internal float minSize;							// How tiny the iterations must have to get before rendering dots (if too large, it might crash, it too low it might look gray and have bad performance)
		internal float cutSize;							// A scaling multiplier to test cutting off iterations that are completely outside the view (if you see pieces disappearing too early when zooming in, increase it, if not yo ucan decrease to boost performance)
		internal float[] childX;						// X coord shifts of Self Similars Inside
		internal float[] childY;						// Y coord shifts of Self Similars Inside
		internal (string, float[])[] childAngle;		// Angle shifts of Self Similars Inside
		internal (string, byte[])[] childColor;			// Color shifts of Self Similars Inside
		internal (string, CutFunction)[] cutFunction;	// Function that takes a bitarray transforms it and decides to cut some specific patterns of Self Simmilars
		// Autoproperties
		internal float periodAngle;
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
			(string, float[])[] childAngle,
			(string, byte[])[] childColor,
			(string, CutFunction)[] cutFunction
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
			(string, float[])[] childAngle,
			(string, byte[])[] childColor,
			(string, CutFunction)[] cutFunction
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
			(string, float[])[] childAngle,
			(string, byte[])[] childColor,
			(string, CutFunction)[] cutFunction
		) {
			return new(this, name, childCount, childAngle, childColor, cutFunction);
		}
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
			(string, float[])[] childAngle,
			(string, byte[])[] childColor,
			(string, CutFunction)[] cutFunction
		) {
			return new(this, name, childCount, childAngle, childColor, cutFunction);
		}
		/// <summary>
		/// Copy function
		/// </summary>
		/// <param name="name">New copy name</param>
		/// <param name="cutFunction">If not nullptr, replace cutFunction with this one</param>
		/// <returns>Copy fractal</returns>
		public Fractal Copy(
			string name,
			(string, CutFunction)[] cutFunction
		) {
			return new(this, name, childCount, null, null, cutFunction);
		}


		#region CutFunctions_Param
		public static (int, int, int, int, int) Param(int inFlags, int flagbits, int rulebits) {
			if (inFlags < 0)
				inFlags = ((-inFlags) & ((1 << rulebits) - 1)) << flagbits;
			int mask = ((1 << flagbits) - 1);
			return (
				inFlags,
				inFlags & ~mask,
				(inFlags >> flagbits) & ((1 << rulebits) - 1),
				inFlags & mask,
				mask
			);
		}
		public static int FlakeTemplate(int index, int inFlags, int bits) {
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, bits+1, bits+2);
			if (index == 0 && (inFlags & (1 << bits)) > 0)
				return -1;
			if (index == 0)
				return newFlags;
			else {
				int cb = 1 << bits;
				if ((rules & cb) > 0)
					newFlags |= cb;
				if ((rules & (2 << bits)) > 0) {
					if((inFlags & cb) > 0)
						newFlags &= ~cb;
					else
						newFlags |= cb;
				}
			} 
			if ((flags >> (index - 1) & 1) == 1)
				return -1;
			int m = ((1 << bits) - 1);
			int r = rules & m;
			newFlags |= ((r >> (bits - index + 1)) | (r << (index - 1))) & m;
			return newFlags;
		}
		/*public static int FlakeTemplate_WithCenters(int index, int inFlags, int bits) {
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, bits, bits);
			if (index == 0)
				return newFlags;
			if ((flags >> (index - 1) & 1) == 1)
				return -1;
			newFlags |= ((rules >> (bits - index + 1)) | (rules << (index - 1))) & mask;
			return newFlags;
		}*/
		public static int Trees_NoChildParam(int index, int inFlags) {
			// Not a very good one...
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, 9, 9);
			if (index == 0)
				return newFlags;
			if ((flags >> (index - 1) & 1) == 1)
				return -1;
			newFlags |= rules;
			return newFlags;
		}
		public static int TriComb_Param(int index, int inFlags) {
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, 13, 6);
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
		#endregion

		#region CutFunctions_Triflake
		public static int Triflake_NoCornerParam(int index, int inFlags) {
			return FlakeTemplate(index, inFlags, 3);
		}
		public static int Triflake_NoBackDiag(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index == 0)
				return newFlags;
			if ((inFlags >> (index) % 3 & inFlags >> (index + 1) % 3 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 0) % 3 & 1) == 1)
				newFlags |= 1 << (index + 0) % 3;
			if ((inFlags >> (index + 1) % 3 & 1) == 1)
				newFlags |= 1 << (index + 1) % 3;
			newFlags |= 1 << (index - 1);
			return newFlags;
		}
		#endregion

		#region CutFunctions_TetraTriflake
		private static int Tetraflake_CustomCornerParam(int index, int inFlags, int fb, int fi) {
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, fb, fb);
			if (index == 0)
				return newFlags;
			if ((flags >> (index - fi) & 1) == 1)
				return -1;
			newFlags |= rules;
			return newFlags;
		}
		public static int Tetraflake_NoCornerParam(int index, int inFlags) {
			return Tetraflake_CustomCornerParam(index, inFlags, 15, 1);
		}
		public static int Tetraflake_NoCornerRadHolesParam(int index, int inFlags) {
			if (index >= 13)
				return -1;
			return Tetraflake_CustomCornerParam(index, inFlags, 12, 1);
		}
		public static int Tetraflake_NoCornerCornerHolesParam(int index, int inFlags) {
			if (index >= 10 && index < 13)
				return -1;
			return Tetraflake_CustomCornerParam(index, inFlags, 9, 1);
		}
		public static int Tetraflake_NoCornerTriangleHolesParam(int index, int inFlags) {
			if (index > 0 && index <= 3)
				return -1;
			return Tetraflake_CustomCornerParam(index, inFlags, 12, 4);
		}
		#endregion

		#region CutFunctions_Carpet
		public static int Carpet_NoCornerParam(int index, int inFlags) {
			return FlakeTemplate(index, inFlags, 8);
		}
		public static int Carpet_NoBackDiag(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index == 0)
				return newFlags;
			if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
				return -1;

			if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
				return -1;

			if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 6) % 8 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 0) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
				return -1;

			if ((inFlags >> (index + 2) % 8 & 1) == 1)
				newFlags |= 1 << (index + 2) % 8;
			if ((inFlags >> (index + 3) % 8 & 1) == 1)
				newFlags |= 1 << (index + 3) % 8;
			if ((inFlags >> (index + 4) % 8 & 1) == 1)
				newFlags |= 1 << (index + 4) % 8;
			newFlags |= 1 << (index - 1);
			return newFlags;
		}
		public static int Carpet_NoBackDiag2(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index == 0)
				return newFlags;
			if ((inFlags >> (index + 1) % 8 & inFlags >> (index + 4) % 8 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 2) % 8 & inFlags >> (index + 5) % 8 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 2) % 8 & 1) == 1)
				newFlags |= 1 << (index + 2) % 8;
			if ((inFlags >> (index + 4) % 8 & 1) == 1)
				newFlags |= 1 << (index + 4) % 8;
			newFlags |= 1 << (index - 1);
			return newFlags;
		}
		#endregion

		#region CutFunctions_Pentaflake
		public static int Pentaflake_NoCornerParam(int index, int inFlags) {
			return FlakeTemplate(index, inFlags, 5);
		}
		public static int Pentaflake_NoBackDiag(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index == 0)
				return newFlags;
			if ((inFlags >> (index + 2) % 5 & inFlags >> index % 5 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 3) % 5 & inFlags >> (index + 1) % 5 & 1) == 1)
				return -1;
			if ((inFlags >> (index + 1) % 5 & 1) == 1)
				newFlags |= 1 << (index + 1) % 5;
			if ((inFlags >> (index + 2) % 5 & 1) == 1)
				newFlags |= 1 << (index + 2) % 5;
			newFlags |= 1 << (index - 1);
			return newFlags;
		}
		#endregion

		#region CutFunctions_Hexaflake
		public static int Hexaflake_NoCornerParam(int index, int inFlags) {
			return FlakeTemplate(index, inFlags, 6);
		}
		public static int Hexaflake_NoBackDiag(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index == 0)
				return newFlags;
			if ((inFlags >> (index + 1) % 6 & inFlags >> (index + 3) % 6 & 1) == 1)
				return -1; // diagonals (LL+RR)
			if ((inFlags >> (index + 0) % 6 & inFlags >> (index + 3) % 6 & 1) == 1)
				return -1; // backwards (L+RR)
			if ((inFlags >> (index + 1) % 6 & inFlags >> (index + 4) % 6 & 1) == 1)
				return -1; // backwards (LL+R)
			if ((inFlags >> (index + 1) % 6 & 1) == 1)
				newFlags |= 1 << (index + 1) % 6; // backdiagonal
			if ((inFlags >> (index + 2) % 6 & 1) == 1)
				newFlags |= 1 << (index + 2) % 6; // backdiagonal
			if ((inFlags >> (index + 3) % 6 & 1) == 1)
				newFlags |= 1 << (index + 3) % 6; // backback

			newFlags |= 1 << (index - 1);
			return newFlags;
		}
		#endregion

		#region CutFunctions_BeamTree
		public static int Beamtree_OuterJoint(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index < 4)
				return newFlags;
			if ((inFlags >> (index - 4) & 1) == 1)
				return -1;
			newFlags |= 1 << (12 - index) % 6;
			return newFlags;
		}
		public static int Beamtree_InnerJoint(int index, int inFlags) {
			if (inFlags < 0) inFlags = 0; // No params
			int newFlags = 0;
			if (index < 4)
				return newFlags;
			if ((inFlags >> (index - 4) & 1) == 1)
				return -1;
			newFlags |= 1 << (index + 5) % 6;
			return newFlags;
		}
		public static int Beamtree_NoBeam(int index, int inFlags) {
			(inFlags, int newFlags, int rules, int flags, int mask) = Param(inFlags, 9, 2);
			//if (inFlags < 0) inFlags = 0; // No params
			//int newFlags = 0;
			if (index < 4)
				return newFlags;
			if ((inFlags >> (index - 4) & 1) == 1)
				return -1;
			if((rules & 1) > 0)
				newFlags |= 1 << (3 * index + 2) % 6;// 002244 InnerBeam
			if ((rules & 2) > 0)
				newFlags |= 1 << (3 * index + 5) % 6;// 002244 OuterBeam
			return newFlags;
		}
		#endregion

		#region DeprecatedCutFunctions
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
}
