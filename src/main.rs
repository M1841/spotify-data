use std::{
  collections::{BTreeSet, HashMap}, 
  error::Error, 
  fs::File, 
  io::BufReader, 
  path::Path
};
use chrono::{DateTime, Utc};
use serde_derive::{Deserialize, Serialize};

fn main() {
  let file_paths: Vec<String> = (0..14).map(|i| format!("data/endsong_{}.json", i)).collect();
  match read_all_files(file_paths) {
    Ok(history) => {
      let (mut songs, episodes) = history;

      songs.sort_by(|a, b| a.timestamp.cmp(&b.timestamp));
      
      let mut final_songs = Vec::<Song>::new();
      for song in &songs {
        if let Some(position) = final_songs.iter().position(|s| s.spotify_uri == song.spotify_uri) {
          final_songs[position].ms_played += song.ms_played;
        } else {
          final_songs.push(song.clone());
        }
      }
      final_songs.retain(|song| song.ms_played > 10 * 60 * 1000);
      final_songs.sort_by(|a, b| b.timestamp.cmp(&a.timestamp));
      let songs = final_songs;

      for song in &songs {
        println!("{:?}", song);
      }
      println!("{}", songs.len());
    },
    Err(err) => eprintln!("{}", err)
  }
}

#[derive(Clone, Debug, Ord, PartialOrd)]
struct Song {
  timestamp: DateTime<Utc>,
  ms_played: u32,
  spotify_uri: String,
  song_name: String,
  artist_name: String,
  album_name: String,
  offline: bool
}

impl PartialEq for Song {
  fn eq(&self, other: &Self) -> bool {
    self.spotify_uri == other.spotify_uri
  }
}
impl Eq for Song {}

#[derive(Clone, Debug)]
struct Episode {
  timestamp: DateTime<Utc>,
  ms_played: u32,
  spotify_uri: String,
  episode_name: String,
  show_name: String
}

#[derive(Serialize, Deserialize)]
struct RawEntry {
  ts: String,
  ms_played: u32,
  master_metadata_track_name: Option<String>,
  master_metadata_album_artist_name: Option<String>,
  master_metadata_album_album_name: Option<String>,
  spotify_track_uri: Option<String>,
  episode_name: Option<String>,
  episode_show_name: Option<String>,
  spotify_episode_uri: Option<String>,
  offline: bool
}

fn read_json<P: AsRef<Path>>(path: P) -> Result<Vec<RawEntry>, Box<dyn Error>> {
  let file = File::open(path)?;
  let reader = BufReader::new(file);
  let history: Vec<RawEntry> = serde_json::from_reader(reader)?;
  Ok(history)
}

fn convert_to_song_or_episode(entry: RawEntry) -> Result<(Option<Song>, Option<Episode>), Box<dyn Error>> {
  let timestamp = DateTime::parse_from_rfc3339(&entry.ts)?.with_timezone(&Utc);
  let ms_played = entry.ms_played;
  let spotify_uri = entry.spotify_track_uri.unwrap_or_default();

  if let Some(song_name) = entry.master_metadata_track_name {
    let artist_name = entry.master_metadata_album_artist_name.unwrap_or_default();
    let album_name = entry.master_metadata_album_album_name.unwrap_or_default();
    let offline = entry.offline;
  
    let song = Song {
      timestamp,
      ms_played,
      song_name,
      artist_name,
      album_name,
      spotify_uri,
      offline
    };
    Ok((Some(song), None))
  } else if let Some(episode_name) = entry.episode_name {
    let show_name = entry.episode_show_name.unwrap_or_default();

    let episode = Episode {
      timestamp,
      ms_played,
      episode_name,
      show_name,
      spotify_uri
    };

    Ok((None, Some(episode)))
  } else {
    Ok((None, None))
  }
}

fn read_all_files(file_paths: Vec<String>) -> Result<(Vec<Song>, Vec<Episode>), Box<dyn Error>> {
  let mut songs = Vec::new();
  let mut episodes = Vec::new();

  for path in file_paths {
    let history = read_json(path)?;
    for entry in history {
      let (song, episode) = convert_to_song_or_episode(entry)?;
      if let Some(song) = song {
        songs.push(song);
      } else if let Some(episode) = episode {
        episodes.push(episode);
      }
    }
  }
  Ok((songs, episodes))
}