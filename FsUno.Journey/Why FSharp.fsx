﻿(**
# Why F# ?

*tldr; I wanted to implement Functional Event Sourcing, and needed a functional language for this.*
*F#'s DSL oriented syntax proved useful on the way.*

## Event Sourcing

Back in 2008, this thing called event sourcing started to draw my attention as
I started to feel the limits of doing Domain Driven Design using an ORM.

Discussion after discussion on the [DDD yahoo group](http://groups.yahoo.com/group/domaindrivendesign)
with Greg Young and others; things started to make sense. But a lot of points remained
a bit fuzzy. 

Taking Greg's course in Paris made all this a lot clearer, but still I was not
totally comfortable with the implementation. I put a first attempt in production
and it's still running, but I was not totally OK with the result.

Then I met Rinat Abdullin, and we created [#cqrsbeers](https://twitter.com/hashtag/cqrsbeers) in Paris.
Rinat had a wonderfuly efficient implementation of [Event Sourcing in C# locally or on Azure](https://github.com/Lokad/lokad-cqrs)
open sourced from the company he was working in at that time.

It was lean and mean, very inspiring, in production; and yet it was not enough.

## Functional Event Sourcing

Then at Skillsmatter's DDDx 2013, Greg did a presentation about [DDD Functional Programming](https://skillsmatter.com/skillscasts/3191-ddd-functional-programming).
I was starting to learn F# at that time, and here things matched perfectly.

Functional event sourcing is defined around two functions:

The first one is in charge of deciding what's happening:
*)
(*** hide ***)
type State = { MyCurrentState: string }
    with static member empty = { MyCurrentState = ""}
(** *)

type Command = | DoSomething 
type Event = | SomethingHappened

let decide state command =
    match command with
    | DoSomething ->
        if state.MyCurrentState = "..." then ()
            // take some decision based on the command and
            // the current state and return events that should
            // result in this case
        [ SomethingHappened ] 

(** Simple enough, and with immutable values, decide is a pure function without
side effects.

Its signature indicates it's meaning:

    State -> Command -> Event list

can be read as **given my current state and what you ask me to do, here is what happens**.

This is something that had always bugged me with OO Event Sourcing; you have to
be careful not to mutate your state before building your events. But nothing
prevents you from doing so and shooting yourself in the foot.

Then comes the event application function.
*)

let apply state event =
    match event with
    | SomethingHappened ->
        { state with MyCurrentState = "something derived from event content" }

(** Here again, a pure function without side effects.

The signature is :

    State -> Event -> State

and can be read as **given my current state and what happened, here is my new state**

No risk of corruption of internal state if something bad happens during processing; all is
immutable.

And rebuilding the current state is as easy as *)

let replay events = List.fold apply State.empty events

(**
## F#

So I started to try implementing [Greg's SimpleCQRS](https://github.com/gregoryyoung/m-r) sample [in F#](https://github.com/thinkbeforecoding/m-r)
and could see that much of the pain went away, even if I was still a noob in F#.

A central aspect of F# is that it has a type system that eerily seems almost purpose-built for DDD.
Have a look at Scott Wlaschin's presentation on [DDD with F#](http://fr.slideshare.net/ScottWlaschin/ddd-with-fsharptypesystemlondonndc2013) 
and [the associated series on his blog](http://fsharpforfunandprofit.com/posts/designing-with-types-more-semantic-types/#series-toc).

Another one is that F# has immutable and [Non Null](http://www.infoq.com/presentations/Null-References-The-Billion-Dollar-Mistake-Tony-Hoare) types by default.

It also has a [DSL](http://martinfowler.com/books/dsl.html)ly feeling due to the low noise -- no curlies - and currying.

It's also a language that can be put into production on many platforms thanks to Xamarin.

All this combined together makes it a good way to implement Event Sourcing. It's robust, clean and fast.
It's also a great way to teach Event Sourcing, even to people who don't speak F#.

I've done several presentations in F# in front of Java people - *"You don't
talk C# and I don't talk Java, let's do this in F# !"* - and it was totally ok.

This being said, most of the things here can be applied outside of F# with possibly
minor adjustments. If you find any, let's chat, and I can talk about it here.
*)