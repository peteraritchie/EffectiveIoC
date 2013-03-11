using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using PRI.ProductivityExtensions.ReflectionExtensions;

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
	/// Alternatively we could manually register a mapping with the <see cref="IoC.RegisterType{TFrom, TTo}()"/> method.  For example:
	/// <code>
	/// IoC.RegisterType(typeof(IList{}), typeof(List{}));
	/// </code>
	/// Which could then be resolved as follows:
	/// <code>
	/// IoC.Resolve{IList{int}}();
	/// </code>
	/// </example>
	public class IoC
	{
		private bool initialized;
		private readonly Dictionary<Type, Type> typeMappings = new Dictionary<Type, Type>();

		private Type FindType(Type @from)
		{
			if (@from == null) throw new ArgumentNullException("from");
			if (initialized == false) LoadMappingsFromConfig();

			if (typeMappings.ContainsKey(@from))
			{
				return typeMappings[@from];
			}
			if (@from.IsGenericType && !@from.ContainsGenericParameters &&
			    typeMappings.ContainsKey(@from.GetGenericTypeDefinition()))
			{
				var realType = typeMappings[@from.GetGenericTypeDefinition()];
				if (realType.ContainsGenericParameters && !@from.ContainsGenericParameters)
				{
					return realType.MakeGenericType(@from.GetGenericTypeArguments());
				}
				return realType;
			}
			return @from;
		}

		private void LoadMappingsFromConfig()
		{
			var section = ConfigurationManager.GetSection("types");

			if (section != null)
			{
				var typesNameValueCollection = section as NameValueCollection;
				if (typesNameValueCollection == null)
					throw new InvalidOperationException("types config section was of expected type.");

				foreach (var t in from e in typesNameValueCollection.Cast<string>() where !String.IsNullOrWhiteSpace(e)
				                  let fromType = Type.GetType(e) where fromType != null
				                  let toType = Type.GetType(typesNameValueCollection[e]) where toType != null
				                  select new {fromType, toType}) typeMappings.Add(t.fromType, t.toType);
			}
			section = ConfigurationManager.GetSection("instances");
			if (section != null)
			{
				var instancesNameValueCollection = section as NameValueCollection;
				if (instancesNameValueCollection == null)
					throw new InvalidOperationException("instances config section was not of expected type.");

				foreach (var t in from name in instancesNameValueCollection.Cast<string>()
				                  where !string.IsNullOrWhiteSpace(name)
				                  let type = Type.GetType(instancesNameValueCollection[name])
				                  where type != null
				                  select new {name, type}) namedInstances.Add(t.name, CreateInstance(t.type, t.type));
			}
			initialized = true;
		}

		private readonly Stack<Type> resolveStack;

		public IoC()
		{
			resolveStack = new Stack<Type>();
		}

		/// <summary>
		/// Create new instance of <paramref name="from">type</paramref>
		/// </summary>
		/// <remarks>
		/// If no type is registered for <paramref name="from"/> and <paramref name="from"/> is not an interface
		/// not an abstract class, an instance of <paramref name="from"/> will be created.
		/// The constructor with the least number of parameters will be chosen.
		/// Each type of parameter will be resolved via <see cref="IoC.Resolve(Type)"/>
		/// </remarks>
		/// <seealso cref="IoC.Resolve&lt;T&gt;()"/>
		/// <param name="from">Type to resolve.</param>
		/// <returns>Instance of <paramref name="from"/> or null if type is abstract or an interface and no mapping exists.</returns>
		/// <exception cref="CircularDependencyException">If a circulat dependency exists attempting to instiate the mapped type.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="from"/> is null.</exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType{IList{}, List{}}();
		///	var listOfInt = IoC.Resolve(typeof(IList{int}));
		/// </code>
		/// </example>
		public object Resolve(Type @from)
		{
			if (@from == null) throw new ArgumentNullException("from");
			if (funcs.ContainsKey(@from)) return funcs[@from](@from);
			KeyValuePair<Type, Func<Type, object>> x = funcs.FirstOrDefault(kvp => @from.IsGenericType && @from.GetGenericTypeDefinition() == kvp.Key);
			if (!x.Equals(default(KeyValuePair<Type, Func<Type, object>>))) return funcs[@from.GetGenericTypeDefinition()](@from);
			if (resolveStack.Contains(@from)) throw new CircularDependencyException("Circular dependency during resolve.");
			resolveStack.Push(@from);

			try
			{
				var t = FindType(@from);
				if (t.IsInterface || t.IsAbstract) return null;

				return CreateInstance(@from, t);
			}
			finally
			{
				resolveStack.Pop();
			}
		}

		private readonly Dictionary<Type, Func<Type, object>> funcs = new Dictionary<Type, Func<Type, object>>();

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
		public void RegisterType(Type @from, Type to)
		{
			if (@from == null) throw new ArgumentNullException("from");
			if (to == null) throw new ArgumentNullException("to");
			if ((@from.IsOpenGenericType() || !@from.IsAssignableFrom(to)) &&
			    !to.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == @from))
			{
				throw new InvalidOperationException(string.Format("type {0} is not assignable to {1}", @to.Name, @from.Name));
			}
			if (!typeMappings.ContainsKey(@from)) typeMappings.Add(@from, to);
		}

		/// <summary>
		/// Create new instance of <typeparam name="TFrom">type</typeparam>
		/// </summary>
		/// <remarks>
		/// If no type is registered for <typeparam name="TFrom">type</typeparam> and T is not an interface
		/// not an abstract class, an instance of <typeparam name="TFrom"></typeparam> will be created.
		/// The constructor with the least number of parameters will be chosen.
		/// Each type of parameter will be resolved via <see cref="IoC.Resolve(Type)"/>
		/// </remarks>
		/// <seealso cref="IoC.Resolve(Type)"/>
		/// <typeparam name="TFrom"></typeparam>
		/// <returns>Instance of <typeparam name="TFrom"/> or null if type is abstract or an interface and no mapping exists.</returns>
		/// <exception cref="CircularDependencyException">If a circular dependency exists attempting to instiate the mapped type.</exception>
		/// <example>
		/// <code>
		///	IoC.RegisterType(typeof(IList{}), typeof(List{}));
		///	var listOfInt = IoC.Resolve{IList{int}}();
		/// </code>
		/// </example>
		public TFrom Resolve<TFrom>()
		{
			return (TFrom) Resolve(typeof (TFrom));
		}

		private readonly Dictionary<Tuple<string, Type>, Type> namedTypes = new Dictionary<Tuple<string, Type>, Type>();

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
		public void RegisterType<TFrom, TTo>()
		{
			RegisterType(typeof (TFrom), typeof (TTo));
		}

		public T Resolve<T>(string name)
		{
			return (T) Resolve(typeof (T), name);
		}

		public object Resolve(Type @from, string name)
		{
			if(!initialized) LoadMappingsFromConfig();
			if (namedInstances.ContainsKey(name))
			{
				return namedInstances[name];
			}
			var key = new Tuple<string, Type>(name, @from);
			if (namedTypes.ContainsKey(key))
				return CreateInstance(@from, namedTypes[key]);

			return null;
		}

		public void RegisterType<TFrom, TTo>(string name)
		{
			RegisterType(typeof(TFrom), typeof(TTo), name);
		}

		public Dictionary<string, object> namedInstances = new Dictionary<string, object>();

		public void RegisterInstance(object instance, string name)
		{
			if (instance == null) throw new ArgumentNullException("instance");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
			namedInstances.Add(name, instance);
		}

		public void RegisterType(Type typeFrom, Type typeTo, string name)
		{
			namedTypes.Add(new Tuple<String, Type>(name, typeFrom), typeTo);
		}

		public void RegisterTypeFunc(Type @from, Func<Type, object> func)
		{
			funcs.Add(@from, func);
		}

		private object CreateInstance(Type type, Type to)
		{
			if (to.ContainsGenericParameters && !type.ContainsGenericParameters)
			{
				to = to.MakeGenericType(type.GetGenericTypeArguments());
			}

			// go through each constructor looking for one where
			// we know how to instantiate all the types
			foreach (var constructor in to.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
										 .OrderBy(e => e.GetParameters().Length)
										 .Select(e => e))
			{
				var paramTypes = constructor.GetParameters().Select(e => FindType(e.ParameterType)).ToArray();
				if (paramTypes.Any(e => e == null)) continue;
				var objects = paramTypes.Select(Resolve).ToArray();
				return Activator.CreateInstance(to, objects);
			}
			return null;
		}
	}
}