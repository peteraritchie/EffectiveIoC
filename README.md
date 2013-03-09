EffectiveIoC
============

Bare-bones Inversion of Control container

This project is basically the outcome of a short spike to get inversion of control with little or no fuss.  I felt that most IoC controllers, as powerful and useful as they were, were more complicated to learn and config than it seemed writing a useful IoC would be.  The results of my tweets RE IoC in < 60 lines.

###Philosophy
I recently [blogged](http://bit.ly/Zm1vIM) about Dependency Injection where I detailed some of my thoughts on the complexity that manu IoC containers tend to promote.

EffectiveIoC is intended to be an easy-to-use IoC container that promotes DI-friendly design.  So, EffectiveIoC really only supports contructor injection.  It supports generics and open generics.

It follows the philosphy of *use what's available* (no custom config section) and *KISS* (a static IoC class with only two methods).

###Usage
Mappings can be performed in app.config via <see cref="T:System.Configuration.NameValueSectionHandler"/>.  For example
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
IEnumerable<int> instance = IoC.Resolve<IEnumerable<int>>();
```
Alternatively we could manually register a mapping with the <see cref="M:PRI.EffectiveIoC.IoC.RegisterType``2"/> method.  For example:
```C#
IoC.RegisterType(typeof(IList<>), typeof(List<>));
```
Which could then be resolved as follows:
```C#
IoC.Resolve<IList<int>>();
```