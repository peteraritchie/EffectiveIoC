EffectiveIoC
============

Bare-bones Inversion of Control container

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