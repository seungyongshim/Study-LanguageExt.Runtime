using System;
using System.Threading;
using KafkaConsumerRuntime.Traits;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace KafkaConsumerRuntime
{
    public readonly struct MyRuntime : HasCancel<MyRuntime>,
                                       HasKafkaConsumer<MyRuntime>
    {
        public MyRuntime LocalCancel => throw new NotImplementedException();

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource => new();

        public Eff<MyRuntime, Traits.KafkaConsumerIO> KafkaConsumerEff => throw new NotImplementedException();
    }
}
