namespace MeetBondDemo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using EventLog;

    public static class Reader
    {
        public static IEnumerable<EventRecordData> ReadAllEvents()
        {
            EventLogQuery eventsQuery = new EventLogQuery("Application", PathType.LogName) { ReverseDirection = true };

            using (EventLogReader logReader = new EventLogReader(eventsQuery))
            {
                for (EventRecord record = logReader.ReadEvent(); null != record; record = logReader.ReadEvent())
                {
                    yield return new EventRecordData
                                 {
                                     Id = record.Id,
                                     ProviderName = record.ProviderName,
                                     TimeCreated = record.TimeCreated,
                                     Level = ConvertLevel(record.Level ?? 0),
                                     Properties = record.Properties.Select(x => x.Value.ToString()).ToList()
                                 };
                }
            }
        }

        private static EventLog.EventLevel ConvertLevel(byte level)
        {
            StandardEventLevel standardEventLevel = (StandardEventLevel)level;

            switch (standardEventLevel)
            {
                case StandardEventLevel.LogAlways:
                    return EventLog.EventLevel.LogAlways;

                case StandardEventLevel.Critical:
                    return EventLog.EventLevel.Critical;

                case StandardEventLevel.Error:
                    return EventLog.EventLevel.Error;

                case StandardEventLevel.Warning:
                    return EventLog.EventLevel.Warning;

                case StandardEventLevel.Informational:
                    return EventLog.EventLevel.Informational;

                case StandardEventLevel.Verbose:
                    return EventLog.EventLevel.Verbose;
            }

            throw new ArgumentOutOfRangeException("level");
        }
    }
}