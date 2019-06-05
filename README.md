# NSimpleEventstore
A very simple event store for the unassuming .NET developer.

If you want to play around with Event Sourcing you can easily get scared by professional tools like [NEventStore](http://neventstore.org/) or Event Store (https://eventstore.org/). They are so powerful - but at the beginning that might be a drawback.

To just get a feeling for how persisting events differs from persisting big data models you don't want to install servers or dive deep into comprehensive frameworks.

Enter NSimlpeEventStore: It makes it trivial to record events, replay events, and even do that with simple consistency checks should you use it with several concurrent clients. Here's an example:

## First Steps

1. Create a .NET console project
2. Add the NSimpleEvenstore Nuget package

Now, define a first event. All event classes need to derive from `nsimpleeventstore.Event`:

```
using nsimpleeventstore;

class NumberEntered : Event
{
    public int Number;
}
```

Then create an instance of an event store. You can choose between the persistent event store `FilebasedEventstore` (storing each event in a file by itself) or the in-memory event store `InMemoryEventstore`. Both implement the interface `contract.IEventstore`.

```
IEventstore es = new FilebasedEventstore();
```

And once you have an event store object you can record events. Think of an event store as a tape recorder where you only append new recordings at the end:

```
es.Record(new NumberEntered {Number = 1});
es.Record(new[]{new NumberEntered {Number = 2}, new NumberEntered {Number = 39}});
```

You can record single events or collections of events (passed in as an array).

What got recorded can then be replayed at any time. (But no need to rewind the event stream.)

```
var events = es.Replay().Events;

foreach(var e in events)
    Console.WriteLine($"{((NumberEntered)e).Number}");
```

That's about it. Really. An event store is not that complicated. Hence it should not be complicated to use it. At least for your first experiments. The real challenge is in changing your mindset. And that should not be impeded by a technology. "Thinking in events" is quite different from "thinking in (single) data models".

## Replaying Contexts
In most situations you don't want to replay all events when aggregating some information from the ever changing event stream. You want to focus on subsets of events which I call *context events*.

Context events are a list of events relevant in - well - a certain context, e.g. handling a command or a query. To replay just the context events you need you can filter the event stream in two ways:

### Selection by Event Type
In a context there might be only a couple of event types relevant. Just pass them to `Replay()` and you don't get to see all events, but just those matching the event types:

Assume you have recorded these events:

```
es.Record(new Event[]{new A(), new B(), new A(), new C(), new B(), new A()});
```

Then you can replay the events for a context like this:

```
es.Replay(typeof(A), typeof(C))
```

which will result in a subset of 4 events (3xA, 1xC).

### Selection by Event Number


## Optimistic Concurrency
