# Meet Bond, Microsoft Bond

## Intro

I bet every experienced developer has been working with data serialization at least once. Let's recap where this matters most:

* Save and restore your business data in files or streams
* Passing messages in networks
* Objects persistence in NoSQL databases

There should be no issues if your scope is limited to a single piece of software and your software lifetime is short. That nice time had passed away. Nowadays we are all building software components. We all want our components to be reused and seamlessly integrate to each other. To make this happen, you need to have well-defined IO. By saying "well-defined" I mean that your IO must have a contract expressed in a DSL (domain-specific language) that would guarantee other components you speak the same language. Nonetheless to say this is vital for heterogeneous systems built by giants like Google, Facebook, Amazon, Microsoft...

Rivalry led to a number of incompatible DSLs and frameworks, each giant has built for its own needs. I hope you've heard of at least one of these:

* Google Inc. - [Google Protocol Buffers](https://developers.google.com/protocol-buffers/)
* Facebook Inc. - [Thrift](http://thrift.apache.org/), which is now maintained by Apache
* Apache Foundation Software - [Avro](http://avro.apache.org/) 

Microsoft has many online services too and therefore it has the same needs. Just think about Bing, MSN, AdCenter, Azure and many others services demanding seamless interoperability. There was an internal technology created for that purpose which was pretty good, but it did not address newer use cases defined by rapidly growing online services division. Here's where Microsoft Bond was born. For several years, Bond remained an internal use only technology, but since Microsoft has decided to go Open Source - Bond has been finally published on GitHub: [Microsoft Bond]((https://github.com/Microsoft/bond)).

That said, let's add Microsoft Bond to the list above and read this article about what Bond offers while others don't and look at few examples.  

## Why Bond is better than others?

The best answer to this question is here: ["Why Bond"](https://microsoft.github.io/bond/why_bond.html).

Here is quick summary:

* Bond supports rich type system including generics
* Bond supports schema versioning and two-way compatibility
* Bond supports runtime schema IO
* Bond supports various collections: `vector<T>, map<T>, list<T>`
* Bond supports type-safe lazy serialization: `bonded<T>`
* Bond supports pluggable protocols (formats) with marshaling and transcoding

What even more important is that Bond follows "pay to play" strategy: the more features you add/use the more you pay for footprint and speed. This allows developers to get most of the Bond at a minimum cost.

The next question I would expect to hear is "what about Bond performance?". My answer to this is as follow: Bond is nearly similar to competitors in terms of performance, but it provides greater features while others don't. If you are building a system using Microsoft stack, then Bond would be your best choice. If you are really looking for any precise performance comparison, I would recommend you to author your own test that would deal with your data schema. You will notice few percent difference for sure, but you will also notice great features described above.

## Give me an example, please

For data contract definition, Bond uses a language similar to [IDL](http://en.wikipedia.org/wiki/Interface_description_language), having files with extension `.bond`.

```
    namespace Examples

    struct Record
    {
        0: string Name;
        1: vector<double> Constants;
    }
``` 

Similar to other serialization frameworks, you use a compiler that will generate smart DTO classes for your schema:

```
    gbc c# example.bond
```  

The usage in C#  is merely harder than that famous `JsonConvert.SerializeObject()`:

```csharp
    var src = new Example
    {
        Name = "FooBar",
        Constants = { 3.14, 6.28 }
    };

    var output = new OutputBuffer();
    var writer = new CompactBinaryWriter<OutputBuffer>(output);
    Serialize.To(writer, src);
```

... and deserialization:

```csharp
    var input = new InputBuffer(output.Data);
    var reader = new CompactBinaryReader<InputBuffer>(input);
    var dst = Deserialize<Example>.From(reader);
```

As you can see, the technology is well-suited for streams of objects and this is where it exhibits the best performance. Also, note that you can choose between different protocols (see next section) or you can implement your own protocol.

The same usage in C++ would look like this:

```C++
    Examples::Record src;
    src.Name = "test";
    src.Constants.push_back(3.14);
    
    bond::OutputBuffer output;
    bond::CompactBinaryWriter<bond::OutputBuffer> writer(output);
    Serialize(src, writer);

    bond::InputBuffer input(output.GetBuffer());
    bond::CompactBinaryReader<bond::InputBuffer> reader(input);
    Examples::Record dst;
    Deserialize(reader, dst);
```

## Supported protocols

Out of the box Bond supports three kinds of protocols:

* Tagged protocols: `CompactBinary` and `FastBinary`
> Tagged protocols interleave schema metadata within the payload. This makes the payload self-describing, allowing consumers to interpret it even without knowing the schema used by the producer.

* Untagged protocols: `SimpleBinary`
> Untagged protocols serialize only data and thus require that consumers know the payload schema via some out-of-band mechanism. Untagged protocols are often used in storage scenarios because they allow storing a schema once (e.g. in a system table in a database) and thus eliminating metadata overhead from many records using the same schema.

* DOM-based protocols: `SimpleJson` and `SimpleXml`
> DOM-based protocol parse whole payload into an in-memory Data Object Model which then is queried during deserialization. Typically this kind of protocol is used to implement text based encoding such as JSON or XML.


## Links

* [Microsoft Bond project on GitHub](https://github.com/Microsoft/bond)
* ["Why Bond" article](https://microsoft.github.io/bond/why_bond.html)
* ["A Young Person's Guide to C#"](https://microsoft.github.io/bond/manual/bond_cs.html)
* ["A Young Person's Guide to C++"](https://microsoft.github.io/bond/manual/bond_cpp.html)
* ["Python bindings to Bond"](https://microsoft.github.io/bond/manual/bond_py.html)
