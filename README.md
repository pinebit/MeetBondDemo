# Meet Bond, Microsoft Bond

## Intro

Bond is a new serialization framework for schematized data created by Microsoft.

Let's recap where data serialization used most:

* Data persistence in files, streams, NoSQL, BigData
* Data transmission in networks, IPC, etc.

In common, these applications have to deal with schematized data, where schema means:

* Structure: hierarchy, relations, order
* Semantic: age is number of years since born.

Actually, any date has schema even if it is implicitly defined or supported by your programming language out-of-box.
When it comes to complex data structures, we end up writing supporting data transfer objects (DTOs) and code responsible for IO, often in different languages.
As soon as it grows and evolves, it quickly becomes a nightmare to maintain all these pieces. Here is where the serialization frameworks win the game.

First of all, any serialization framework defines an abstraction for data schema definition that is not bound to a particular programming language or platform.
This abstraction is known as [DSL (domain-specific language)](http://en.wikipedia.org/wiki/Domain-specific_language).
Having such a DSL, we can define data schema for a particular application. The definition, in turn, can be expressed in multiple forms, but often
serialization frameworks support a single form, which is well suited for its DSL. Too complicated? Here is one well-known example: XSD and XML.
XSD defines a DSL, XML is (recommended) to define documents matching the XSD schema. But, you can also use `xsd.exe` to generate DTO classes matching the XSD,
hence the generated classes are just another form. Note that your can generate XML from DTOs and vice versa and they will be semantically identical,
because the semantic is common: it is defined with the XSD. Summarizing: a serialization framework provides you with a DSL, which you use to
define data schemas in a certain format that is supported best by the given framework.

The abstract data schema shall be eventually materialized into a set of entities expressed in a programming language. All serialization frameworks provide special tools called code generators.
They generate all supporting code for target programming languages, which is needed to clients to work with schematized data: DTOs, proxy, etc. This is ultimately required for
strongly-typed languages, while it can be optional for duck-typed (dynamic) languages. 

The last but not least matter is the data persistence on the wire. The actual data shall be eventually serialized into raw bytes (or text) and deserialized back.
All serialization frameworks provide another abstraction here called protocols. A protocol defines a set of rules that define how structured data
should be serialized or deserialized in according with its schema. Each protocol is normally implemented for all programming languages and platforms
supported by given serialization framework. The more programming languages/platforms it supports, the more implementations it shall provide.
Imagine a framework is willing to support JSON protocol, then it must provide JSON reader/writer for say C#, C++, Windows, Linux, etc.

Putting all together: any modern data serialization framework provides you with the following:

* Abstractions: DSL and Protocols
* Code generation tools
* Protocol implementations

Microsoft Bond is a modern data serialization framework. It provides powerful DSL and flexible protocols, code generators for C++ and C#, efficient protocol implementations for Windows, Linux and Mac OS X.

For several years, Bond remained an internal use only technology, but thanks to Microsoft Open Source initiative, Bond has been made available on GitHub: [Microsoft Bond]((https://github.com/Microsoft/bond)). 

### Competitors

Rivalry led to a number of serialization frameworks to appear each software giant has built:

* Google Inc. - [Google Protocol Buffers](https://developers.google.com/protocol-buffers/)
* Facebook Inc. - [Thrift](http://thrift.apache.org/), which is now maintained by Apache
* Apache Foundation Software - [Avro](http://avro.apache.org/) 

Obviously, they are all incompatible, which is Okay unless you make your public API using one of these. 
Each of them has pros and cons, so you can choose from them based on your needs.

### Why Bond?

The official answer to this question is here: ["Why Bond"](https://microsoft.github.io/bond/why_bond.html).

Here is the quick summary:

* Bond supports rich type system including generics
* Bond supports schema versioning and two-way compatibility
* Bond supports runtime schema manipulation
* Bond supports various collections: `vector<T>, map<T>, list<T>`
* Bond supports type-safe lazy serialization: `bonded<T>`
* Bond supports pluggable protocols (formats) with marshaling and transcoding

An important note is that Bond follows "pay to play" strategy: the more features you add/use the more you pay for size and speed. This gives developers great flexibility.

Let's be honest and list the drawbacks as well:

* Bond is targeting Microsoft stack with supporting C++ and C#, but not supporting Java (yet)
* Bond does not support union type (`oneof` in protobuf)

### What about performance?

When it comes to compare one framework to another, developers are looking for performance comparisons.
But let's recall that these frameworks consists of DSL, code generators and protocols.
If you consider protocols performance only, then you will miss the features provided by DSL and codegens.
Sometimes, having better DSL is way more important than having few percent difference in serialization speed.
Other than speed, the space-efficient encodings supported by some protocols could also be important.
I encourage you to do a performance/space comparison with your domain specific data. This is the only way to estimate all benefits you could get from a particular framework.  

This article comes with [the demo project](https://github.com/pinebit/MeetBondDemo) that demonstrates Bond framework usage by reading all records from Windows Application Event Log,
serializing them as Bond objects and deserialize them back. 

> To build and run the demo, you don't need to install any software other than Visual Studio. 

## Using Bond

### Getting Bond

Check [the official guide on getting Bond](https://github.com/Microsoft/bond) for your platform(s).

For .NET projects this is as simple as:

```powershell
    install-package Bond.CSharp
```  

The package includes:

* Code generator (gbc.exe) in bin folder
* .NET libraries
* MSBuild tasks

### Workflow

The workflow includes the following steps:

* Learn DSL and define data schema by writing `.bond` file(s)
* Use code generator (`gbc.exe`) to get DTOs for your programming language
* Reference the generated files as well as Bond runtime libraries in your project

Consider using MSBuild tasks provided with the framework to automate code generation step.

### DSL features overview

When you start writing your first `.bond` file, you will need to know its syntax and features.
Please visit the [official documentation page](https://microsoft.github.io/bond/manual/compiler.html#idl-syntax) describing the IDL in details. Here let's review just basic features:

* Modules: schema can be split into different files, which are included with `import' statement.
* Namespace: have the same meaning as C++/C# have.
* User defined structs: a unit of user type definition.
* Forward declaration is useful for recursive data structures.
* Basic types: `bool, uint8(through 64), int8(through 64), float, double, string, wstring`.
* Container types: `blob, list<T>, vector<T>, set<T>, map<K, T>, nullable<T>`.
* Custom types aliases and mapping, e.g. if you want to have `DateTime` in C#, but ticks (`int64`) on the wire.
* Custom attributes: useful for custom code generation.

Bored? Here is an example:

```c_cpp
    namespace MyProject
    
    struct MyRecord
    {
        0: string Name = "Noname";
        1: vector<double> Constants;
    }
```

where:
    
* `0` and `1` are the field ordinal numbers (can be any integers with any pace).
* `= "Noname"` is the (optional) default value.

### Code generation

Bond framework provides code generation tool written in Haskell.
Here is how to generate C# and C++ code from a `.bond` schema in command line:

```sh
    gbc c# example.bond
    gbc c++ example.bond
```

### Supported protocols (formats)

Out of the box Bond supports three kinds of protocols:

* Tagged protocols: `CompactBinary` and `FastBinary`
> Tagged protocols interleave schema metadata within the payload. This makes the payload self-describing, allowing consumers to interpret it even without knowing the schema used by the producer.

* Untagged protocols: `SimpleBinary`
> Untagged protocols serialize only data and thus require that consumers know the payload schema via some out-of-band mechanism. Untagged protocols are often used in storage scenarios because they allow storing a schema once (e.g. in a system table in a database) and thus eliminating metadata overhead from many records using the same schema.

* DOM-based protocols: `SimpleJson` and `SimpleXml`
> DOM-based protocol parse whole payload into an in-memory Data Object Model which then is queried during deserialization. Typically this kind of protocol is used to implement text based encoding such as JSON or XML.

For each protocol, Bond runtime library provides you with the corresponding Reader and Writer classes, which do the job on the actual serialization.
Using the protocols is pretty straightforward and merely harder than that famous `JsonConvert.SerializeObject()`:

```csharp
    var record = new MyRecord
    {
        Name = "FooBar",
        Constants = { 3.14, 6.28 }
    };

    var output = new OutputBuffer();
    var writer = new CompactBinaryWriter<OutputBuffer>(output);
    Serialize.To(writer, record);

    var input = new InputBuffer(output.Data);
    var reader = new CompactBinaryReader<InputBuffer>(input);
    record = Deserialize<Example>.From(reader);
``` 

## Ideas

If you love Bond, and if you have a plenty of spare time for coding, consider taking one of these items into development. I won't be enumerating all benefits you may get from contributing, but I know many developers are looking for ideas to contribute to:

* Implement a port to Java. Replace Java with other mainstream language at your choice.
* Implement Bond schema import/export to exchange with other DSLs (e.g. `.proto <=> .bond`).

Whatever you decided to do regarding Bond, I recommend you to contact [Adam Sapek](https://github.com/sapek) first. He is the lead of this project and he will guide you with what's most demanded by market.

## Links

* [Microsoft Bond project on GitHub](https://github.com/Microsoft/bond)
* [Why Bond](https://microsoft.github.io/bond/why_bond.html)
* [A Young Person's Guide to C#](https://microsoft.github.io/bond/manual/bond_cs.html)
* [A Young Person's Guide to C++](https://microsoft.github.io/bond/manual/bond_cpp.html)
* [Python bindings to Bond](https://microsoft.github.io/bond/manual/bond_py.html)
