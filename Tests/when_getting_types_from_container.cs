using System;
using System.Collections.Generic;
using PRI.EffectiveIoC;
using Tests.TestDoubles;
using Xunit;

namespace Tests
{
	public class when_getting_types_from_container
	{
		[Fact]
		public void then_resolving_type_from_unloaded_assembly_works()
		{
			var x = IoC.Resolve<IFormattable>();
			Assert.Equal("TestTypes.Class1", x.GetType().FullName);
		}

		[Fact]
		public void then_resolving_instance_of_mapped_interface_correct_instance_results()
		{
			var instance = IoC.Resolve<IInterface>();
			Assert.IsType<InterfaceImplementation>(instance);
		}

		[Fact]
		public void then_resolving_instance_of_unmapped_type_results_with_null()
		{
			var instance = IoC.Resolve<System.Collections.Generic.ISet<int>>();
			Assert.Null(instance);
		}

		[Fact]
		public void then_resolving_concrete_type_with_default_constructor_instance_of_type_results()
		{
			var instance = IoC.Resolve<ArgumentException>();
			Assert.IsType<ArgumentException>(instance);
		}

		[Fact]
		public void then_resolving_concrete_dependency_correct_instance_results()
		{
			var instance = IoC.Resolve<DependantTwo>();
			Assert.NotNull(instance);
			Assert.IsType<DependantTwo>(instance);
		}

		[Fact]
		public void then_resolving_concrete_hierarchial_dependency_correct_instance_results()
		{
			var instance = IoC.Resolve<DependantOne>();
			Assert.NotNull(instance);
			Assert.IsType<DependantOne>(instance);
		}

		[Fact]
		public void then_resolving_generic_concrete_type_correct_instance_type_results()
		{
			var instance = IoC.Resolve<List<int>>();
			Assert.NotNull(instance);
		}

		[Fact]
		public void then_resolving_open_generic_correct_type_results()
		{
			IEnumerable<int> instance = IoC.Resolve<IEnumerable<int>>();
			Assert.NotNull(instance);
			Assert.IsType<List<int>>(instance);
		}

		[Fact]
		public void then_resolving_generic_results_in_correct_type()
		{
			var instance = IoC.Resolve<ICollection<int>>();
			Assert.NotNull(instance);
			Assert.IsType<List<int>>(instance);
		}

		[Fact]
		public void then_resolving_closed_generic_with_open_generic_registration_succeeds()
		{
			IoC.RegisterType(typeof(IList<>), typeof(List<>));
			IoC.Resolve<IList<int>>();
		}

		[Fact]
		public void then_resolving_closed_generic_with_incompatible_closed_generic_registration_fails()
		{
			IoC.RegisterType(typeof(IGeneric<>), typeof(GenericImplementation<string>));
			Assert.Throws<InvalidCastException>(() => IoC.Resolve<IGeneric<int>>());
		}

		[Fact]
		public void then_registering_incompatible_open_generics_fails()
		{
			Assert.Throws<InvalidOperationException>(()=>IoC.RegisterType(typeof(IList<>), typeof(HashSet<>)));
		}

		[Fact]
		public void then_resolving_circular_dependency_causes_exception()
		{
			Assert.Throws<CircularDependencyException>(() => IoC.Resolve<CircularDependant2>());
		}
	}
}
