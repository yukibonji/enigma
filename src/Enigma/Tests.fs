﻿module Enigma.Tests

open Components
open Xunit
open FsCheck
open FsCheck.Xunit
open Swensen.Unquote
open System

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message that only needs the right rotor to advance``() =
    (defaultEnigma |> withWheelPositions 'A' 'B' 'C')
    |> translate "AEFAEJXXBNXYJTY"
    =! "CONGRATULATIONS"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message with rotor turnover``() =
    (defaultEnigma |> withWheelPositions 'A' 'B' 'R')
    |> translate "MABE KGZXSG"
    =! "TURN MIDDLE"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message with double stepping``() =
    (defaultEnigma |> withWheelPositions 'A' 'D' 'S')
    |> translate "RZFOG FYHPL"
    =! "TURNS THREE"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message with ring settings``() =
    (defaultEnigma |> withWheelPositions 'X' 'Y' 'Z'
                   |> withRingSettings 'J' 'N' 'U')
    |> translate "QKTP EBZIUK"
    =! "GOOD RESULT"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message with custom rotors``() =
    (defaultEnigma |> withRotors Rotor2 Rotor3 Rotor1
                   |> withWheelPositions 'X' 'Y' 'Z'
                   |> withRingSettings 'J' 'N' 'U')
    |> translate "WMUOMJ YRLFLA"
    =! "CUSTOM ROTORS"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Should translate a message with a plugboard``() =
    (defaultEnigma |> withWheelPositions 'V' 'Q' 'Q'
                   |> withRingSettings 'J' 'N' 'U'
                   |> withPlugBoard "AP BR CM FZ GJ IL NT OV QS WX")
    |> translate "HABHV HL YDFN ADZY"
    =! "THATS IT WELL DONE"


[<Fact>]
[<Trait("", "Unit Test")>]
let ``Enigma Instruction Manual 1930``() =
    ({ defaultEnigma
        with Reflector = ReflectorA
             Left = Rotor2
             Middle = Rotor1
             Right = Rotor3 }
        |> withWheelPositions 'A' 'B' 'L'
        |> withRingSettings 'X' 'M' 'V'
        |> withPlugBoard "AM FI NV PS TU WZ")
    |> translate "GCDSE AHUGW TQGRK VLFGX UCALX VYMIG MMNMF DXTGN VHVRM MEVOU YFZSL RHDRR XFJWC FHUHM UNZEF RDISI KBGPM YVXUZ"
    =! "FEIND LIQEI NFANT ERIEK OLONN EBEOB AQTET XANFA NGSUE DAUSG ANGBA ERWAL DEXEN DEDRE IKMOS TWAER TSNEU STADT"

[<Fact>]
[<Trait("", "Unit Test")>]
let ``Operation Barbarossa Part 1``() =
    ({ defaultEnigma
          with Reflector = ReflectorB
               Left = Rotor2
               Middle = Rotor4
               Right = Rotor5 }
      |> withWheelPositions 'B' 'L' 'A'
      |> withRingSettings 'B' 'U' 'L'
      |> withPlugBoard "AV BS CG DL FU HZ IN KM OW RX")
    |> translate "EDPUD NRGYS ZRCXN UYTPO MRMBO FKTBZ REZKM LXLVE FGUEY SIOZV EQMIK UBPMM YLKLT TDEIS MDICA GYKUA CTCDO MOHWX MUUIA UBSTS LRNBZ SZWNR FXWFY SSXJZ VIJHI DISHP RKLKA YUPAD TXQSP INQMA TLPIF SVKDA SCTAC DPBOP VHJK"
    =! "AUFKL XABTE ILUNG XVONX KURTI NOWAX KURTI NOWAX NORDW ESTLX SEBEZ XSEBE ZXUAF FLIEG ERSTR ASZER IQTUN GXDUB ROWKI XDUBR OWKIX OPOTS CHKAX OPOTS CHKAX UMXEI NSAQT DREIN ULLXU HRANG ETRET ENXAN GRIFF XINFX RGTX"

let testEnigma =
    { defaultEnigma
         with Reflector = ReflectorB
              Left = Rotor2
              Middle = Rotor4
              Right = Rotor5 }
    |> withWheelPositions 'T' 'E' 'D'
    |> withRingSettings 'A' 'B' 'C'
    |> withPlugBoard "AB VS DG CL HU FZ KN IM RW OX"
let testTranslate text = testEnigma |> translate text

type ValidTextGen() =
    static member private isValidChar = fun c -> Char.IsLetter c && Char.IsUpper c
    static member GenerateString() = 
        Arb.Default.String()
        |> Arb.filter(function
            | null | "" -> false
            | text when text |> Seq.forall ValidTextGen.isValidChar -> true
            | _ -> false)

    static member GenerateChar() =
        Arb.Default.Char()
        |> Arb.filter ValidTextGen.isValidChar

[<Property(Verbose = true, Arbitrary = [| typeof<ValidTextGen> |])>]
[<Trait("", "Property-Based Test")>]
let ``Encrypting and decrypting text always gives the same text`` text =
    testTranslate >> testTranslate <| text = text.ToUpper()

[<Property(Verbose = true, Arbitrary = [| typeof<ValidTextGen> |])>]
[<Trait("", "Property-Based Test")>]
let ``Encrypted and decrypted text are never the same``(text) =
    (testTranslate text) <> text.ToUpper()

[<Property(Verbose = true, Arbitrary = [| typeof<ValidTextGen> |])>]
[<Trait("", "Property-Based Test")>]
let ``Encrypted and decrypted text are always the same length``(text) =
    (testTranslate text).Length = text.Length

[<Property(Verbose = true, Arbitrary = [| typeof<ValidTextGen> |])>]
[<Trait("", "Property-Based Test")>]
let ``Encrypting the same character multiple times produces different results``(letter:char) =
    Char.IsLetter letter ==> lazy
        (String(Array.init 5 (fun _ -> letter)))
        |> testTranslate 
        |> Seq.distinct
        |> Seq.length > 1