using ezvm.Core.Programming.DTO;
using ezvm.Core.VM;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ezvm
{
    [Command(Name ="ezvm", Description ="ezvm command line interface")]
    [Subcommand(typeof(Validate), typeof(Run))]
    class Program
    {
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }

        private class ProgramBasedCommand
        {
            [Required(ErrorMessage = "You must specify the program path")]
            [Argument(0, "program", "Path to program (JSON)")]
            public string ProgramPath { get; }
        }

        private static void DumpException(Exception ex, IConsole console)
        {
            while(ex != null)
            {
                console.WriteLine(ex.Message);
                console.WriteLine(ex.StackTrace);
                ex = ex.InnerException;
                if (ex != null)
                    console.WriteLine("##### Inner exception:");
            }
        }

        [Command("validate", Description = "Validate a program")]
        private class Validate : ProgramBasedCommand
        {

            [Option(Description = "Verbose output")]
            public bool Verbose { get; } = false;

            private int OnExecute(IConsole console)
            {
                var file = new FileInfo(ProgramPath);
                try
                {
                    var program = Newtonsoft.Json.JsonConvert.DeserializeObject<ProgramDto>(File.ReadAllText(file.FullName));
                    program.Validate();
                    console.WriteLine($"Program \"{file.Name}\" was validated successfully");
                    return 0;
                }
                catch(Newtonsoft.Json.JsonSerializationException nex)
                {
                    console.WriteLine($"Failed to deserialize \"{file.Name}\"");
                    if (Verbose) DumpException(nex, console);
                }
                catch (ezvm.Core.Programming.DTO.ValidationException vex)
                {
                    console.WriteLine($"Validation failed for \"{file.Name}\"");
                    if (Verbose) DumpException(vex, console);
                }
                catch (Exception ex)
                {
                    console.WriteLine($"General exception during validation of \"{file.Name}\"");
                    if (Verbose) DumpException(ex, console);
                }
                return 1;
            }
        }

        [Command("run", Description = "Run a program")]
        private class Run : ProgramBasedCommand
        {
            [Option(Description = "Run VM until it terminates")]
            public bool Automated { get; } = false;

            [Option(Description = "Ignore execution errors")]
            public bool IgnoreErrors { get; } = false;

            [Option(Description = "Validate program before execution", ShortName = "validate")]
            public bool Validate { get; } = false;

            [Option(Description = "Verbose output")]
            public bool Verbose { get; } = false;

            [Option(Description = "Dump debug information after each step")]
            public bool Debug { get; } = false;

            [Option(CommandOptionType.SingleOrNoValue, Description ="Arguments to pass to VM", ShortName = "args")]
            public string VMArgument { get; set; }

            private int OnExecute(IConsole console)
            {
                var file = new FileInfo(ProgramPath);
                try
                {
                    var program = Newtonsoft.Json.JsonConvert.DeserializeObject<ProgramDto>(File.ReadAllText(file.FullName));
                    if (Validate)
                        program.Validate();

                    if (!Automated && string.IsNullOrEmpty(VMArgument))
                        VMArgument = Prompt.GetString("You did not specify input for the VM, you can do so now: ", "");

                    var vm = new VirtualMachine(program, VMArgument ?? "");
                    vm.OnError += (sender, args) => {
                        if (Verbose) DumpException(args.Exception, console);
                    };

                    while (true)
                    {
                        if (!Automated) Prompt.GetString("Press ENTER to continue execution");

                        vm.Step(IgnoreErrors);

                        if (Debug) console.WriteLine(vm.Stack.Dump());

                        if (vm.State == VirtualMachine.VMState.Exited)
                        {
                            if (Verbose) console.WriteLine("VM exited");
                            break;
                        }
                        else if (!IgnoreErrors && vm.State == VirtualMachine.VMState.Errored)
                        {
                            if (Verbose) console.WriteLine("VM is in error state; aborting VM execution");
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    console.WriteLine($"General exception during execution of \"{file.Name}\"");
                    DumpException(ex, console);
                }
                return 0;
            }
        }
    }
}
