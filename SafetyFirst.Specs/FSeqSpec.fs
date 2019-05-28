module SafetyFirst.Specs.FSeqSpec

open NUnit.Framework
open Swensen.Unquote

open SafetyFirst

[<Test>]
let ``can construct an FSeq with the fseq function`` () =
  let x = FSeq.ofList [ 0 .. 10 ]
  let y = FSeq.ofArray [| 0 .. 10 |]

  test <@ Seq.forall (fun (a, b) -> a = b) (Seq.indexed x) && Seq.forall (fun (a, b) -> a = b) (Seq.indexed y) @>

module StructuralEquality =

  [<Test>]
  let ``can identify unequal fseqs with different lengths`` () =
    let x = FSeq.ofList [0 .. 10]
    let y = FSeq.ofArray [| 0 .. 12 |]
    test <@ x <> y @>

    let x = FSeq.ofList [0 .. 12]
    let y = FSeq.ofArray [| 0 .. 10 |]
    test <@ x <> y @>

    let x = fseq {0 .. 10}
    let y = FSeq.ofSeq {0 .. 12}
    test <@ x <> y @>

    let x = fseq {0 .. 12}
    let y = FSeq.ofSeq {0 .. 10}
    test <@ x <> y @>

  [<Test>]
  let ``can identify unequal fseqs with different elements`` () =
    let x = FSeq.ofList [0 .. 10]
    let y = FSeq.ofArray [| 1 .. 11 |]
    test <@ x <> y @>

    let x = FSeq.ofSeq {0 .. 10}
    let y = FSeq.ofSeq {1 .. 11}
    test <@ x <> y @>

  [<Test>]
  let ``can identify equal fseqs`` () =
    let x = FSeq.ofList [ 0 .. 10 ]
    let y = FSeq.ofArray [| 0 .. 10 |]
    test <@ x = y @>

    let x = FSeq.ofSeq (ResizeArray {0 .. 10})
    let y = FSeq.ofList [0 .. 10]
    test <@ x = y @>

    let x = FSeq.ofSeq {0 .. 10}
    let y = FSeq.ofArray [| 0 .. 10 |]
    test <@ x = y @>

    //same x
    let y = fseq {0 .. 10}
    test <@ x = y @>

    let x = FSeq.ofList [ 0 .. 10 ]
    let y = FSeq.ofArray [| 0 .. 10 |]
    test <@ x = y @>

  type Vector = { Values : float fseq }

  [<Test>]
  let ``can compare objects that contain an fseq`` () =
    let x = { Values = FSeq.ofList [0.0 .. 10.0] } 
    let y = { Values = FSeq.ofList [0.0 .. 10.0] }

    test 
      <@ 
        x = y
        &&
        x.Values = FSeq.ofList [0.0 .. 10.0]
      @>

  [<Test>]
  let ``doesn't change comparison value when the length is calculated`` () =
    let x = FSeq.ofSeq {0 .. 10}
    let y = FSeq.ofSeq {1 .. 9}

    let initialCompare = compare x y
    ignore x.Length
    ignore y.Length
    let finalCompare = compare x y

    test <@ initialCompare = finalCompare @>


[<NoComparison>]
type JustEquatable = JustEquatable of int

[<Test>]
let ``can create an fseq of non-comparable elements, and can still check equality, but can't compare`` () = 
  let xs = fseq [ JustEquatable 1; JustEquatable 2 ]
  let ys = fseq [ JustEquatable 1; JustEquatable 2 ]
  let zs = fseq [ JustEquatable 2; JustEquatable 1 ]

  test 
    <@
      xs = ys
      && 
      xs <> zs
      // line below should fail to compile because they aren't comparable
      // && compare xs ys = 0
    @>

[<Test>]
let ``can create an fseq of non-equatable elements, but can't compare or check equality``() = 
  let xs = fseq [ ((+) 5); ((*) 2) ]
  let ys = fseq [ ((+) 5); ((*) 2) ]
  let zs = fseq [ ((*) 2); ((+) 5) ]

  // line below should fail both to equate and compare
  // test <@ xs = ys && compare xs ys @>
  ()
  
[<Test>]
let ``wrapping a seq in an fseq is lazy`` () =
  let mutable elementsCalculated = 0;
  let xs = seq {
    for i in 0 .. 10 ->
      elementsCalculated <- elementsCalculated + 1
      i
  }

  let xs : int fseq = FSeq.ofSeq xs

  let sumOfFirstThree = xs |> Seq.take 3 |> Seq.sum
  test <@ elementsCalculated = 3 @>

let oneToFive = FSeq.ofList [1 .. 5]

let is<'a> (x:'a) = true

[<Test>]
let ``mapping a finite sequence returns a finite sequence`` () = 
  let xs = FSeq.map ((+) 3) oneToFive

  test 
    <@ 
      xs |> is<int fseq> 
      && 
      xs = (FSeq.ofList [4 .. 8]) 
    @>

[<Test>]
let ``mapi-ing a finite sequence returns a finite sequence`` () = 
  let xs = FSeq.mapi (+) oneToFive

  test 
    <@ 
      xs |> is<int fseq> 
      && 
      xs = (FSeq.ofList [1;3;5;7;9]) 
    @>

[<Test>]
let ``filtering a finite sequence returns a finite sequence`` () = 
  let xs = FSeq.filter (fun x -> x > 2) oneToFive

  test 
    <@ 
      xs |> is<int fseq>
      &&
      xs = (FSeq.ofList [3 .. 5])
    @>

[<Test>]
let ``appending two finite sequences returns another finite sequence`` () = 
  let xs = oneToFive
  let ys = FSeq.ofList [6 .. 10]
  let zs = FSeq.append xs ys

  test 
    <@
      zs |> is<int fseq>
      &&
      zs = (FSeq.ofList [1 .. 10])
    @>

#nowarn "10101"

open System

[<Test>]
let ``taking the tail of a finite sequence returns a finite sequence`` () =
  let xs = FSeq.tryTail oneToFive

  test 
    <@
      xs |> is<int fseq option>
      &&
      xs = (Some (FSeq.ofList [2 .. 5]))
    @>

[<Test>]
let ``(code coverage test) uses a predefined length if available on many different seq types`` () =
  test
    <@
      (FSeq.ofList [1 .. 10]).Length = 10
      &&
      (FSeq.ofSeq {1 .. 10}).Length = 10
      &&
      (FSeq.ofArray [|1 .. 10|]).Length = 10
      &&
      (FSeq.ofSeq (ResizeArray {1 .. 10})).Length = 10
    @>

[<Test>]
let ``can be used as a Map key (proper hashing and equality)`` () =
  let data = 
    Map.ofList [
      (FSeq.ofList [1 .. 3], "oneToThree")
      (FSeq.ofList [4 .. 6], "fourToSix")
      (FSeq.ofList [7 .. 9], "sevenToNine")
    ]

  test 
    <@
      data.[FSeq.ofList [1 .. 3]] = "oneToThree"
      &&
      data.[FSeq.ofList [4 .. 6]] = "fourToSix"
      &&
      data.[FSeq.ofList [7 .. 9]] = "sevenToNine"
    @>

[<Test>]
let ``doesn't allow comparison or equality with other types`` () =
  test <@ not ((FSeq.ofList [1 .. 3]).Equals("hello")) @>

  raises <@ ((FSeq.ofList [1 .. 3]) :> IComparable).CompareTo("hello") @>

module Splitting = 
  let ofNonEmpty (xs:NonEmptyFSeq<NonEmptyFSeq<int>>) = 
    FSeq.NonEmpty.toList <| FSeq.NonEmpty.map FSeq.NonEmpty.toList xs

  let toNonEmpty xs = 
    FSeq.NonEmpty.ofFSeq' (fseq xs) |> Result.expect

  [<Test>]
  let ``returns what the documentation says`` () =

    test 
      <@
        (FSeq.NonEmpty.split ((=) 100) (toNonEmpty [1;2;3;100;100;4;100;5;6]) |> ofNonEmpty)
          = [[1;2;3;100];[100];[4;100];[5;6]]

        &&

        (FSeq.NonEmpty.splitPairwise (=) (toNonEmpty [0;1;1;2;3;4;4;4;5]) |> ofNonEmpty)
          = [[0;1];[1;2;3;4];[4];[4;5]]
      @>
  
  [<Test>]
  let ``splits properly for multiple types of inputs`` () = 
    test 
      <@
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [0]) |> ofNonEmpty) = [[0]]
        &&
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [5]) |> ofNonEmpty) = [[5]]
        &&
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [0;5]) |> ofNonEmpty) = [[0; 5]]
        &&
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [5;5]) |> ofNonEmpty) = [[5]; [5]]
        &&
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [5;0]) |> ofNonEmpty) = [[5]; [0]]
        &&
        (FSeq.NonEmpty.split ((=) 5) (toNonEmpty [5;0;0;5;5;0;5]) |> ofNonEmpty) = [[5]; [0;0;5]; [5]; [0;5]]
      @>

  [<Test>]
  let ``splits pairwise properly for multiple types of inputs`` () = 
    let bigDiff i j = abs (i - j) > 5
    test 
      <@
        (FSeq.NonEmpty.splitPairwise (=) (toNonEmpty [0]) |> ofNonEmpty) = [[0]]
        &&
        (FSeq.NonEmpty.splitPairwise (=) (toNonEmpty [0;1]) |> ofNonEmpty) = [[0;1]]
        &&
        (FSeq.NonEmpty.splitPairwise (=) (toNonEmpty [0;0]) |> ofNonEmpty) = [[0]; [0]]
        &&
        (FSeq.NonEmpty.splitPairwise (bigDiff) (toNonEmpty [1;2;12;13;23;24]) |> ofNonEmpty)
          = [[1;2]; [12;13]; [23;24]]
        &&
        (FSeq.NonEmpty.splitPairwise (bigDiff) (toNonEmpty [1;2;12;13;23]) |> ofNonEmpty)
          = [[1;2]; [12;13]; [23]]
      @>





