using System;
using System.Diagnostics;
using System.Threading;
using Confluent.Kafka;
using KafkaConsumerRuntime.Traits;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace KafkaConsumerRuntime
{
    namespace Traits
    {
        public interface KafkaConsumerIO
        {
        }

        public interface HasKafkaConsumer<RT> where
            RT : struct, HasKafkaConsumer<RT>
        {
            Eff<RT, KafkaConsumerIO> KafkaConsumerEff { get; }
        }
    }

    public readonly struct KafkaConsumerIO : Traits.KafkaConsumerIO
    {
        public static readonly KafkaConsumerIO Default = new KafkaConsumerIO();
    }

    public static class KafkaConsumer<RT> where
        RT : struct, HasCancel<RT>, HasKafkaConsumer<RT>
    {
        public static Eff<RT, IConsumer<string, string>> build(ConsumerConfig conf) =>
            Eff<RT, IConsumer<string, string>>(r => new ConsumerBuilder<string, string>(conf).Build());

        public static Eff<RT, ConsumeResult<string, string>> consume(IConsumer<string, string> consumer) =>
            Eff<RT, ConsumeResult<string, string>>(rt => consumer.Consume(rt.CancellationToken));

        public static Eff<RT, Activity> activity(ConsumeResult<string, string> msg) =>
            Eff<RT, Activity>(rt => new ActivitySource("Aums", "").StartActivity("kafka"));

        public static Eff<RT, object> parse(ConsumeResult<string, string> msg) =>
            Eff<RT, object>(rt => "1");

        public static Eff<RT, Unit> commit(ConsumeResult<string, string> msg) =>
            Eff<RT, Unit>(rt => unit)

    }

    public readonly struct MyRuntime :
        HasCancel<MyRuntime>, HasKafkaConsumer<MyRuntime>
    {
        public MyRuntime LocalCancel => throw new NotImplementedException();

        public CancellationToken CancellationToken => throw new NotImplementedException();

        public CancellationTokenSource CancellationTokenSource => throw new NotImplementedException();

        public Eff<MyRuntime, Traits.KafkaConsumerIO> KafkaConsumerEff => throw new NotImplementedException();
    }

    public static class Controller<RT> where
        RT : struct, HasKafkaConsumer<RT>, HasCancel<RT>
    {
        public static Aff<RT, Unit> SendingMail(Func<object, Aff<Unit>> func) =>
            use(KafkaConsumer<RT>.build(null), c =>
                repeat(from m in KafkaConsumer<RT>.consume(c)
                       from x in use(KafkaConsumer<RT>.activity(m), c =>
                                     from r in KafkaConsumer<RT>.parse(m).ToAff()
                                     from _1 in func(r)
                                     from _2 in KafkaConsumer<RT>.commit(m).ToAff()
                                     select _2)
                       select x));
    }
}
