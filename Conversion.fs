namespace SpotifyData

open System

module Conversion =
  let convertEntry entry =
    async {
      match entry.SongName, entry.ArtistName, entry.AlbumName with
      | Some songName, Some artistName, Some albumName ->
        let song =
          { Timestamp = DateTime.Parse(entry.Timestamp)
            MsPlayed = entry.MsPlayed
            SpotifyUri = entry.SpotifyTrackUri |> Option.defaultValue ""
            SongName = songName
            ArtistName = artistName
            AlbumName = albumName
            Offline = entry.Offline }

        return Some(Choice1Of2 song)
      | _ ->
        match entry.EpisodeName, entry.ShowName with
        | Some episodeName, Some showName ->
          let episode =
            { Timestamp = DateTime.Parse(entry.Timestamp)
              MsPlayed = entry.MsPlayed
              SpotifyUri = entry.SpotifyEpisodeUri |> Option.defaultValue ""
              EpisodeName = episodeName
              ShowName = showName }

          return Some(Choice2Of2 episode)
        | _ -> return None
    }

  let convertEntries (entries: RawEntry[]) =
    async {
      let! results = entries |> Array.map convertEntry |> Async.Parallel

      let songs, episodes =
        results
        |> Array.fold
          (fun (songs, episodes) result ->
            match result with
            | Some(Choice1Of2 song) -> song :: songs, episodes
            | Some(Choice2Of2 episode) -> songs, episode :: episodes
            | None -> songs, episodes)
          ([], [])

      return songs, episodes
    }
