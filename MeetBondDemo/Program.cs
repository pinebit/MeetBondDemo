namespace MeetBondDemo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Bond;
    using Bond.Protocols;
    using Bond.IO.Unsafe;
    using HttpLog;

    public static class Program
    {
        static void Main()
        {
            // HttpRequestData is code-generated (see the MSBuild task in .csproj) from HttpLogger.bond file
            HttpRequest src = new HttpRequest
                                  {
                                      Verb = HttpVerb.GET,
                                      Url = "https://www.random.org/cgi-bin/randbyte?nbytes=16&format=h",
                                      When = DateTime.Now,
                                      Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                                  };

            // Serialize
            var output = new OutputBuffer();
            var writer = new SimpleBinaryWriter<OutputBuffer>(output);

            Serialize.To(writer, src);
            Console.WriteLine("Serialized HttpRequest size = {0} bytes", output.Position);

            // Deserialize
            var input = new InputBuffer(output.Data);
            var reader = new SimpleBinaryReader<InputBuffer>(input);
            HttpRequest dst = Deserialize<HttpRequest>.From(reader);
            Console.WriteLine("[{0}] {1} {2} with {3} header(s)", dst.When, dst.Verb, dst.Url, dst.Headers.Count);

            // Compare original object to the deserialized one
            Debug.Assert(Comparer.Equal(src, dst), "Request data objects do not match");
        }
    }
}
