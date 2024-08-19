namespace SpotifyData

open System

module Conversion =
  let ConvertEntry entry =
    async {
      match entry.SongName, entry.ArtistName, entry.AlbumName with
      | Some songName, Some artistName, Some albumName ->
        let song =
          { Timestamp = DateTime.Parse(entry.Timestamp)
            MsPlayed = entry.MsPlayed
            SongName = songName
            ArtistName = artistName
            AlbumName = albumName
            SpotifyUri = entry.SpotifyTrackUri
            Offline = entry.Offline }

        return Some(Song song)
      | _ ->
        match entry.EpisodeName, entry.ShowName with
        | Some episodeName, Some showName ->
          let episode =
            { Timestamp = DateTime.Parse(entry.Timestamp)
              MsPlayed = entry.MsPlayed
              EpisodeName = episodeName
              ShowName = showName
              SpotifyUri = entry.SpotifyEpisodeUri }

          return Some(Episode episode)
        | _ -> return None
    }

  let ConvertEntries (entries: RawEntry[]) =
    async {
      let! results = entries |> Array.map ConvertEntry |> Async.Parallel

      let songs, episodes =
        results
        |> Array.fold
          (fun (songs, episodes) result ->
            match result with
            | Some(Song song) -> song :: songs, episodes
            | Some(Episode episode) -> songs, episode :: episodes
            | None -> songs, episodes)
          ([], [])

      return songs, episodes
    }
