namespace SpotifyData

open System

module Format =
  let FormatLine items =
    items |> List.map (fun (s: string, l) -> s.PadRight l) |> String.concat " | "

  let FormatTimestamp (timestamp: DateTime) =
    sprintf "%04d/%02d/%02d %02d:%02d" timestamp.Year timestamp.Month timestamp.Day timestamp.Hour timestamp.Minute
