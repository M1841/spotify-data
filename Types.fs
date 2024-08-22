namespace SpotifyData

open System
open System.Text.Json.Serialization

type Song =
  { Timestamp: DateTime
    MsPlayed: uint32
    SongName: string
    ArtistName: string
    AlbumName: string
    SpotifyUri: string option
    Offline: bool }

type Episode =
  { Timestamp: DateTime
    MsPlayed: uint32
    EpisodeName: string
    ShowName: string
    SpotifyUri: string option }

type Item =
  | Song of Song
  | Episode of Episode

type RawItem =
  { [<JsonPropertyName("ts")>]
    Timestamp: string
    [<JsonPropertyName("ms_played")>]
    MsPlayed: uint32
    [<JsonPropertyName("master_metadata_track_name")>]
    SongName: string option
    [<JsonPropertyName("master_metadata_album_artist_name")>]
    ArtistName: string option
    [<JsonPropertyName("master_metadata_album_album_name")>]
    AlbumName: string option
    [<JsonPropertyName("spotify_track_uri")>]
    SpotifyTrackUri: string option
    [<JsonPropertyName("episode_name")>]
    EpisodeName: string option
    [<JsonPropertyName("episode_show_name")>]
    ShowName: string option
    [<JsonPropertyName("spotify_episode_uri")>]
    SpotifyEpisodeUri: string option
    [<JsonPropertyName("offline")>]
    Offline: bool }
