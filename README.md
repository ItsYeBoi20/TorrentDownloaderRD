# TorrentDownloaderRD

**Torrent Searcher and Downloader using the Real-Debrid API**

## Features

### Settings
- **Settings.txt** is automatically created wherever the .exe is stored.
- **API Key**: Enter your Real-Debrid API Key here.
- **Website Selection**: Choose which website you want to search.
- **Pages to Search**: The number of pages to search applies to each enabled provider separately.
- **Remove Torrent**: Option to remove the torrent from Real-Debrid after download.

### Viewing and Managing Torrents
- **View Saved Torrents**: Displays all added Real-Debrid torrents (won't be added if "Remove Torrent" is enabled).
  - **Double Click**: Shows download links which can be copied to the clipboard.

### Filtering and Sorting
- **Filter Textbox**: Applies only after the initial search.
- **Min Seeders Dropdown**: Applies only after the initial search.
- **Content and Sort By Dropdowns**: Affect the website searching.

### Downloading Torrents
- **Open Torrent**: Double click on any item to open it.
- **Fetch Download Links**: Click to add all download links and copy them to the clipboard (useful for download managers like JDownloader).
- **Download Button**: Downloads the files using the internal downloader once the listbox is filled.

### Supported Providers
- **[1337x](https://1337x.to/)**
- **[Lime Torrents](https://www.limetorrents.lol/)**
- **[Nyaa.si](https://nyaa.si/)**
- **[Pirate Bay](https://thepiratebay.org/)**
- **[TorLock](https://www.torlock2.com/)**
- **[Torrent Project](https://torrentproject.cc/)**
- **[Torrents-CSV](https://torrents-csv.com/)**
- **[Torrent Download](https://www.torrentdownload.info/)**
- **[YourBittorrent](https://yourbittorrent.com/)**
- **[Torrent Galaxy](https://torrentgalaxy.to/)**
- **[BitSearch](https://bitsearch.to/)**
- **[TheRarbg](https://therarbg.com/)**
- **[FitGirl](https://fitgirl-repacks.site/)**

### Known Issues
- **Nyaa.si**: Repeated attempts to open a specific file may result in temporary server blocks. Wait a few seconds and try again. This occurs only when trying to open the same file multiple times in quick succession.
