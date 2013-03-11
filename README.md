EffectiveIoC
============

Bare-bones Inversion of Control container

This project is basically the outcome of a short spike to get inversion of control with little or no fuss.  I felt that most IoC controllers, as powerful and useful as they were, were more complicated to learn and config than it seemed writing a useful IoC would be.  The results of my tweets RE IoC in < 60 lines.

###Philosophy
I recently [blogged](http://bit.ly/Zm1vIM) about Dependency Injection where I detailed some of my thoughts on the complexity that many IoC containers tend to promote.

EffectiveIoC is intended to be an easy-to-use IoC container that promotes DI-friendly design.  So, EffectiveIoC really only supports constructor injection.  It supports generics and open generics.

It follows the philosophies of:
 - *Use what's available* (no custom config section)
 - *KISS* (a static IoC class with only two methods).
 - Don't try to be everything to everyone and end up being mediocre at everything

So, EffectiveIoC doesn't make you include a custom configuration section into your app.config and thus doesn't make you learn how to use a new configuration section.  It just uses the built-in `NamedValueCollection`.  (See below)

####Requirements
EffectiveIoC is not meant to replace existing IoC containers.  EffectiveIoC had some simple requirements:
- Resolution of types not necessarily in current AppDomain
- Simple app.config configuration (see above)
- Supprt constructor dependency injection
- Resolution of instances by name
- Creation of closed generic type instances from open generic type mappings 
- Time-boxed to less than a week of work

###When to use
Clearly, EffectiveIoC has a vary narrow focus.  If you need things like object lifecycle management, property injection, method injection, injection of values, or creation of arrarys then you probably don't want to use EffectiveIoC and should choose one of the other IoC containers like:
- [StructureMap](http://docs.structuremap.net/)
- [AutoFac](http://code.google.com/p/autofac/)
- [Castle Windsor](http://docs.castleproject.org/Default.aspx?Page=MainPage&NS=Windsor&AspxAutoDetectCookieSupport=1)
- [NInject](http://ninject.org/)
- [Unity](http://www.codeplex.com/unity)
- [Sprint.NET](http://www.springframework.net/)
- [LinFu](http://www.codeproject.com/Articles/20884/Introducing-the-LinFu-Framework-Part-I-LinFu-Dynam)
- [PicoContainer.NET](http://docs.codehaus.org/display/PICO/Home)

[see also](http://www.hanselman.com/blog/ListOfNETDependencyInjectionContainersIOC.aspx)

###Usage
Type mappings can be performed in app.config via <see cref="T:System.Configuration.NameValueSectionHandler"/>.  For example
```XML
<configSections>
    <section name="types" type="System.Configuration.NameValueSectionHandler"/>
</configSections>
    <types>
    <add
        key="System.Collections.Generic.ICollection`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
        value="System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"/>
    <add
        key="System.Collections.Generic.IEnumerable`1, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        value="System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"/>
    </types>
```
Given the above app.config, we could resolve a IEnumerable<T> instance as follows:
```C#
IEnumerable<int> instance = ioc.Resolve<IEnumerable<int>>();
```
Alternatively we could manually register a mapping with the `ioc.RegisterType` method.  For example:
```C#
ioc.RegisterType(typeof(IList<>), typeof(List<>));
```
Which could then be resolved as follows:
```C#
ioc.Resolve<IList<int>>();

```
You can also register instances within the config:
```XML
  <configSections>
    <section name="instances" type="System.Configuration.NameValueSectionHandler"/>
  </configSections>
  <instances>
    <add 
      key="myIntList"
      value="System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"/>
  </instances>
```
Given the above app.config, we could resolve an instance as follows:
```C#
var instance = ioc.Resolve<List<int>>("myIntList");
```

You can also register instances in code, which is useful if their construction is complex:
```C#
ioc.RegisterInstance(new Person("Peter", "Ritchie), "peter");
```

and then be resolved in the same way as instance registered in config:
```C#
var instance = ioc.Resolve<Person>("peter");
```
