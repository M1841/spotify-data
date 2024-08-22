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

  let WriteJson<'T> filePath (item: 'T) =
    async {
      try
        let json = JsonSerializer.Serialize(item)
        do! File.WriteAllTextAsync(filePath, json) |> Async.AwaitTask
      with
      | :? IOException as ex -> printfn "I/O error: %s" ex.Message
      | :? JsonException as ex -> printfn "JSON error: %s" ex.Message
      | ex -> printfn "Unexpected error: %s" ex.Message
    }

  let WriteSongsTable (filePath: string) (songs: Song list) =
    let maxTitleLength =
      songs
      |> List.maxBy (fun song -> song.SongName.Length)
      |> fun song -> song.SongName.Length

    let maxArtistLength =
      songs
      |> List.maxBy (fun song -> song.ArtistName.Length)
      |> fun song -> song.ArtistName.Length

    use writer = new StreamWriter(filePath)

    let paddedHeader1 = "Timestamp".PadRight 16
    let paddedHeader2 = "Artist".PadRight maxArtistLength
    let paddedHeader3 = "Song".PadRight maxTitleLength
    writer.WriteLine(sprintf "%s | %s | %s" paddedHeader1 paddedHeader2 paddedHeader3)

    let separator1 = String.replicate 16 "-"
    let separator2 = String.replicate maxArtistLength "-"
    let separator3 = String.replicate maxTitleLength "-"
    writer.WriteLine(sprintf "%s | %s | %s" separator1 separator2 separator3)

    try
      for song in songs do
        let timestampString =
          sprintf
            "%04d/%02d/%02d %02d:%02d"
            song.Timestamp.Year
            song.Timestamp.Month
            song.Timestamp.Day
            song.Timestamp.Hour
            song.Timestamp.Minute

        let paddedTitle = song.SongName.PadRight maxTitleLength
        let paddedArtist = song.ArtistName.PadRight maxArtistLength

        // printfn "%s - %s" song.ArtistName song.SongName

        writer.WriteLine(sprintf "%s | %s | %s" timestampString paddedArtist paddedTitle)
    with ex ->
      printfn "Unexpected error: %s" ex.Message

  let WriteEpisodesTable (filePath: string) (episodes: Episode list) =
    let maxEpisodeNameLength =
      episodes
      |> List.maxBy (fun e -> e.EpisodeName.Length)
      |> fun e -> e.EpisodeName.Length

    let maxShowNameLength =
      episodes
      |> List.maxBy (fun e -> e.ShowName.Length)
      |> fun e -> e.ShowName.Length

    use writer = new StreamWriter(filePath)

    let paddedHeader1 = "Timestamp".PadRight 16
    let paddedHeader2 = "Show".PadRight maxShowNameLength
    let paddedHeader3 = "Episode".PadRight maxEpisodeNameLength
    writer.WriteLine(sprintf "%s | %s | %s" paddedHeader1 paddedHeader2 paddedHeader3)

    let separator1 = String.replicate 16 "-"
    let separator2 = String.replicate maxShowNameLength "-"
    let separator3 = String.replicate maxEpisodeNameLength "-"
    writer.WriteLine(sprintf "%s | %s | %s" separator1 separator2 separator3)

    try
      for episode in episodes do
        let timestampString =
          sprintf
            "%04d/%02d/%02d %02d:%02d"
            episode.Timestamp.Year
            episode.Timestamp.Month
            episode.Timestamp.Day
            episode.Timestamp.Hour
            episode.Timestamp.Minute

        let paddedEpisodeName = episode.EpisodeName.PadRight(maxEpisodeNameLength)
        let paddedShowName = episode.ShowName.PadRight(maxShowNameLength)

        // printfn "%s - %s" episode.ShowName episode.EpisodeName

        writer.WriteLine(sprintf "%s | %s | %s" timestampString paddedShowName paddedEpisodeName)
    with ex ->
      printfn "Unexpected error: %s" ex.Message
