namespace SpotifyData

open System

module Conversion =
  let ConvertItem item =
    async {
      match item.SongName, item.ArtistName, item.AlbumName with
      | Some songName, Some artistName, Some albumName ->
        let song =
          { Timestamp = DateTime.Parse(item.Timestamp)
            MsPlayed = item.MsPlayed
            SongName = songName
            ArtistName = artistName
            AlbumName = albumName
            SpotifyUri = item.SpotifyTrackUri
            Offline = item.Offline }

        return Some(Song song)
      | _ ->
        match item.EpisodeName, item.ShowName with
        | Some episodeName, Some showName ->
          let episode =
            { Timestamp = DateTime.Parse(item.Timestamp)
              MsPlayed = item.MsPlayed
              EpisodeName = episodeName
              ShowName = showName
              SpotifyUri = item.SpotifyEpisodeUri }

          return Some(Episode episode)
        | _ -> return None
    }

  let ConvertItems (items: RawItem[]) =
    async {
      let! results = items |> Array.map ConvertItem |> Async.Parallel

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
