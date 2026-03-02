namespace RBitUtils
{
	namespace ResponseTypes
	{
		using System;
		using Unity.VisualScripting.FullSerializer;
		using UnityEngine;
		///<summary>
		///Physics based response supporting easing, overshoot and spring motion taken from <a href="https://www.youtube.com/watch?v=KPoeNZZ6H4s">t3ssel8r's video on procedural animation.</a>
		///</summary>
		public static class SecondOrderDynamicsOld
		{
			public static void InitConstants(
				float f, float z, float r,
				out float k1, out float k2,
				out float k3)
			{
				k1 = z / (Mathf.PI * f);
				k2 = 1 / (4 * (Mathf.PI * f) * (Mathf.PI * f));
				k3 = r * z / (2 * Mathf.PI * f);
			}

			public static float UpdateSingle(
				float dt, float x, float? xd,
				ref float xp, ref float y, ref float yd,
				float k1, float k2, float k3)
			{
				if (xd == null) // estimate velocity if absent
				{
					xd = (x - xp) / dt;
					xp = x;
				}

				float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
				y += dt * yd; // integrate by vel
				yd += dt * (x + k3 * (float)xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration

				return y;
			}
			public static Vector2 UpdateVector2(
				float dt, Vector2 x, Vector2? xd,
				ref Vector2 xp, ref Vector2 y, ref Vector2 yd,
				float k1, float k2, float k3)
			{
				if (xd == null)
				{
					xd = (x - xp) / dt;
					xp = x;
				}

				float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1);
				y += dt * yd;
				yd += dt * (x + k3 * (Vector2)xd - y - k1 * yd) / k2_stable;

				return y;
			}
			public static Vector3 UpdateVector3(
				float dt, Vector3 x, Vector3? xd,
				ref Vector3 xp, ref Vector3 y, ref Vector3 yd,
				float k1, float k2, float k3)
			{
				if (xd == null)
				{
					xd = (x - xp) / dt;
					xp = x;
				}

				float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1);
				y += dt * yd;
				yd += dt * (x + k3 * (Vector3)xd - y - k1 * yd) / k2_stable;

				return y;
			}

			public class F
			{
				float xp, y, yd;
				float k1, k2, k3;

				public F(float x0, float f, float z, float r)
				{
					InitConstants(
						f, z, r,
						out k1, out k2, out k3);
					xp = y = x0;
					yd = 0;
				}

				public float Update(float dt, float x, float? xd = null) => UpdateSingle(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
			}

			public class V2
			{
				Vector2 xp, y, yd;
				float k1, k2, k3;

				public V2(Vector2 x0, float f, float z, float r)
				{
					InitConstants(
						f, z, r,
						out k1, out k2, out k3);
					xp = y = x0;
					yd = Vector2.zero;
				}

				public Vector2 Update(float dt, Vector2 x, Vector2? xd = null) => UpdateVector2(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
			}

			public class V3
			{
				Vector3 xp, y, yd;
				float k1, k2, k3;

				public V3(Vector3 x0, float f, float z, float r)
				{
					InitConstants(
						f, z, r,
						out k1, out k2, out k3);
					xp = y = x0;
					yd = Vector3.zero;
				}

				public Vector3 Update(float dt, Vector3 x, Vector3? xd = null) => UpdateVector3(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
			}
		}
		/// <summary>
		/// Update an output value y based on an input value x with respect to timestep.
		/// </summary>
		public abstract class ResponseType
		{
			public float x, y;
			public abstract void Update(float dt);
			public virtual float Update(float dt, float x)
			{
				this.x = x;
				Update(dt);
				return y;
			}
		}
		/// <summary>
		/// Update an output value y based on an input value x and its velocity, with respect to timestep.
		/// </summary>
		public abstract class VelResponseType : ResponseType
		{
			public float xp, xd;
			public override float Update(float dt, float x)
			{
				xd = (x - xp) / dt; // estimate vel
				xp = x;
				base.Update(dt, x);
				return y;
			}
			public virtual float Update(float dt, float x, float xd)
			{
				this.xd = xd;
				base.Update(dt, x);
				return y;
			}
		}
		public class Vec2Response<T> where T : ResponseType
		{
			T rx, ry;

			public Vec2Response(
				Func<float, T> factory,
				Vector2 x0)
			{
				rx = factory(x0.x);
				ry = factory(x0.y);
			}

			public Vector2 Update(float dt, Vector2 x)
				=> new(
					rx.Update(dt, x.x),
					ry.Update(dt, x.y));
		}
		public class Vec3Response<T> where T : ResponseType
		{
			T rx, ry, rz;

			public Vec3Response(
				Func<float, T> factory,
				Vector3 x0)
			{
				rx = factory(x0.x);
				ry = factory(x0.y);
				rz = factory(x0.z);
			}

			public Vector3 Update(float dt, Vector3 x)
				=> new(
					rx.Update(dt, x.x),
					ry.Update(dt, x.y),
					rz.Update(dt, x.z));
		}
		///<summary>
		///Physics based response supporting easing, overshoot and spring motion taken from <a href="https://www.youtube.com/watch?v=KPoeNZZ6H4s">t3ssel8r's video on procedural animation.</a>
		///</summary>
		public class Spring : VelResponseType
		{
			public float yd;
			private float k1, k2, k3;

			public Spring(float x0, float f, float z, float r)
			{
				var pif = Mathf.PI * f;
				k1 = z / pif;
				k2 = 1 / (4 * pif * pif);
				k3 = r * k1 / 2;

				xp = y = x0;
				yd = 0;
			}
			public override void Update(float dt)
			{
				float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
				y += dt * yd; // integrate by vel
				yd += dt * (x + k3 * xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration
			}
		}
		///<summary>
		///Response type that updates at a fixed or velocity-dependent rate.
		///</summary>
		public class AdaptiveRate : VelResponseType
		{
			private float timer;
			public float rate;
			public Func<float, float> rateMultOverVel;	

			public AdaptiveRate(float x0, float rate, Func<float, float> rateMultOverVel)
			{
				x = y = x0;
				this.rate = rate;
				this.rateMultOverVel = rateMultOverVel;
			}
			public override void Update(float dt)
			{
				timer += dt;
				if (timer >= (1 / (rate * rateMultOverVel(xd))))
				{
					timer = 0;
					y = x;
				}
			}
		}
		///<summary>
		///Response type based on Lerp.
		///</summary>
		public class LerpD : ResponseType
		{
			float f;
			
			public LerpD(float x0, float k, float t)
			{
				x = y = x0;
				f = Mathf.Pow(1 - k, 1 / t);
			}

			public override void Update(float dt)
			{
				y = Mathf.Lerp(
					y, x,
					1 - Mathf.Pow(f, dt));
			}
		}

		public class AdaptiveStepV3
		{
			private float timer;
			public float rate;
			public Func<float, float> rateMultOverVel;

			public Vector3 xv, yv, xdv; // actual vec state

			public AdaptiveStepV3(Vector3 x0, float rate,
				Func<float, float> rateMultOverVel)
			{
				xv = yv = x0;
				this.rate = rate;
				this.rateMultOverVel = rateMultOverVel;
			}

			public Vector3 Update(float dt, Vector3 x)
			{
				var vel = (x - xv).magnitude; // scalar vel
				xv = x;
				timer += dt;
				if (timer >= 1f / (rate * rateMultOverVel(vel)))
				{
					timer = 0;
					yv = x; // snap ALL components together
				}

				Debug.Log((rate * rateMultOverVel(vel)));

				return yv;
			}
		}
	}
}