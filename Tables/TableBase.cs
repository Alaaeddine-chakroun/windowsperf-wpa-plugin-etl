// BSD 3-Clause License
//
// Copyright (c) 2024, Arm Limited
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
