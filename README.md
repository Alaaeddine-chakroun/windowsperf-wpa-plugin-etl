# WPA-plugin-etl

[[_TOC_]]

# Introduction

The [WPA-plugin-etl](https://gitlab.com/Linaro/WindowsPerf/wpa-plugin-etl) is a dedicated plugin developed for the Windows Performance Analyzer (WPA). Its primary function is to interpret and present event traces that have been injected by the WindowsPerf ETW (Event Tracing for Windows). These events can be injected through two main sources: the [wperf](https://gitlab.com/Linaro/WindowsPerf/windowsperf/-/tree/main/wperf?ref_type=heads) application and the [wperf-driver](https://gitlab.com/Linaro/WindowsPerf/windowsperf/-/tree/main/wperf-driver?ref_type=heads). The `wperf` application is a user-mode application, while the `wperf-driver` is a Windows Kernel Driver. 

Together, they provide a comprehensive view of system performance and behavior, making the `WPA-plugin-etl` a valuable tool for system analysis and debugging. This plugin enhances the capabilities of WPA, allowing users to delve deeper into the Arm core and uncore PMU performance characteristics of their Windows on Arm systems. It’s an essential tool for anyone looking to optimize system performance or troubleshoot issues.

##  What is WPA

Windows Performance Analyzer (WPA) is a tool that creates graphs and data tables of Event Tracing for Windows (ETW) events
that are recorded by Windows Performance Recorder (WPR), Xperf, or an assessment that is run in the
Assessment Platform. WPA can open any event trace log (ETL) file for analysis.

# Installation
WPA is included in the Windows Assessment and Deployment Kit (Windows ADK) that can be downloaded [here](https://go.microsoft.com/fwlink/?linkid=2243390).

> :warning: The wperf WPA plugin requires a WPA version of `11.0.7.2` or higher.

Once downloaded, make sure that the "Windows Performance Toolkit" checkbox is checked under "Select the features you want to install".

## Plugin releases

WPA-plugin is built on the [Microsoft Performance Toolkit SDK](https://github.com/microsoft/microsoft-performance-toolkit-sdk) and is shipped as a set of Dynamic link libraries (DLLs).
Go to `WPA-plugin-etl` [releases](https://gitlab.com/Linaro/WindowsPerf/wpa-plugin-etl/-/releases) to download the latest plugin binaries.

## Installation instructions in WPA

There are 2 different methods to install the plugin:

- Moving the plugin dll to the **CustomDataSources** directory next
to the WPA executable (defaults to `C:\\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\CustomDataSources`).
- Calling `wpa` from the command line and passing the plugin directory to the `-addsearchdir` flag (example : `wpa -addsearchdir "%USERPROFILE%\plugins"`).

> To verify that the plugin is loaded successfully, launch WPA then the plugin should appear under Help > About Windows Performance Analyzer.

# Contributing

To contribute to the project follow our [Contributing Guidelines](CONTRIBUTING.md).

# License

All code in this repository is licensed under the [BSD 3-Clause License](LICENSE).
