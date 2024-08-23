namespace SpotifyData

open System.IO
open System.Text.Json
open SpotifyData.Format

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
    use writer = new StreamWriter(filePath)

    let maxArtistNameLength, maxSongNameLength =
      songs
      |> List.fold (fun (mANL, mSNL) s -> (max mANL s.ArtistName.Length, max mSNL s.SongName.Length)) (0, 0)

    [ ("Timestamp", 16)
      ("Artist", maxArtistNameLength)
      ("Song", maxSongNameLength) ]
    |> FormatLine
    |> writer.WriteLine

    [ 16; maxArtistNameLength; maxSongNameLength ]
    |> List.map (fun l -> String.replicate l "-")
    |> String.concat " | "
    |> writer.WriteLine

    try
      songs
      |> List.iter (fun song ->
        let timestampStr = song.Timestamp |> FormatTimestamp

        [ (timestampStr, 16)
          (song.ArtistName, maxArtistNameLength)
          (song.SongName, maxSongNameLength) ]
        |> FormatLine
        |> writer.WriteLine)
    with
    | :? IOException as ex -> printfn "I/O error: %s" ex.Message
    | ex -> printfn "Unexpected error: %s" ex.Message

  let WriteEpisodesTable (filePath: string) (episodes: Episode list) =
    use writer = new StreamWriter(filePath)

    let maxShowNameLength, maxEpisodeNameLength =
      episodes
      |> List.fold (fun (mSNL, mENL) e -> (max mSNL e.ShowName.Length, max mENL e.EpisodeName.Length)) (0, 0)

    [ ("Timestamp", 16)
      ("Show", maxShowNameLength)
      ("Episode", maxEpisodeNameLength) ]
    |> FormatLine
    |> writer.WriteLine

    [ 16; maxShowNameLength; maxEpisodeNameLength ]
    |> List.map (fun l -> String.replicate l "-")
    |> String.concat " | "
    |> writer.WriteLine

    try
      episodes
      |> List.iter (fun episode ->
        let timestampStr = episode.Timestamp |> FormatTimestamp

        [ (timestampStr, 16)
          (episode.ShowName, maxShowNameLength)
          (episode.EpisodeName, maxEpisodeNameLength) ]
        |> FormatLine
        |> writer.WriteLine)
    with
    | :? IOException as ex -> printfn "I/O error: %s" ex.Message
    | ex -> printfn "Unexpected error: %s" ex.Message
