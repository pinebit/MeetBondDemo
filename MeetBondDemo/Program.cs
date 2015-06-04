namespace MeetBondDemo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Bond;
    using Bond.Protocols;
    using Bond.IO.Unsafe;
    using EventLog;

    public static class Program
    {
        static void Main()
        {
            Console.Write("Reading all Application log events... ");
            
            List<EventRecordData> allEvents = Reader.ReadAllEvents().ToList();
            
            Console.WriteLine("{0} events successfully read.", allEvents.Count);

            // OutputBuffer will receive serialized data
            var output = new OutputBuffer();
            // Choose another protocol writer to match your needs
            var writer = new SimpleBinaryWriter<OutputBuffer>(output);

            Console.Write("Serializing all records... ");
            Stopwatch stopwatch = new Stopwatch();

            // Serialize.To performs actual serialization
            stopwatch.Start();
            allEvents.ForEach(x => Serialize.To(writer, x));
            stopwatch.Stop();

            Console.WriteLine("done in {0} ms, output size = {1} Kb", stopwatch.ElapsedMilliseconds, output.Position / 1024);

            // InputBuffer represents octet stream source for deserialization
            var input = new InputBuffer(output.Data);
            // Protocol reader shall match the writer from above
            var reader = new SimpleBinaryReader<InputBuffer>(input);

            Console.Write("Deserializing all records... ");
            Dictionary<EventLevel, int> counts = new Dictionary<EventLevel, int>();

            stopwatch.Restart();
            for (int i = 0; i < allEvents.Count; i++)
            {
                // Deserializing..
                EventRecordData record = Deserialize<EventRecordData>.From(reader);

                if (!counts.ContainsKey(record.Level))
                {
                    counts[record.Level] = 0;
                }

                counts[record.Level]++;
            }
            stopwatch.Stop();

            Console.WriteLine("done in {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Statistics:");
            foreach (KeyValuePair<EventLevel, int> pair in counts.OrderByDescending(x => x.Value))
            {
                Console.WriteLine("\t{0} events of level {1}", pair.Value, pair.Key);
            }
        }
    }
}
