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
using Microsoft.Performance.SDK.Extensibility;
using System;

namespace wpa_plugin_etl
{
    public class ReadGPCEvent : IKeyedDataType<string>
    {
        public ulong Core { get; set; }
        public String Event { get; set; }
        public uint EventIdx { get; set; }
        public uint GPCIdx { get; set; }
        public String EventNote { get; set; }
        public ulong Value { get; set; }
        public string Key { get; set; }
        public Timestamp Time { get; set; }

        public ReadGPCEvent() { }

        public ReadGPCEvent(ReadGPCEvent ev)
        {
            Core = ev.Core;
            Event = ev.Event;
            EventIdx = ev.EventIdx;
            EventNote = ev.EventNote;
            Value = ev.Value;
            GPCIdx = ev.GPCIdx;
            Key = ev.Key;
            Time = ev.Time;
        }

        public string GetKey()
        {
            return this.Key;
        }
    }
}