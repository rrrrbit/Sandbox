namespace RBitUtils
{
	using System;
	using UnityEngine;

	public static class Misc
	{
		public static void CheckChange<T>(this T self, ref T prev, Action callback)
		{
			if (!self.Equals(prev))
			{
				prev = self;
				callback();
			}
		}

		public static Vector2 Distribute(this Vector2 vec, Func<float, float> func) => new Vector2(func(vec.x), func(vec.y));
		public static Vector3 Distribute(this Vector3 vec, Func<float, float> func) => new Vector3(func(vec.x), func(vec.y), func(vec.z));
	}
}