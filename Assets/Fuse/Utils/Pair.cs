using System;

namespace Fuse.Core
{
	[Serializable]
	public class Pair<T1, T2>
	{
		public readonly T1 A;
		public readonly T2 B;

		public Pair(T1 a, T2 b)
		{
			A = a;
			B = b;
		}
	}

	[Serializable]
	public class Pair<T1, T2, T3>
	{
		public readonly T1 A;
		public readonly T2 B;
		public readonly T3 C;

		public Pair(T1 a, T2 b, T3 c)
		{
			A = a;
			B = b;
			C = c;
		}
	}

	[Serializable]
	public class Pair<T1, T2, T3, T4>
	{
		public readonly T1 A;
		public readonly T2 B;
		public readonly T3 C;
		public readonly T4 D;

		public Pair(T1 a, T2 b, T3 c, T4 d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
		}
	}
}