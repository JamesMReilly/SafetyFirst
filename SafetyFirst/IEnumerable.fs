namespace SafetyFirst

open System
open System.Runtime.CompilerServices
open System.Collections.Generic
open System.Linq

open FSharpx
open ResultDotNet

open SafetyFirst.ErrorTypes

[<Extension>]
type IEnumerableExtensions () = 

  /// Applies an accumulator function over a sequence.
  [<Extension>]
  static member AggregateSafe (xs, f:Func<_,_,_>) = 
    Seq.reduce' (FSharpFunc.FromFunc f) xs
    |> Result.FromFs

  /// Computes the average of a sequence.
  [<Extension>]
  static member AverageSafe (xs:int seq) =
    Seq.averageBy' float xs
    |> Result.FromFs

  /// Computes the average of a sequence.
  [<Extension>]
  static member AverageSafe (xs:float seq) = 
    Seq.average' xs
    |> Result.FromFs

  /// Computes the average of a sequence of Double values that are obtained by invoking 
  /// a transform function on each element of the input sequence.
  [<Extension>]
  static member AverageSafe (xs, transform:Func<_,float>) =
    Seq.averageBy' (FSharpFunc.FromFunc transform) xs  
    |> Result.FromFs

  /// Returns the element at a specified index in a sequence.
  /// O(1) for any IList<T> or IReadOnlyList<T>.  
  /// O(n) where n is the <c>index</c> for any other IEnumerable<T>.
  [<Extension>]
  static member ElementAtSafe (xs:_ seq, index) = 
    if index < 0
      then Error <| indexNegativeErr index
    else 
      let count = 
        match xs with
        | :? ICollection<obj> as xs -> Some xs.Count 
        | :? IReadOnlyCollection<obj> as xs -> Some xs.Count
        | _ -> None

      match count with
      | Some c when c <= index -> Error <| indexTooLargeErr index c
      | Some c -> 
        match xs with
        | :? IList<_> as xs -> Ok <| xs.[index]  
        | :? IReadOnlyList<_> as xs -> Ok <| xs.[index]
        | _ -> Ok <| Seq.item index xs
      | None -> Seq.item' index xs |> Result.FromFs


  /// Returns the first element of a sequence.
  [<Extension>]
  static member FirstSafe (xs) = Seq.head' xs |> Result.FromFs


  /// Returns the first element in a sequence that satisfies a specified condition.
  [<Extension>]
  static member FirstSafe (xs, predicate:Func<_,_>) = 
    Seq.find' (FSharpFunc.FromFunc predicate) xs
    |> Result.FromFs

  /// Returns the last element of a sequence.
  [<Extension>]
  static member LastSafe (xs) = Seq.last' xs |> Result.FromFs
 

  /// Returns the last element in a sequence that satisfies a specified condition.
  [<Extension>]
  static member LastSafe (xs, predicate:Func<_,_>) = 
    Seq.findBack' (FSharpFunc.FromFunc predicate) xs
    |> Result.FromFs

  /// Returns the maximum value in a sequence of values.
  [<Extension>]
  static member MaxSafe (xs:int seq) = Seq.max' xs |> Result.FromFs


  /// Returns the maximum value in a sequence of values.
  [<Extension>]
  static member MaxSafe (xs:float seq) = Seq.max' xs |> Result.FromFs


  /// Invokes a transform function on each element of a sequence and returns the maximum value.
  [<Extension>]
  static member MaxSafe (xs, transform:Func<_,float>) =
    Seq.maxBy' (FSharpFunc.FromFunc transform) xs  
    |> Result.FromFs

  /// Invokes a transform function on each element of a sequence and returns the maximum value.
  [<Extension>]
  static member MaxSafe (xs, transform:Func<_,int>) =
    Seq.maxBy' (FSharpFunc.FromFunc transform) xs  
    |> Result.FromFs

  /// Returns the minimum value in a sequence of values.
  [<Extension>]
  static member MinSafe (xs:int seq) = Seq.min' xs |> Result.FromFs


  /// Returns the minimum value in a sequence of values.
  [<Extension>]
  static member MinSafe (xs:float seq) = Seq.min' xs |> Result.FromFs


  /// Invokes a transform function on each element of a sequence and returns the minimum value.
  [<Extension>]
  static member MinSafe (xs, transform:Func<_,float>) =
    Seq.minBy' (FSharpFunc.FromFunc transform) xs  
    |> Result.FromFs

  /// Invokes a transform function on each element of a sequence and returns the minimum value.
  [<Extension>]
  static member MinSafe (xs, transform:Func<_,int>) =
    Seq.minBy' (FSharpFunc.FromFunc transform) xs  
    |> Result.FromFs

  /// Returns the only element of a sequence
  [<Extension>]
  static member SingleSafe (xs) = Seq.exactlyOne' xs |> Result.FromFs


  /// Returns the only element of a sequence that satisfies a specified condition.
  [<Extension>]
  static member SingleSafe (xs, predicate:Func<_, bool>) = 
    Seq.filter (FSharpFunc.FromFunc predicate) xs
    |> Seq.exactlyOne'
    |> Result.FromFs