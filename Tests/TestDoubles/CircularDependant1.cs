namespace Tests.TestDoubles
{
	public class CircularDependant1
	{
		private CircularDependant2 obj;

		public CircularDependant1(CircularDependant2 obj)
		{
			this.obj = obj;
		}
	}
}