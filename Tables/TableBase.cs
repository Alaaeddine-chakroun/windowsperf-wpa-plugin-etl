using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace wpa_plugin_etl.Tables
{
    [Table]
    public sealed class TableBase
    {
        public TableBase(IReadOnlyList<Tuple<String, DateTime, Timestamp, Timestamp, long>> events)
        {
            this.Events = events;
        }
        public static TableDescriptor TableDescriptor => new TableDescriptor(
                    Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"), // The GUID must be unique across all tables
                    "Values Table",                                         // The Table must have a name
                    "Values ABC",                               // The Table must have a description
                    "Value");                                             // A category is optional. It useful for grouping different types of tables in the viewer's UI.


        private static readonly ColumnConfiguration EventColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{E7907471-C6E2-41D2-AAA2-E4D790EB8676}"), "Event", "The number of characters in the word."),
           new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TimeColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{E3056A08-D44D-4CD6-8158-503BDAEF899C}"), "Time", "The number of characters in the word."),
           new UIHints { 
               IsVisible = true,
               Width = 150 });

        private static readonly ColumnConfiguration EndTimeColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{4A9464D3-9A17-4E3C-86D9-9E57C97785AE}"), "End Time", "The number of characters in the word."),
           new UIHints
           {
               IsVisible = true,
               Width = 150
           });

        private static readonly ColumnConfiguration ValueColumn = new ColumnConfiguration(
         new ColumnMetadata(new Guid("{54B4A016-9F78-4BAE-A0AB-AFDFBF33C3F1}"), "Value", "The time when the word is written to the file."),
         new UIHints { 
             AggregationMode = AggregationMode.Sum,
             Width = 150 });

        public IReadOnlyList<Tuple<string, DateTime, Timestamp, Timestamp, long>> Events { get; }

        internal void Build(ITableBuilder tableBuilder)
        {
            var baseProjection = Projection.Index(this.Events);

            var eventProjection = baseProjection.Compose(x => x.Item1);
            var timeProjection = baseProjection.Compose(x => x.Item3);
            var endTimeProjection = baseProjection.Compose(x => x.Item4);
            var valueProjection = baseProjection.Compose(x => x.Item5);
            
            var config = new TableConfiguration("Values Table")
            {
                Columns = new[]
                {
                    EventColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    TimeColumn,
                    EndTimeColumn,
                    TableConfiguration.GraphColumn,
                    TableConfiguration.RightFreezeColumn,
                    ValueColumn,
                },
            };

            config.AddColumnRole(ColumnRole.StartTime, TimeColumn.Metadata.Guid);
            config.AddColumnRole(ColumnRole.EndTime, EndTimeColumn.Metadata.Guid);

            _ = tableBuilder.AddTableConfiguration(config)
               .SetDefaultTableConfiguration(config)
               .SetRowCount(this.Events.Count)
               .AddColumn(EventColumn, eventProjection)
               .AddColumn(TimeColumn, timeProjection)
               .AddColumn(EndTimeColumn, endTimeProjection)
               .AddColumn(ValueColumn, valueProjection);            
            
        }
    }
}
