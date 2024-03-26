﻿using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpa_plugin_etl.Tables
{
    [Table]
    internal class ReadGPCAppTable
    {
        public ReadGPCAppTable(IReadOnlyList<Tuple<string, DateTime, Timestamp, ReadGPCEventApp>> events)
        {
            this.Events = events;
        }
        public static TableDescriptor TableDescriptor => new TableDescriptor(
                    Guid.Parse("{CC0CDEC2-710D-4955-8C3C-CEBF606960D2}"),
                    "WindowsPerf GPC Data",
                    "GPC data gathered with WindowsPerf App",
                    "PMU");

        private static readonly ColumnConfiguration EventIdxColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{58A54596-0A1A-4F59-91E5-4F231F80F06E}"), "Event Index", "The raw index of the event"),
           new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TimeColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{61979965-B0B8-4529-AE52-3C6E2A58BA8A}"), "Start Time", "The start time of the event"),
           new UIHints
           {
               IsVisible = false,
               Width = 150
           });

        private static readonly ColumnConfiguration CoreColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{B8F574DD-1DAC-44AD-A623-4B39D39A09F6}"), "Core", "The CPU core that generated this event"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration EventColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{15C1CB95-FA17-4C10-8EE9-1851C8992D8E}"), "Event Name", "The human readable name of the event"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration EventNoteColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{C646628B-DD05-489A-9DF1-A1BB2E62561D}"), "Event Note", "The event note"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration ValueColumn = new ColumnConfiguration(
         new ColumnMetadata(new Guid("{30971239-377C-443C-B00D-EE8047E59B28}"), "Value", "The value read"),
         new UIHints
         {
             AggregationMode = AggregationMode.Sum,
             Width = 150
         });

        public IReadOnlyList<Tuple<string, DateTime, Timestamp, ReadGPCEventApp>> Events { get; }

        internal void Build(ITableBuilder tableBuilder)
        {
            var baseProjection = Projection.Index(this.Events);

            var eventProjection = baseProjection.Compose(x => x.Item4.Event);
            var timeProjection = baseProjection.Compose(x => x.Item3);

            var coreProjection = baseProjection.Compose(x => x.Item4.Core);
            var eventIdxProjection = baseProjection.Compose(x => x.Item4.EventIdx);
            var eventNoteProjection = baseProjection.Compose(x => x.Item4.EventNote);
            var valueProjection = baseProjection.Compose(x => x.Item4.Value);

            var config = new TableConfiguration("PMU Data")
            {
                Columns = new[]
                {
                    EventColumn,
                    CoreColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    EventIdxColumn,
                    EventNoteColumn,
                    TimeColumn,
                    TableConfiguration.GraphColumn,
                    TableConfiguration.RightFreezeColumn,
                    ValueColumn,
                },
            };

            config.AddColumnRole(ColumnRole.StartTime, TimeColumn.Metadata.Guid);

            _ = tableBuilder.AddTableConfiguration(config)
               .SetDefaultTableConfiguration(config)
               .SetRowCount(this.Events.Count)
               .AddColumn(EventColumn, eventProjection)
               .AddColumn(TimeColumn, timeProjection)
               .AddColumn(CoreColumn, coreProjection)
               .AddColumn(EventIdxColumn, eventIdxProjection)
               .AddColumn(EventNoteColumn, eventNoteProjection)
               .AddColumn(ValueColumn, valueProjection);

        }
    }
}