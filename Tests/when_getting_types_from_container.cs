using System;
using System.Collections.Generic;
using PRI.EffectiveIoC;
using Tests.TestDoubles;
using Xunit;

namespace Tests
{
// ReSharper disable InconsistentNaming
	public class when_getting_types_from_container
	{
		private IoC ioc;
		public when_getting_types_from_container()
		{
			ioc = new IoC();
		}

		[Fact]
		public void then_resolving_type_from_unloaded_assembly_works()
		{
			var x = ioc.Resolve<IFormattable>();
			Assert.Equal("TestTypes.Class1", x.GetType().FullName);
		}

		[Fact]
		public void then_resolving_instance_of_mapped_interface_correct_instance_results()
		{
			var instance = ioc.Resolve<IInterface>();
			Assert.IsType<InterfaceImplementation>(instance);
		}

		[Fact]
		public void then_resolving_instance_of_unmapped_type_results_with_null()
		{
			var instance = ioc.Resolve<ISet<int>>();
			Assert.Null(instance);
		}

		[Fact]
		public void then_resolving_concrete_type_with_default_constructor_instance_of_type_results()
		{
			var instance = ioc.Resolve<ArgumentException>();
			Assert.IsType<ArgumentException>(instance);
		}

		[Fact]
		public void then_resolving_concrete_dependency_correct_instance_results()
		{
			var instance = ioc.Resolve<DependantTwo>();
			Assert.NotNull(instance);
			Assert.IsType<DependantTwo>(instance);
		}

		[Fact]
		public void then_resolving_concrete_hierarchial_dependency_correct_instance_results()
		{
			var instance = ioc.Resolve<DependantOne>();
			Assert.NotNull(instance);
			Assert.IsType<DependantOne>(instance);
		}

		[Fact]
		public void then_resolving_generic_concrete_type_correct_instance_type_results()
		{
			var instance = ioc.Resolve<List<int>>();
			Assert.NotNull(instance);
		}

		[Fact]
		public void then_resolving_open_generic_correct_type_results()
		{
			IEnumerable<int> instance = ioc.Resolve<IEnumerable<int>>();
			Assert.NotNull(instance);
			Assert.IsType<List<int>>(instance);
		}

		[Fact]
		public void then_resolving_generic_results_in_correct_type()
		{
			var instance = ioc.Resolve<ICollection<int>>();
			Assert.NotNull(instance);
			Assert.IsType<List<int>>(instance);
		}

		[Fact]
		public void then_resolving_closed_generic_with_open_generic_registration_succeeds()
		{
			ioc.RegisterType(typeof(IList<>), typeof(List<>));
			ioc.Resolve<IList<int>>();
		}

		[Fact]
		public void then_resolving_closed_generic_with_incompatible_closed_generic_registration_fails()
		{
			ioc.RegisterType(typeof(IGeneric<>), typeof(GenericImplementation<string>));
			Assert.Throws<InvalidCastException>(() => ioc.Resolve<IGeneric<int>>());
		}

		[Fact]
		public void then_registering_incompatible_open_generics_fails()
		{
			Assert.Throws<InvalidOperationException>(()=>ioc.RegisterType(typeof(IList<>), typeof(HashSet<>)));
		}

		[Fact]
		public void then_resolving_circular_dependency_causes_exception()
		{
			Assert.Throws<CircularDependencyException>(() => ioc.Resolve<CircularDependant2>());
		}

		[Fact]
		public void then_resolving_delegate_mapping_results_in_success()
		{
			ioc.RegisterTypeFunc(typeof(DelegateBase), type => new DelegateSuper());
			Assert.IsType<DelegateSuper>(ioc.Resolve<DelegateBase>());
		}

		[Fact]
		private void then_resolving_named_instance_succeeds()
		{
			var expected = new[] {1, 2, 3, 4};
			ioc.RegisterInstance(expected, "myArray");
			Assert.Equal(expected, ioc.Resolve<int[]>("myArray"));
		}

		[Fact]
		private void then_resolving_named_instance_in_config_succeeds()
		{
			var expected = new List<int>();
			Assert.Equal(expected, ioc.Resolve<List<int>>("myIntList"));
		}

		[Fact]
		private void then_resolving_unnamed_type_results_in_null()
		{
			var actual = ioc.Resolve<IList<int>>("gibberish");
			Assert.Null(actual);
		}

		[Fact]
		private void then_resolving_named_type_succeeds()
		{
			ioc.RegisterType<IList<int>, List<int>>("myListType");
			var actual = ioc.Resolve<IList<int>>("myListType");
			Assert.IsType<List<int>>(actual);
		}
	}
	// ReSharper restore InconsistentNaming
}
