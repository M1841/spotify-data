namespace SpotifyData

open System.IO
open System.Text.Json

module JsonUtils =
  let readJson filePath =
    async {
      try
        let! json = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
        return JsonSerializer.Deserialize<RawEntry[]>(json)
      with
      | :? IOException as ex ->
        printfn "I/O error: %s" ex.Message
        return [||]
      | :? JsonException as ex ->
        printfn "JSON error: %s" ex.Message
        return [||]
      | ex ->
        printfn "Unexpected error: %s" ex.Message
        return [||]
    }
