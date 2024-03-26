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

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpa_plugin_etl
{
    [ProcessingSource(
        "{3ABDFE57-0E4F-4223-A7F4-3F6F5813D741}",  // The GUID must be unique for your Processing Source. You can use Visual Studio's Tools -> Create Guid… tool to create a new GUID
        "WindowsPerf WPA Plugin ETL",    
        "WindowsPerf Driver ETW Reader")]
    [FileDataSource(
        ".etl",                          
        "ETL files")]
    public class WpaPluginEtlProcessingSource : ProcessingSource
    {
        private IApplicationEnvironment applicationEnvironment;

        public override ProcessingSourceInfo GetAboutInfo()
        {
            return new ProcessingSourceInfo
            {
                CopyrightNotice = "Copyright 2024 Linaro",

                LicenseInfo = new LicenseInfo
                {
                    Name = "XXX",
                    Text = "Please see the link for the full license text.",
                    Uri = "https://a.b.com/LICENSE.txt",
                },

                Owners = new[]
                {
                    new ContactInfo
                    {
                        Address = "XXX",
                        EmailAddresses = new[]
                        {
                            "X@X.com",
                        },
                    },
                },

                ProjectInfo = new ProjectInfo
                {
                    Uri = "https://a.b.com",
                },

                AdditionalInformation = new[]
                {
                    "XXX.",
                }
            };
        }

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            this.applicationEnvironment = applicationEnvironment;
        }

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            return new WpaPluginEtlDataProcessor(
                dataSources.Select(x => x.Uri.LocalPath).ToArray(),
                options,
                this.applicationEnvironment,
                processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource source)
        {
            Debug.WriteLine("yadayada");
            return true;
        }
    }
}
