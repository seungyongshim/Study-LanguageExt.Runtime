using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using LanguageExt;
using static LanguageExt.Prelude;

namespace KafkaConsumerRuntime
{
    internal class Program
    {
        private static Task Main(string[] args) =>
            from x in Controller<MyRuntime>.SendMail(new ConsumerConfig(new Dictionary<string, string>()
            {
                ["bootstrap.servers"] = "localhost:9092",
                ["group.id"] = "bcw",
                ["enable.auto.commit"] = "false",
                ["auto.offset.reset"] = "earliest",
                ["acks"] = "-1",
            }), x => Eff(() => { Console.Write(x); return unit; })).Run(new MyRuntime()).ToRef()
            let r = x.ThrowIfFail()
            select r;
    }
}
