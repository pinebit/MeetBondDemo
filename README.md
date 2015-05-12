# Meet Bond, Microsoft Bond

## Intro

I bet every experienced developer has been working with data serialization at least once. Let's recap where this matters most:

* Save and restore your business data in files or streams
* Passing messages in networks
* Objects persistence in NoSQL, BigData

There should be no issues if your scope is limited to a single piece of software and your software lifetime is shorter that your life. That nice time had passed away. Nowadays we are all building software components. We all want our components to be reused and seamlessly integrate with each other. To make this happen, you need to have well-defined IO. By saying "well-defined" I mean that your IO must have a contract expressed in a DSL (domain-specific language) that would guarantee other components you speak the same language. As long as all components obey the same contract, they are fine. Nonetheless to say this is vital for heterogeneous systems built by giants like Google, Facebook, Amazon, Microsoft...

Rivalry led to a number of incompatible DSLs and frameworks, each giant has built for its own needs. I hope you've heard of at least one of these:

* Google Inc. - [Google Protocol Buffers](https://developers.google.com/protocol-buffers/)
* Facebook Inc. - [Thrift](http://thrift.apache.org/), which is now maintained by Apache
* Apache Foundation Software - [Avro](http://avro.apache.org/) 

Microsoft has many online services too and therefore it has the same needs. Just think about Bing, MSN, AdCenter, Azure and many other services demanding seamless interoperability. There was an internal technology created for that purpose which was pretty good, but it did not address newer use cases defined by rapidly growing online services division. Here's where Microsoft Bond was born. For several years, Bond remained an internal use only technology, but since Microsoft has decided to go Open Source - Bond has been finally published on GitHub: [Microsoft Bond]((https://github.com/Microsoft/bond)).

That said, let's add Microsoft Bond to the list above and read this article about what Bond offers and look at few examples.

## Why Bond?

The official answer to this question is here: ["Why Bond"](https://microsoft.github.io/bond/why_bond.html).

Here is my answer:

* Bond supports rich type system including generics
* Bond supports schema versioning and two-way compatibility
* Bond supports runtime schema manipulation
* Bond supports various collections: `vector<T>, map<T>, list<T>`
* Bond supports type-safe lazy serialization: `bonded<T>`
* Bond supports pluggable protocols (formats) with marshaling and transcoding

An important note is that Bond follows "pay to play" strategy: the more features you add/use the more you pay for size and speed. This gives developers great flexibility. I've never seen the same in competitive frameworks.

Let's be honest and list the drawbacks as well:

* Bond is targeting Microsoft stack by supporting C++ and C#, but not supporting Java (yet)
* Bond does not support union type (`oneof` in protobuf)

### What about performance?

The next question I would expect to hear is "what about Bond performance?". My answer to this is as follow: Bond is nearly similar to competitors in terms of performance, but it provides greater features while others don't. If you are building a system using Microsoft stack, then Bond would be your best choice. If you are looking for precise performance metrics, I encourage you to build your own test that would deal with *your data schema* and deal with *a protocol of your choice*. You will probably notice few percent difference in speed/size, but you will also notice great features described previously.

## Using Bond

### Getting Bond

TBD

### Summary on DSL (IDL) features

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

```
    namespace MyProject
    
    struct MyRecord
    {
        0: string Name = "Noname";
        1: vector<double> Constants;
    }
```

Here,
    
* `0` and `1` are the field ordinal numbers (can be any integers with any pace).
* `= "Noname"` is the (optional) default value.

Pretty natural for C/C++/C# developers, isn't it?

### Code generation

Bond framework provides code generation tool written in ... Haskell (wow).
Here is how to generated C# and C++ code from a `.bond` schema:

```powershell
    gbc c# example.bond
    gbc c++ example.bond
```

> The current version of Bond framework also supports Python, but I did not try it yet.

Bond framework also provides helpful MSBuild integration (MSBuild tasks and props) that will run code generation for you in your Visual Studio environment.
Once generated, reference the generate file(s) to your project and link (reference) the libraries. You project is now Bond-ready.

### Supported protocols (formats)

Out of the box Bond supports three kinds of protocols:

* Tagged protocols: `CompactBinary` and `FastBinary`
> Tagged protocols interleave schema metadata within the payload. This makes the payload self-describing, allowing consumers to interpret it even without knowing the schema used by the producer.

* Untagged protocols: `SimpleBinary`
> Untagged protocols serialize only data and thus require that consumers know the payload schema via some out-of-band mechanism. Untagged protocols are often used in storage scenarios because they allow storing a schema once (e.g. in a system table in a database) and thus eliminating metadata overhead from many records using the same schema.

* DOM-based protocols: `SimpleJson` and `SimpleXml`
> DOM-based protocol parse whole payload into an in-memory Data Object Model which then is queried during deserialization. Typically this kind of protocol is used to implement text based encoding such as JSON or XML.

For each protocol, Bond API provides you with the corresponding Reader and Writer classes, which do the job on the actual serialization. Remember that previously you've asked me about performance: it's obviously that untagged protocol(s) would be the fastest, because they don't write auxiliary information, hence they write less data (copy less memory, etc).  

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

## Demo

If you check the official [Bond GitHub repository](https://github.com/Microsoft/bond), you'll find a plenty of demos in there demonstrating every single feature of Bond. Some people (like me) however, would prefer a demo that is as closer to the real life scenario as possible. Specially for this article, I have cooked such a demo. This will let you play with Bond in Visual Studio (2012 onward) and even measure size and speed:
[See MeetBondDemo on my GitHub](https://github.com/pinebit/MeetBondDemo)

> To build and run the demo, you don't need to install any software other than Visual Studio. 

## Ideas

If you love Bond, and if you have a plenty of spare time for coding, consider taking one of these items into development. I won't be enumerating all benefits you may get from contributing, but I know many developers are looking for something to contribute to and become a superstar!

* Implement a port to Java. Replace Java with other mainstream language at your choice.
* Implement Bond schema import/export to exchange with other DSLs (e.g. `.proto <=> .bond`).

Whatever you decided to do regarding Bond, I recommend you to contact [Adam Sapek](https://github.com/sapek) first. He is the lead of this project and he will guide you with what's most demanded by market.

## Links

* [Microsoft Bond project on GitHub](https://github.com/Microsoft/bond)
* ["Why Bond" article](https://microsoft.github.io/bond/why_bond.html)
* ["A Young Person's Guide to C#"](https://microsoft.github.io/bond/manual/bond_cs.html)
* ["A Young Person's Guide to C++"](https://microsoft.github.io/bond/manual/bond_cpp.html)
* ["Python bindings to Bond"](https://microsoft.github.io/bond/manual/bond_py.html)
