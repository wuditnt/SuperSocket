using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Command;
using System.Reflection;

namespace Tests
{
    [Collection("Command")]
    public class CommandTest : TestBase
    {
        class StringPackageInfo : IKeyedPackageInfo<string>
        {
            public string Key { get; set; }

            public string[] Parameters { get; set; }
        }

        class ADD : IAsyncCommand<string, StringPackageInfo>
        {
            public string Key => "ADD";

            public string Name => Key;

            public async Task ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Sum();

                await session.Channel.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        class MULT : IAsyncCommand<string, StringPackageInfo>
        {
            public string Key => "MULT";

            public string Name => Key;

            public async Task ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x * y);

                await session.Channel.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        class SUB : IAsyncCommand<string, StringPackageInfo>
        {
            public string Key => "SUB";

            public string Name => Key;

            public async Task ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x - y);

                await session.Channel.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        class MyPipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
        {
            public MyPipelineFilter()
                : base(new[] { (byte)'\r', (byte)'\n' })
            {

            }

            protected override StringPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                var text = buffer.GetString(Encoding.UTF8);
                var parts = text.Split(' ');

                return new StringPackageInfo
                {
                    Key = parts[0],
                    Parameters = parts.Skip(1).ToArray()
                };
            }
        }

        public CommandTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Fact] 
        public async Task TestCommands()
        {
            var server = CreateSocketServerBuilder<StringPackageInfo, MyPipelineFilter>()
                .UseCommand<string, StringPackageInfo>(commandOptions =>
                {
                    // register commands one by one
                    commandOptions.AddCommand<ADD>();
                    commandOptions.AddCommand<MULT>();
                    commandOptions.AddCommand<SUB>();

                    // register all commands in one aassembly
                    //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
                }).BuildAsServer();

            Assert.Equal("TestServer", server.Name);

            Assert.True(await server.StartAsync());

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
            OutputHelper.WriteLine("Connected.");

            using (var stream = new NetworkStream(client))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024 * 1024 * 4))
            {
                await streamWriter.WriteAsync("ADD 1 2 3\r\n");
                await streamWriter.FlushAsync();
                var line = await streamReader.ReadLineAsync();
                Assert.Equal("6", line);

                await streamWriter.WriteAsync("MULT 2 5\r\n");
                await streamWriter.FlushAsync();
                line = await streamReader.ReadLineAsync();
                Assert.Equal("10", line);

                await streamWriter.WriteAsync("MULT 8 2\r\n");
                await streamWriter.FlushAsync();
                line = await streamReader.ReadLineAsync();
                Assert.Equal("6", line);
            }

            await server.StopAsync();
        }
    }
}
