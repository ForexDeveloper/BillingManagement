// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "HAA0102:Non-overridden virtual method call on value type", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Utilities.TextFormatter.Format(Serilog.Events.LogEvent,System.IO.TextWriter)")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Utilities.SerilogEventExtensions.ExtractMessage(Serilog.Events.LogEvent)~System.Object")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Shared.Logging.Serilog.Configurations.SerilogConfiguration.ExcludedLogPaths")]
[assembly: SuppressMessage("Performance", "HAA0102:Non-overridden virtual method call on value type", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Registration.AddAPSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Action{Shared.Logging.Serilog.Configurations.SerilogConfiguration})~Microsoft.AspNetCore.Builder.WebApplicationBuilder")]
[assembly: SuppressMessage("Performance", "HAA0102:Non-overridden virtual method call on value type", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Registration.AddCustomWorkerSerilog(Microsoft.Extensions.Hosting.IHostBuilder,System.Action{Shared.Logging.Serilog.Configurations.SerilogConfiguration})~Microsoft.Extensions.Hosting.IHostBuilder")]
[assembly: SuppressMessage("Performance", "HAA0301:Closure Allocation Source", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Registration.AddAPSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Action{Shared.Logging.Serilog.Configurations.SerilogConfiguration})~Microsoft.AspNetCore.Builder.WebApplicationBuilder")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:Shared.Logging.Serilog.Registration.AddAPSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Action{Shared.Logging.Serilog.Configurations.SerilogConfiguration})~Microsoft.AspNetCore.Builder.WebApplicationBuilder")]
