﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using NeoSharp.Network.DI;
using NeoSharp.Logging.DI;
using SimpleInjector;
using Moq;
using Microsoft.Extensions.Logging;

namespace NeoSharp.Network.Benchmark
{       
    public class BM_PeerFactory
    {
        PeerFactory uub;

        [GlobalSetup]
        public void Setup()
        {
            Container cont = new Container();
            cont.Register<IPeer, Peer>(Lifestyle.Transient);
            cont.Register(ConfigureLogger, Lifestyle.Singleton);
            cont.Register(typeof(ILogger<>), typeof(LoggerAdapter<>));
            uub = new PeerFactory(cont);
        }

        private static ILoggerFactory ConfigureLogger()
        {
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            return mockLoggerFactory.Object;
        }

        [Benchmark]
        public void createPeers()
        {
            for (int i=0; i<10000; i++)
                uub.Create();
        }

    }
}
