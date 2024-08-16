open System
open System.IO
open SpotifyData
open SpotifyData.JsonUtils
open SpotifyData.Conversion

[<EntryPoint>]
let main argv =
  async {
    let filePaths = [ for i in 0..13 -> sprintf "./data/endsong_%d.json" i ]

    let rawEntries =
      filePaths
      |> List.map readJson
      |> Async.Parallel
      |> Async.RunSynchronously
      |> Array.concat

    let! songs, episodes = convertEntries rawEntries

    let sortedSongs =
      songs
      |> List.filter (fun song -> song.MsPlayed > uint32 (30000))
      |> List.sortBy (fun song -> song.Timestamp)

    let maxTitleLength =
      sortedSongs
      |> List.maxBy (fun song -> song.SongName.Length)
      |> fun song -> song.SongName.Length

    let maxArtistLength =
      sortedSongs
      |> List.maxBy (fun song -> song.ArtistName.Length)
      |> fun song -> song.ArtistName.Length

    use songWriter = new StreamWriter("songs.txt")

    for song in sortedSongs do
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

      printfn "%s | %s | %s" timestampString paddedTitle paddedArtist
      songWriter.WriteLine(sprintf "%s | %s | %s" timestampString paddedTitle paddedArtist)

    return 0
  }
  |> Async.RunSynchronously
