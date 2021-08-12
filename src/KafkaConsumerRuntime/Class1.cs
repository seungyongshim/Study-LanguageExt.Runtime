using System;
using System.Linq;
using System.Diagnostics;
using Confluent.Kafka;
using KafkaConsumerRuntime.Traits;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;
using Error = LanguageExt.Common.Error;

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
            from msg in Eff<RT, ConsumeResult<string, string>>(rt => consumer.Consume(rt.CancellationToken))
            from __1 in guardnot(msg is null, Error.New(0, "ConsumeResult is null"))
            from __2 in guardnot(msg.IsPartitionEOF, Error.New(0, "ConsumeResult is PartitionEOF"))
            select msg;

        public static Eff<RT, Activity> activity(ConsumeResult<string, string> msg) =>
            Eff<RT, Activity>(rt => new ActivitySource("Aums").StartActivity("kafka"));

        public static Eff<RT, object> parse(ConsumeResult<string, string> msg) =>
            Eff<RT, object>(rt => msg.Message.Value);

        public static Eff<RT, Unit> commit(ConsumeResult<string, string> msg) =>
            Eff<RT, Unit>(rt => unit);
        public static Eff<RT, Unit> subscribe(IConsumer<string, string> consumer, string topic) => 
            Eff<RT, Unit>(rt => { consumer.Subscribe(topic); return unit; });
    }

    public static class Controller<RT> where
        RT : struct, HasKafkaConsumer<RT>, HasCancel<RT>
    {
        public static Aff<RT, Unit> SendMail(ConsumerConfig config, Func<object, Aff<Unit>> func) =>
            use(KafkaConsumer<RT>.build(config), c =>
                from s in KafkaConsumer<RT>.subscribe(c, "aums.send-mail").ToAff()
                from _ in repeat((from m  in KafkaConsumer<RT>.consume(c)
                                  from _1 in use(KafkaConsumer<RT>.activity(m), a =>
                                                 from r in KafkaConsumer<RT>.parse(m).ToAff()
                                                 from _1 in func(r)
                                                 from _2 in KafkaConsumer<RT>.commit(m).ToAff()
                                                 select unit) 
                                  select unit)|@catch(0, unit))
                select unit);
    }
}
