using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace PRI.EffectiveIoC
{
	/// <summary>
	/// Simple but effective Inversion of Control container
	/// </summary>
	/// <remarks>
	/// Mappings can be performed in app.config via <see cref="System.Configuration.NameValueSectionHandler"/>.  For example
	/// <c>
	/// {configSections}
	///     {section name="types" type="System.Configuration.NameValueSectionHandler"/}
	///   {/configSections}
	///   {types}
	///     {add
	///       key="Tests.TestDoubles.IInterface, Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
	///       value="Tests.TestDoubles.InterfaceImplementation, Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"/}
	///     {add
	///       key="System.Collections.Generic.ICollection`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
	///       value="System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"/}
	///     {add
	///       key="System.Collections.Generic.IEnumerable`1, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
	///       value="System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"/}
	///     {add
	///       key="System.IFormattable, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
	///       value="TestTypes.Class1, TestTypes, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"/}
	///   {/types}
	/// </c>
	/// </remarks>
	/// <example>
	/// Given the above app.config, we could resolve a IEnumerable{T} instance as follows:
	/// <code>
	/// IEnumerable{int} instance = IoC.Resolve{IEnumerable{int}}();
	/// </code>
	/// Alternatively we could manually register a mapping with the <see cref="IoC.RegisterType{TFrom, TTo}"/> method.  For example:
	/// <code>
	/// IoC.RegisterType(typeof(IList{}), typeof(List{}));
	/// </code>
	/// Which could then be resolved as follows:
	/// <code>
	/// IoC.Resolve{IList{int}}();
	/// </code>
	/// </example>
	public static class IoC
	{
		private static bool initialized;
		private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

		private static Type FindType(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (initialized == false) LoadMappingsFromConfig();

			if (TypeMappings.ContainsKey(type))
			{
				return TypeMappings[type];
			}
			if (type.IsGenericType && !type.ContainsGenericParameters && TypeMappings.ContainsKey(type.GetGenericTypeDefinition()))
			{
				var realType = TypeMappings[type.GetGenericTypeDefinition()];
				if (realType.ContainsGenericParameters && !type.ContainsGenericParameters)
				{
#if NET_4_5
					return realType.MakeGenericType(type.GenericTypeArguments);
#elif NET_4_0
					return realType.MakeGenericType(type.GetGenericArguments());
#endif
				}
				return realType;
			}
			return type;
		}

		private static void LoadMappingsFromConfig()
		{
			var collection = ConfigurationManager.GetSection("types") as NameValueCollection;
			if (collection == null) throw new InvalidOperationException("types config section was not of expected type.");
			foreach (string fromTypeText in collection)
			{
				if (String.IsNullOrWhiteSpace(fromTypeText)) continue;
				var fromType = Type.GetType(fromTypeText) ??
							   Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(assType => assType.Name == fromTypeText);
				if (fromType == null) continue;
				var toTypeText = collection[fromTypeText];
				var toType = Type.GetType(toTypeText) ??
							   Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(assType => assType.Name == toTypeText);
				if (toType == null) continue;
				TypeMappings.Add(fromType, toType);
			}
			initialized = true;
		}

		/// <summary>
		/// Create new instance of <typeparam name="T">type</typeparam>
		/// </summary>
		/// <remarks>
		/// If no type is registered for <typeparam name="T">type</typeparam> and T is not an interface
		/// not an abstract class, an instance of <typeparam name="T"></typeparam> will be created.
		/// The constructor with the least number of parameters will be chosen.
		/// Each type of parameter will be resolved via <see cref="IoC.Resolve"/>
		/// </remarks>
		/// <seealso cref="IoC.Resolve"/>
		/// <typeparam name="T"></typeparam>
		/// <returns>Instance of <typeparam name="T"/> or null if type is abstract or an interface and no mapping exists.</returns>
		/// <exception cref="CircularDependencyException">If a circular dependency exists attempting to instiate the mapped type.</exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType(typeof(IList{}), typeof(List{}));
		///	var listOfInt = IoC.Resolve{IList{int}}();
		/// </code>
		/// </example>
		public static T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		[ThreadStatic]
		private static readonly Stack<Type> ResolveStack;

		static IoC()
		{
			ResolveStack = new Stack<Type>();
		}

		/// <summary>
		/// Create new instance of <paramref name="type">type</paramref>
		/// </summary>
		/// <remarks>
		/// If no type is registered for <paramref name="type"/> and <paramref name="type"/> is not an interface
		/// not an abstract class, an instance of <paramref name="type"/> will be created.
		/// The constructor with the least number of parameters will be chosen.
		/// Each type of parameter will be resolved via <see cref="IoC.Resolve"/>
		/// </remarks>
		/// <seealso cref="IoC.Resolve"/>
		/// <param name="type">Type to resolve.</param>
		/// <returns>Instance of <paramref name="type"/> or null if type is abstract or an interface and no mapping exists.</returns>
		/// <exception cref="CircularDependencyException">If a circulat dependency exists attempting to instiate the mapped type.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="type"/> is null.</exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType{IList{}, List{}}();
		///	var listOfInt = IoC.Resolve(typeof(IList{int}));
		/// </code>
		/// </example>
		public static object Resolve(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			if (ResolveStack.Contains(type)) throw new CircularDependencyException("Circular dependency during resolve.");
			ResolveStack.Push(type);

			try
			{
				var t = FindType(type);
				if (t.IsInterface || t.IsAbstract) return null;

				// go through each constructor looking for one where
				// we know how to instantiate all the types
				foreach (var constructor in t.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
											 .OrderBy(e => e.GetParameters().Length)
											 .Select(e => e))
				{
					var paramTypes = constructor.GetParameters().Select(e => FindType(e.ParameterType)).ToArray();
					if (paramTypes.Any(e => e == null)) continue;
					var objects = paramTypes.Select(Resolve).ToArray();
					return Activator.CreateInstance(t, objects);
				}
				return null;
			}
			finally
			{
				ResolveStack.Pop();
			}
		}

		/// <summary>
		/// Registers type <param name="to"/> for requested instances of <paramref name="@from"/>
		/// </summary>
		/// <param name="from">Type to be resolved.</param>
		/// <param name="to">Type that will be instantiated (or a mappint thereof) when types of <paramref name="@from"/> are requested </param>
		/// <exception cref="ArgumentNullException">If <paramref name="@to"/> or <paramref name="from"/> is null</exception>
		/// <exception cref="InvalidOperationException">If type <paramref name="@to"/> is not assignable to a variable of type <paramref name="from"/></exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType(typeof(IList{}), typeof(List{}));
		///	var listOfInt = IoC.Resolve{IList{int}}();
		/// </code>
		/// </example>
		public static void RegisterType(Type @from, Type to)
		{
			if(!@from.IsAssignableFrom(to)) throw new InvalidOperationException(string.Format("type {0} is not assignable to {1}", @to.Name, from.Name));
			if (@from == null) throw new ArgumentNullException("from");
			if (to == null) throw new ArgumentNullException("to");
			if (!TypeMappings.ContainsKey(@from)) TypeMappings.Add(@from, to);
		}

		/// <summary>
		/// Registers type <typeparamref name="TTo"/> for requested instancee of <typeparamref name="TFrom"/>
		/// </summary>
		/// <typeparam name="TFrom">Type to be resolved</typeparam>
		/// <typeparam name="TTo">Type that will be instantiated (or a mapping thereof) when types of <typeparamref name="TFrom"/> are requested </typeparam>
		/// <exception cref="InvalidOperationException">If type <typeparamref name="TTo"/> is not assignable to a variable of type <typeparamref name="TFrom"/></exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType{IList{}, List{}}();
		///	var listOfInt = IoC.Resolve(typeof(IList{int}));
		/// </code>
		/// </example>
		public static void RegisterType<TFrom, TTo>()
		{
			RegisterType(typeof (TFrom), typeof (TTo));
		}
	}
}