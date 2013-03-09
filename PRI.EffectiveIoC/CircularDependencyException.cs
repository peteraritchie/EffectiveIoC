using System;

namespace PRI.EffectiveIoC
{
	public class CircularDependencyException : Exception
	{
		public CircularDependencyException(string message) : base(message)
		{
		}
	}
}