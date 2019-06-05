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

## Replaying Contexts Events
In most situations you don't want to replay all events when aggregating some information from the ever changing event stream. You want to focus on subsets of events which I call *context events*.

Context events are a list of events relevant in - well - a certain context, e.g. handling a command or a query. To replay just the context events you need you can filter the event stream in two ways:

### Selection by Event Type
In a context there might be only a couple of event types relevant. Just pass them to `Replay()` and you don't get to see all events, but just those matching the event types:

Assume you have recorded these events:

```
es.Record(new Event[]{new A(), new B(), new A(), new C(), new B(), new A()});
```

Then you can replay the events for a context concerned only with events `A` and `C` like this:

```
es.Replay(typeof(A), typeof(C))
```

which will result in a subset of 4 events (A,A,C,A). Of course the original order of events is retained.

### Selection by Event Number
Or you want to replay all events - but starting from a certain event, not from the beginning of the event stream. Maybe you know that a certain aggregation already assimilated events 0..456. To update it you'd only need the events from 457 on:

```
es.Replay(457);
```

Events are numbered in the order the are recorded starting with 0.

Of course you can combine event types and event number when replaying, e.g. `es.Replay(5, typeof(B), typeof(C))`.

## Optimistic Concurrency
The event store implementations of NSimpleEventstore are thread safe. That means you can use them from multiple threads consurrently. Events recorded by thread 1 will first be written to disk before events from thread 2 will be written. Events from different threads never interleave.

Still, though, the result might be unexpected if the events threads 2 produces depend on the overall content of the event stream. And if thread 2 was working on stale data since thread 1 has appended more events in the meantime... then an inconsistency could result.

To avoid this the event store implementations allow for optimistic concurrency: The event store is versioned. Whenever its state changes (i.e. new events get recorded) the version changes, too. That's why replaying events does not just deliver a list of events, but an object with an `Events` property. This object also carries the current version of the event store:

```
es.Replay().Version
```

The version is an opaque string. Don't look at it, to assume it to be of any special format or content. There is not even an order in how version numbers get created. Its only property you can rely on is that it changes whenever the event stream changes.

At any time you can query the event store for its version number (and the number of the last event recorded):

```
var state = es.State;
Console.WriteLine($"{state.Version}, {state.FinalEventNumber}");
```

But you also get the version number in the result of `Replay` (see above) and also from `Record()` in the same manner as from the `State` property:

```
var state = es.Record(new A());
Console.WriteLine($"{state.Version}, {state.FinalEventNumber}");
```

One you have a version number you can enforce optimistic concurrency. "Optimistic" means you're optimistic that recording will work because the version of the event stream hasn't changed since you last looked. For that you pass in the version number you know:

```
var state = es.Record(new A());
...
es.Record(new B(), state.Version);
```

If no other thread has written to the event stream then the second call to `Record` will work just fine.

To what happens otherwise let's provoke a situation where optimism will be disappointed.

```
var state = es.Record(new A());

es.Record(new C());

es.Record(new B(), state.Version); // uses outdated version number
```

Recording event `C` changed the version number of the event stream. Recording `B` thus has an wrong expectation and fails with a `VersionNotFoundException`. Bummer.

What to do in such situations is up to you. Maybe re-replaying a context stream, generating some events anew, and trying to record them will do. Maybe some more severe measures have to be taken.
