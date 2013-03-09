namespace Tests.TestDoubles
{
	public class CircularDependant2
	{
		private CircularDependant1 obj;

		public CircularDependant2(CircularDependant1 obj)
		{
			this.obj = obj;
		}
	}
}