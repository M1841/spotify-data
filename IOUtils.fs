namespace SpotifyData

open System.IO
open System.Text.Json

module IOUtils =
  let ReadJson<'T> filePath =
    async {
      try
        let! json = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
        return Some(JsonSerializer.Deserialize<'T>(json))
      with
      | :? IOException as ex ->
        printfn "I/O error: %s" ex.Message
        return None
      | :? JsonException as ex ->
        printfn "JSON error: %s" ex.Message
        return None
      | ex ->
        printfn "Unexpected error: %s" ex.Message
        return None
    }

  let WriteSongsToFile (filePath: string) (songs: Song list) =
    let maxTitleLength =
      songs
      |> List.maxBy (fun song -> song.SongName.Length)
      |> fun song -> song.SongName.Length

    let maxArtistLength =
      songs
      |> List.maxBy (fun song -> song.ArtistName.Length)
      |> fun song -> song.ArtistName.Length

    try
      use songWriter = new StreamWriter(filePath)

      for song in songs do
        let timestampString =
          sprintf
            "%04d/%02d/%02d %02d:%02d"
            song.Timestamp.Year
            song.Timestamp.Month
            song.Timestamp.Day
            song.Timestamp.Hour
            song.Timestamp.Minute

        let paddedTitle = song.SongName.PadRight(maxTitleLength)
        let paddedArtist = song.ArtistName.PadRight(maxArtistLength)

        printfn "%s | %s | %s" timestampString paddedArtist paddedTitle
        songWriter.WriteLine(sprintf "%s | %s | %s" timestampString paddedArtist paddedTitle)
    with ex ->
      printfn "Unexpected error: %s" ex.Message
