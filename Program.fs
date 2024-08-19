open SpotifyData
open SpotifyData.IOUtils
open SpotifyData.Conversion

[<EntryPoint>]
let main _ =
  async {
    let filePaths = [ for i in 0..13 -> sprintf "./data/endsong_%d.json" i ]

    let rawEntries =
      filePaths
      |> List.map ReadJson<RawEntry[]>
      |> Async.Parallel
      |> Async.RunSynchronously
      |> Array.choose id
      |> Array.concat

    let! songs, _ = ConvertEntries rawEntries

    songs
    |> List.filter (fun song -> song.MsPlayed > uint32 (30000))
    |> List.sortBy (fun song -> song.Timestamp)
    |> WriteSongsToFile "songs.txt"

    return 0
  }
  |> Async.RunSynchronously
