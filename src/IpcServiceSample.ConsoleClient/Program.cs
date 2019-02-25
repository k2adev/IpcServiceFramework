﻿using IpcServiceSample.ServiceContracts;
using K2adev.IpcServiceFramework;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Press any key to start.");
            Console.ReadKey();
            try
            {
                Console.WriteLine("Press any key to break.");
                var source = new CancellationTokenSource();

                await Task.WhenAll(RunTestsAsync(source.Token), Task.Run(() =>
                {
                    Console.ReadKey();
                    Console.WriteLine("Cancelling...");
                    source.Cancel();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunTestsAsync(CancellationToken cancellationToken)
        {
            IpcServiceClient<IComputingService> computingClient = new IpcServiceClientBuilder<IComputingService>()
                .UseNamedPipe("pipeName", new IpcServiceOptions() { GZipCompressionEnabled = true, Aes256EncryptionEnabled = true })
                .Build();

            
            IpcServiceClient<ISystemService> systemClient = new IpcServiceClientBuilder<ISystemService>()
                .UseTcp(new IpcServiceOptions() { GZipCompressionEnabled = true, Aes256EncryptionEnabled = true }, IPAddress.Loopback, 45684)
                .Build();
                
            // test 1: call IPC service method with primitive types
            float result1 = await computingClient.InvokeAsync(x => x.AddFloat(1.23f, 4.56f), cancellationToken);
            Console.WriteLine($"[TEST 1] sum of 2 floating number is: {result1}");

           // Console.ReadLine();

            // test 2: call IPC service method with complex types
            ComplexNumber result2 = await computingClient.InvokeAsync(x => x.AddComplexNumber(
                new ComplexNumber(0.1f, 0.3f),
                new ComplexNumber(0.2f, 0.6f)), cancellationToken);
            Console.WriteLine($"[TEST 2] sum of 2 complexe number is: {result2.A}+{result2.B}i");

            // test 3: call IPC service method with an array of complex types
            ComplexNumber result3 = await computingClient.InvokeAsync(x => x.AddComplexNumbers(new[]
            {
                new ComplexNumber(0.5f, 0.4f),
                new ComplexNumber(0.2f, 0.1f),
                new ComplexNumber(0.3f, 0.5f),
            }), cancellationToken);
            Console.WriteLine($"[TEST 3] sum of 3 complexe number is: {result3.A}+{result3.B}i", cancellationToken);
            
            // test 4: call IPC service method without parameter or return
            await systemClient.InvokeAsync(x => x.DoNothing(), cancellationToken);
            Console.WriteLine($"[TEST 4] invoked DoNothing()");

            // test 5: call IPC service method with enum parameter
            string text = await systemClient.InvokeAsync(x => x.ConvertText("hEllO woRd!", TextStyle.Upper), cancellationToken);
            Console.WriteLine($"[TEST 5] {text}");

            // test 6: call IPC service method returning GUID
            Guid generatedId = await systemClient.InvokeAsync(x => x.GenerateId(), cancellationToken);
            Console.WriteLine($"[TEST 6] generated ID is: {generatedId}");

            // test 7: call IPC service method with byte array
            byte[] input = Encoding.UTF8.GetBytes("Test");
            byte[] reversed = await systemClient.InvokeAsync(x => x.ReverseBytes(input), cancellationToken);
            Console.WriteLine($"[TEST 7] reversed bytes are: {Convert.ToBase64String(reversed)}");

            // test 8: call IPC service method with generic parameter
            //string print = await systemClient.InvokeAsync(x => x.Printout(DateTime.UtcNow), cancellationToken);
            //Console.WriteLine($"[TEST 8] print out value: {print}");
            Console.WriteLine($"[TEST 8] generic parameter <T> not supported yet");

            // test 9: call slow IPC service method 
            await systemClient.InvokeAsync(x => x.SlowOperation(), cancellationToken);
            Console.WriteLine($"[TEST 9] Called slow operation");
            
            // test 10: call async server method
            await computingClient.InvokeAsync(x => x.MethodAsync());
            Console.WriteLine($"[TEST 10] Called async method");

            // test 11: call async server function
            int sum = await computingClient.InvokeAsync(x => x.SumAsync(1, 1));
            Console.WriteLine($"[TEST 11] Called async function: {sum}");

            Console.WriteLine("[TESTS] Done.");
            return;
        }
    }
}
