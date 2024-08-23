open SpotifyData
open SpotifyData.IOUtils
open SpotifyData.Conversion

let GetItemsRaw =
  let filePaths = [ for i in 0..13 -> sprintf "data/raw/endsong_%d.json" i ]

  let rawItems =
    filePaths
    |> List.map ReadJson<RawItem[]>
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.choose id
    |> Array.concat

  ConvertItems rawItems

let WriteItemsJson (songs, episodes) =
  [| async { do! songs |> WriteJson "data/extracted/songs.json" }
     async { do! episodes |> WriteJson "data/extracted/episodes.json" } |]
  |> Async.Parallel
  |> Async.RunSynchronously

let WriteItemsTable ((songs: Song list), (episodes: Episode list)) =
  [ async {
      songs
      |> List.filter (fun s -> s.MsPlayed > uint32 30000)
      |> List.sortBy (fun s -> s.Timestamp)
      |> WriteSongsTable "data/tables/songs_table.txt"
    }
    async {
      episodes
      |> List.filter (fun e -> e.MsPlayed > uint32 3000)
      |> List.sortBy (fun e -> e.Timestamp)
      |> WriteEpisodesTable "data/tables/episodes_table.txt"
    } ]
  |> Async.Parallel
  |> Async.RunSynchronously

[<EntryPoint>]
let Main _ =
  async {
    let! songs, episodes = GetItemsRaw
    (songs, episodes) |> WriteItemsJson |> ignore

    (songs, episodes) |> WriteItemsTable |> ignore
    return 0
  }
  |> Async.RunSynchronously

// TODO:
// - raw entries should be accessed and converted only the first time, then the work should only be done on the resulting songs and episodes
// - songs and episodes should be filter-able, sort-able and search-able, group-able
//// - filter by: timestamp range, play duration, cumulative play duration, cumulative play count
//// - sort by: timestamp, title/name, artist, album/show
//// - search by: title/name, artist, album/show
//// - group by: UID and accumulate play duration and play count
// - should generate song/episode, artist, album/show leaderboards by play duration or play count for any timestamp range
// - everything should be structured in a modular manner in reusable functions for possible future projects, such as a web service
