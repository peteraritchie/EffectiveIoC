namespace Tests.TestDoubles
{
	public class DependantOne
	{
		private DependantTwo dependant;

		public DependantOne(DependantTwo dependant)
		{
			this.dependant = dependant;
		}
	}
}