using ezvm.Assembler.Builders;
using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ezvm.Assembler
{
    class Program
    {
        private static Random RNG = new Random(1337);
        private const long KEY_SIZE = 256;
        private static IEnumerator<string> nameGenerator = new CharCombinator("!Il|").GetStrings().GetEnumerator();
        static void Main(string[] args)
        {
            var programB = new ProgramBuilder("n07-50-345y");
            var xorBuffers = (xorBuffers)(new xorBuffers().Rename(nameGenerator));
            var b64dec = (b64dec)(new b64dec().Rename(nameGenerator));
            var validateBuffer = (validateBuffer)(new validateBuffer().Rename(nameGenerator));
            //Main
            {
                programB.AddMain()
                        .AddLocals("secret", "key", "keyLen")
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@"        _______ _________       ._______________         ________     _____  .________        ").SetAppendNewLine().Close()
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@"  ____  \   _  \\______  \      |   ____/\   _  \        \_____  \   /  |  | |   ____/___.__. ").SetAppendNewLine().Close()
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@" /    \ /  /_\  \   /    /______|____  \ /  /_\  \  ______ _(__  <  /   |  |_|____  \<   |  | ").SetAppendNewLine().Close()
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@"|   |  \\  \_/   \ /    //_____//       \\  \_/   \/_____//       \/    ^   //       \\___  | ").SetAppendNewLine().Close()
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@"|___|  / \_____  //____/       /______  / \_____  /      /______  /\____   |/______  // ____| ").SetAppendNewLine().Close()
                        .AddJunkSpin(100000, nameGenerator, RNG)
                        .OpPrint(PrintType.Char).SetConstant(@"     \/        \/                     \/        \/              \/      |__|       \/ \/    ").SetAppendNewLine().Close()
                        .AddJunkSpin(1000000, nameGenerator, RNG)

                        .OpJumpIfAeqB("cmp", ProgramBuilder.InputBufferLength, 0, "err_no_key", "proc")
                        .AddLabel("proc")

                        //Calculate b64 decoded key size
                        .OpArithmetic(ArithmeticOperator.Multiply, "keyLen", ProgramBuilder.InputBufferLength, 4)
                        .OpArithmetic(ArithmeticOperator.Divide, "keyLen", "keyLen", 2)
                        .OpIncrement("keyLen")
                        .OpBufferAllocate("key").SetLength().SetVariable("keyLen").Close().Close()
                        //Load secret
                        .OpPush(PushType.ByRef, "dstBuffId").SetValue().SetVariable("secret").Close().Close()
                        .OpCall("generate_flag_bytes")
                        //Reset keyLen
                        .OpSetVariable("keyLen", 0)
                        //b64 decode key: b64_dec(srcBufferId, srcBufferOffset, srcBufferLen, dstBufferId, dstBufferOffset)
                        .OpPrint(PrintType.Char).SetConstant("[>] Decoding <").Close()
                        .OpPush(PushType.ByVal, b64dec.srcBufferId).SetValue().SetVariable(ProgramBuilder.InputBufferId).Close().Close()
                        .OpPush(PushType.ByVal, b64dec.srcBufferOffset).SetValue().SetConstant(0).Close().Close()
                        .OpPush(PushType.ByVal, b64dec.srcBufferLen).SetValue().SetVariable(ProgramBuilder.InputBufferLength).Close().Close()
                        .OpPush(PushType.ByVal, b64dec.dstBufferId).SetValue().SetVariable("key").Close().Close()
                        .OpPush(PushType.ByRef, b64dec.dstBufferOffset).SetValue().SetVariable("keyLen").Close().Close()
                        .OpCall("b64_dec")
                        .OpPrint(PrintType.Char).SetConstant(">").SetAppendNewLine().Close()
                        //secret ^ key
                        .OpPrint(PrintType.Char).SetConstant("[>] Decrypting <").Close()
                        .AddJunkSpin(1000000, nameGenerator, RNG)
                        .OpPush(PushType.ByVal, xorBuffers.srcBufferId).SetValue().SetVariable("secret").Close().Close()
                        .OpPush(PushType.ByVal, xorBuffers.keyBufferId).SetValue().SetVariable("key").Close().Close()
                        .OpPush(PushType.ByVal, xorBuffers.keyBufferLen).SetValue().SetVariable("keyLen").Close().Close()
                        .OpCall("xorBuffers")
                        .OpPrint(PrintType.Char).SetConstant(">").SetAppendNewLine().Close()
                        //validate
                        .OpPrint(PrintType.Char).SetConstant("[>] Validating <").Close()
                        .OpPush(PushType.ByVal, validateBuffer.srcBufferId).SetValue().SetVariable("secret").Close().Close()
                        .OpPush(PushType.ByRef, validateBuffer.result).SetValue().SetVariable("key").Close().Close()
                        .OpCall("validateBuffer")
                        .OpPrint(PrintType.Char).SetConstant(">").SetAppendNewLine().Close()
                        //key == 1?
                        .OpJumpIfAeqB("key", "key", 1, "print", "err_wrong_key")

                        .AddLabel("print")
                        .OpPrint(PrintType.Char).SetConstant("[#] The flag: ").Close()
                        .OpPrint(PrintType.Char).SetBufferString("secret").SetBufferOffset(256).SetAppendNewLine().Close()
                        .OpJump("end")

                        .AddLabel("err_wrong_key")
                        .OpPrint(PrintType.Char).SetConstant("[!] Wrong key supplied").SetAppendNewLine().Close()
                        .OpJump("end")
                        
                        .AddLabel("err_no_key")
                        .OpPrint(PrintType.Char).SetConstant("[!] No key supplied: pass the keyphrase as an argument.").SetAppendNewLine().Close()

                        .AddLabel("end")
                        .OpReturn()
                        .Close();
            }
            //xorBuffers(srcBufferId, keyBufferId, keyLen)
            {
                programB.AddMethod("xorBuffers")
                    .AddParameters(xorBuffers.srcBufferId, xorBuffers.keyBufferId, xorBuffers.keyBufferLen)
                    .AddLocals("i", "cnt", "tA", "tB", "srcLen")
                    // Get buffer length
                    .OpBufferGetLength("srcLen").SetId().SetVariable(xorBuffers.srcBufferId).Close().Close()
                    
                    .OpJumpIfAeqB("cmp", "srcLen", 0, "src_zero", "proc1")
                    .AddLabel("proc1")
                    .OpJumpIfAeqB("cmp", xorBuffers.keyBufferLen, 0, "key_zero", "proc2")
                    .AddLabel("proc2")

                    // cnt = srcLen
                    .OpSetVariable("cnt","srcLen")
                    // cnt = keyLen * ITERATIONS
                    //.OpArithmetic(ArithmeticOperator.Multiply, "cnt", xorBuffers.keyBufferLen, ITERATIONS)

                    .AddLabel("loop")
                    .AddJunkSpin(50000, nameGenerator, RNG)
                    .AddJunkOps(RNG.Next(5, 10), nameGenerator, RNG)
                    // Print update
                    .OpModulo("cmp", "i", 10)
                    .OpJumpIfAeqB("cmp", "cmp", 0, "printUpdate", "skipUpdate")
                    .AddLabel("printUpdate")
                    .OpPrint(PrintType.Char).SetConstant(".").Close()
                    .AddLabel("skipUpdate")
                    // i < cnt?
                    .OpJumpIfAltB("cmp", "i", "cnt", "logic", "end")
                    .AddLabel("logic")
                    // tA = i % srcLen
                    .OpModulo("tA", "i", "srcLen")
                    // tA = srcBuff[i]
                    .OpBufferRead("tA", xorBuffers.srcBufferId, "tA")
                    // tB = i % keyLen
                    .OpModulo("tB", "i", xorBuffers.keyBufferLen)
                    // tB = keyBuff[i]
                    .OpBufferRead("tB", xorBuffers.keyBufferId, "tB")
                    // tB = tA ^ tB
                    .OpArithmetic(ArithmeticOperator.Xor, "tB", "tA", "tB")
                    // tA = i % srcLen
                    .OpModulo("tA", "i", "srcLen")
                    // srcBuff[i] = tA
                    .OpBufferWrite("tB", xorBuffers.srcBufferId, "tA")
                    // i++
                    .OpIncrement("i")
                    .OpJump("loop")

                    .AddLabel("src_zero")
                    .OpPrint(PrintType.Char)
                        .SetConstant("Somehow the src is empty, lol.")
                        .SetAppendNewLine()
                        .Close()
                    .OpJump("end")
                    .AddLabel("key_zero")
                    .OpPrint(PrintType.Char)
                        .SetConstant("You must specify an input!")
                        .SetAppendNewLine()
                        .Close()
                    .AddLabel("end")
                    .OpReturn()
                    .Close();
            }
            //validateBuffer(srcBufferId, &result)
            {
                programB.AddMethod("validateBuffer")
                    .AddParameters(validateBuffer.srcBufferId, validateBuffer.result)
                    .AddLocals("v", "i", "cmp")

                    //New: check if first 256 bytes are 0...255 => leaks key easily
                    .OpSetVariable(validateBuffer.result, 1)
                    .AddLabel("loop")
                    .AddJunkSpin(50000, nameGenerator, RNG)
                    .AddJunkOps(RNG.Next(5, 10), nameGenerator, RNG)
                    // Print update
                    .OpModulo("cmp", "i", 10)
                    .OpJumpIfAeqB("cmp", "cmp", 0, "printUpdate", "skipUpdate")
                    .AddLabel("printUpdate")
                    .OpPrint(PrintType.Char).SetConstant(".").Close()
                    .AddLabel("skipUpdate")

                    .OpJumpCmp("cmp", Comparator.Lesser, "i", 256, "proc1", "end")
                    .AddLabel("proc1")
                    .OpBufferRead("v", validateBuffer.srcBufferId, "i")
                    .OpJumpCmp("cmp", Comparator.Equal, "v", "i", "proc2", "err")
                    .AddLabel("proc2")
                    .OpIncrement("i")
                    .OpJump("loop")

                    .AddJunkSpin(1000000, nameGenerator, RNG)
                    .AddJunkOps(RNG.Next(5, 10), nameGenerator, RNG)
                    .AddLabel("err")
                    .OpSetVariable(validateBuffer.result, 0)

                    .AddJunkOps(RNG.Next(50, 100), nameGenerator, RNG)
                    .AddLabel("end")
                    .OpReturn()
                    .Close();
            }
            //b64_charToByte(&char)
            {
                var meth = programB.AddMethod("b64_charToByte")
                    .AddParameters("char");
                var map = meth.OpMapValue("char")
                    .SetSource().SetVariable("char").Close()
                    .SetDefault().SetConstant(0).Close();
                // 'A' -> 0 
                foreach (var c in Enumerable.Range(0, 26)) map.AddKeyValue('A' + c).SetValue().SetConstant(c).Close().Close();
                foreach (var c in Enumerable.Range(0, 26)) map.AddKeyValue('a' + c).SetValue().SetConstant(c + 26).Close().Close();
                foreach (var c in Enumerable.Range(0, 10)) map.AddKeyValue('0' + c).SetValue().SetConstant(c + 52).Close().Close();
                map.AddKeyValue('+').SetValue().SetConstant(62).Close().Close();
                map.AddKeyValue('/').SetValue().SetConstant(63).Close().Close();
                map.Close();
                meth.OpReturn().Close();
            }
            //b64_dec(srcBufferId, srcBufferOffset, srcBufferLen, dstBufferId, &dstBufferOffset)
            {
                programB.AddMethod("b64_dec")
                    .AddParameters(b64dec.srcBufferId, b64dec.srcBufferOffset, b64dec.srcBufferLen, b64dec.dstBufferId, b64dec.dstBufferOffset)
                    .AddLocals("c", "i", "k", "k0", "k1", "k2", "k3")
                    .OpBufferGetLength("cmp").SetId().SetVariable(b64dec.srcBufferId).Close().Close()
                    .OpModulo("cmp", "cmp", 4)
                    .OpJumpIfAeqB("cmp", "cmp", 0, "loop", "end")
                    // increment over the length of the string, four characters at a time
                    .AddLabel("loop")
                    .AddJunkSpin(50000, nameGenerator, RNG)
                    .AddJunkOps(RNG.Next(5, 10), nameGenerator, RNG)
                    // Print update
                    .OpPrint(PrintType.Char).SetConstant(".").Close()
                    //.OpPrint(PrintType.IntDec).SetVariable("i").SetAppendNewLine().Close()
                    .OpJumpIfAltB("cmp", "i", b64dec.srcBufferLen, "body", "end")
                    .AddLabel("body")
                    .AddLocals("it")
                    // Retrieve 4 chars
                    .OpSetVariable("it", "i")
                    .OpBufferRead("k0", b64dec.srcBufferId, "it")
                    //.OpCompare("cmp", Core.Programming.DTO.Operations.Comparator.Equal)
                    //    .SetOperatorA().SetVariable("k0").Close()
                    //    .SetOperatorB().SetConstant(0).Close()
                    //    .Close()
                    //.OpIf("cmp")
                    //    .SetMarkerTrue("proc2")
                    //    .SetMarkerFalse("end")
                    //    .Close()
                    //.AddLabel("proc2")
                    .OpIncrement("it")
                    .OpBufferRead("k1", b64dec.srcBufferId, "it")
                    .OpIncrement("it")
                    .OpBufferRead("k2", b64dec.srcBufferId, "it")
                    .OpIncrement("it")
                    .OpBufferRead("k3", b64dec.srcBufferId, "it")
                    //Translate k
                    .OpPush(PushType.ByRef, "char").SetValue().SetVariable("k0").Close().Close()
                    .OpCall("b64_charToByte")
                    .OpPush(PushType.ByRef, "char").SetValue().SetVariable("k1").Close().Close()
                    .OpCall("b64_charToByte")
                    .OpPush(PushType.ByRef, "char").SetValue().SetVariable("k2").Close().Close()
                    .OpCall("b64_charToByte")
                    .OpPush(PushType.ByRef, "char").SetValue().SetVariable("k3").Close().Close()
                    .OpCall("b64_charToByte")
                    // Construct k
                    .OpSetVariable("k", "k0") // k = k0
                    .OpArithmetic(ArithmeticOperator.ShiftLeft, "k", "k", 6) // k << 6
                    .OpArithmetic(ArithmeticOperator.Or, "k", "k", "k1") // k |= k1
                    .OpArithmetic(ArithmeticOperator.ShiftLeft, "k", "k", 6) // k << 6
                    .OpArithmetic(ArithmeticOperator.Or, "k", "k", "k2") // k |= k2
                    .OpArithmetic(ArithmeticOperator.ShiftLeft, "k", "k", 6) // k << 6
                    .OpArithmetic(ArithmeticOperator.Or, "k", "k", "k3") // k |= k3
                                                                         // Deconstruct k
                    .OpArithmetic(ArithmeticOperator.And, "k2", "k", 0xff) //k0 = k & FF
                    .OpArithmetic(ArithmeticOperator.ShiftRight, "k", "k", 8) //k >> 8
                    .OpArithmetic(ArithmeticOperator.And, "k1", "k", 0xff) //k1 = k & FF
                    .OpArithmetic(ArithmeticOperator.ShiftRight, "k", "k", 8) //k >> 8
                    .OpArithmetic(ArithmeticOperator.And, "k0", "k", 0xff) //k2 = k & FF
                                                                           //Write k
                    .OpBufferWrite("k0", b64dec.dstBufferId, b64dec.dstBufferOffset)
                    .OpIncrement(b64dec.dstBufferOffset)
                    .OpBufferWrite("k1", b64dec.dstBufferId, b64dec.dstBufferOffset)
                    .OpIncrement(b64dec.dstBufferOffset)
                    .OpBufferWrite("k2", b64dec.dstBufferId, b64dec.dstBufferOffset)
                    .OpIncrement(b64dec.dstBufferOffset)
                    .OpAdd("i", "i", 4) // i+=4
                    .OpJump("loop")

                    .AddLabel("end")
                    .OpDecrement(b64dec.dstBufferOffset)
                    .OpDecrement(b64dec.dstBufferOffset)
                    .OpReturn()
                    .Close();
            }
            
            //Generate secret
            {
                var checkbytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
                var flag_content = "ιsη'τ ιτ sτяαηgε, το ςяεατε sοмετнιηg τнατ нατεs λομ? - εχ мαςнιηα (αℓsο zατ, ςοδιηg τнιs ςнαℓℓεηgε)";
                var flag = $"CSR{{{Convert.ToBase64String(Encoding.UTF8.GetBytes(flag_content))}}}";
                var key = new byte[KEY_SIZE];
                RNG.NextBytes(key);

                var flag_bytes = checkbytes.Concat(Encoding.ASCII.GetBytes(flag)).ToArray();
                
                for (int i = 0; i< flag_bytes.Length; i++)
                    flag_bytes[i % flag_bytes.Length] = (byte)(flag_bytes[i % flag_bytes.Length] ^ key[i % key.Length]);

                programB.AddObfuscatedBufferLoadingMethod(flag_bytes, "generate_flag_bytes", RNG);
                Console.WriteLine(flag);
                Console.WriteLine(Convert.ToBase64String(key));
            }
            var program = programB.Item;
#if !DEBUG
            Obfuscator.Obfuscate(program, nameGenerator, RNG);
#endif
            program.Validate();
            var json = JsonConvert.SerializeObject(program, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Console.WriteLine(json);
            File.WriteAllText("program.json", json);

            var program2 = JsonConvert.DeserializeObject<Core.Programming.DTO.ProgramDto>(json);

            //var arg = "aAD0x0NeAqrCfE/ZJ/wiJ2qJVLLlEu8emvcifvA24yg=";
            var arg = "lFkuZ3PTkHZO3C+i31redvnXZaUuv0W2rIv2d3dZKOSXLdedRqgo/cJ1cRHNie9f6xfef5QfuNUTeLvn3Bj+9bNLCrKCQC9QrmjjuU406CKcl4xfeqlPd3QCis3tQSCpdnxegn493KrnjtAsO/6G34+ZSkWMn3oJ+5M9gLZhY21HOkPS1WajX7rheOzZIjx2fKo+U/baHNcBVIbr/1z0hSpi5AG7swkp9euKqiaOMNlXxwMLIw93bB1s0YHgt19e7fLBdj46usihFV4KUHKwciOslDhY6WEb6nfjT+KHSD2LK7w0zbqbHAw7aQ6xbRyqu15dMyMmBgSRpYvGVmvlGQ==";
            var vm = new VirtualMachine(program2, arg);
            vm.OnError += Vm_OnError;
            vm.OnExit += (s, e) => Console.WriteLine("[VM] Exited");
            while (vm.State == VirtualMachine.VMState.Ok)
                vm.Step();

            Console.WriteLine();
        }

        private static void Vm_OnError(object sender, Core.VM.ErrorEventArgs e)
        {
            Console.WriteLine($"VM encountered an error during execution: {e.Exception.Message}");
            Console.WriteLine("StackTrace:");
            Console.WriteLine(((VirtualMachine)sender).Stack.Dump());
        }
    }
}
