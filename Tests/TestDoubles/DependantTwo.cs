namespace Tests.TestDoubles
{
	public class DependantTwo
	{
		private InterfaceImplementation obj;

		public DependantTwo(InterfaceImplementation obj)
		{
			this.obj = obj;
		}
	}
}